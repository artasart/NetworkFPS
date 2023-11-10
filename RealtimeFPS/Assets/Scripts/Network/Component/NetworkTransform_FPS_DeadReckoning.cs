using Framework.Network;
using MEC;
using Protocol;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FrameWork.Network
{
	public class NetworkTransform_FPS_DeadReckoning : MonoBehaviour
	{
		NetworkObject networkObject;

		private readonly float interval = 0.05f;
		private readonly float hardsnapThreshold = 3f;

		private Vector3 velocity;

		private CoroutineHandle updatePosition;
		private CoroutineHandle calculateVelocity;

		private CoroutineHandle remoteUpdatePosition;
		private CoroutineHandle remoteUpdateRotation;

		private CharacterController controller;

		protected void Start()
		{
            networkObject = GetComponent<NetworkObject>();

			controller = GetComponent<CharacterController>();

			velocity = new();

			if (networkObject.isMine)
			{
				Timing.RunCoroutine(CalculeateVelocity(), nameof(CalculeateVelocity) + this.GetHashCode());
				Timing.RunCoroutine(UpdatePosition(), nameof(UpdatePosition) + this.GetHashCode());
				Timing.RunCoroutine(UpdateRotation(), nameof(UpdateRotation) + this.GetHashCode());
			}
			else
			{
				networkObject.Client.packetHandler.AddHandler(Handle_S_SET_FPS_POSITION);
				networkObject.Client.packetHandler.AddHandler(Handle_S_SET_FPS_ROTATION);

                remoteUpdatePosition = Timing.RunCoroutine(RemoteUpdatePosition());
			}
		}

        protected void OnDestroy()
		{
			if (networkObject.isMine)
			{
				Timing.KillCoroutines(nameof(CalculeateVelocity) + this.GetHashCode());
				Timing.KillCoroutines(nameof(UpdatePosition) + this.GetHashCode());
				Timing.KillCoroutines(nameof(UpdateRotation) + this.GetHashCode());
			}
			else
			{
				networkObject.Client.packetHandler.RemoveHandler(Handle_S_SET_FPS_POSITION);
				networkObject.Client.packetHandler.RemoveHandler(Handle_S_SET_FPS_ROTATION);

				Timing.KillCoroutines(remoteUpdatePosition);
			}
		}

		private IEnumerator<float> CalculeateVelocity()
		{
			Vector3 prevPos = transform.position;
			Vector3 currentPos;
			float prevDeltaTime = Time.deltaTime;

			while (true)
			{
				currentPos = transform.position;
				velocity = (currentPos - prevPos) / (prevDeltaTime * 1000);

				prevPos = currentPos;
				prevDeltaTime = Time.deltaTime;

				yield return Timing.WaitForOneFrame;
			}
		}

		private IEnumerator<float> UpdatePosition()
		{
			Vector3 prevVelocity = velocity;
			float delTime = 0;

			while (true)
			{
				delTime += Time.deltaTime;
				if (delTime > interval)
				{
					delTime -= interval;

					if (prevVelocity != velocity)
					{
						C_SET_FPS_POSITION packet = new()
						{
							PlayerId = networkObject.id,
							Timestamp = networkObject.Client.calcuatedServerTime,
							Position = NetworkUtils.UnityVector3ToProtocolVector3(transform.position),
							Velocity = NetworkUtils.UnityVector3ToProtocolVector3(velocity)
						};

						networkObject.Client.Send(PacketManager.MakeSendBuffer(packet));

						prevVelocity = velocity;
					}
				}

				yield return Timing.WaitForOneFrame;
			}
		}

		private IEnumerator<float> UpdateRotation()
		{
			quaternion prevRotation = transform.rotation;
			float delTime = 0;

			while (true)
			{
				delTime += Time.deltaTime;
				if (delTime > interval)
				{
					delTime -= interval;

					if (prevRotation != transform.rotation)
					{
						C_SET_FPS_ROTATION packet = new()
						{
							PlayerId = networkObject.id,
							Rotation = NetworkUtils.UnityVector3ToProtocolVector3(transform.eulerAngles)
						};

						networkObject.Client.Send(PacketManager.MakeSendBuffer(packet));

						prevRotation = transform.rotation;
					}
				}

				yield return Timing.WaitForOneFrame;
			}
		}

		private IEnumerator<float> RemoteUpdatePosition()
		{
			while (true)
			{
				_ = controller.Move(velocity * Time.deltaTime * 1000);
				yield return Timing.WaitForOneFrame;
			}
		}

		private void Handle_S_SET_FPS_POSITION(S_SET_FPS_POSITION packet)
		{
			if (packet.PlayerId != networkObject.id)
			{
				return;
			}

			float timeGap;

			Vector3 packetPosition = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Position);
			velocity = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Velocity);
			Vector3 predictedPosition;

			timeGap = networkObject.Client.calcuatedServerTime - packet.Timestamp;

			predictedPosition = packetPosition + (velocity * timeGap);

			float distance = Vector3.Distance(predictedPosition, transform.position);

			if (distance > hardsnapThreshold)
			{
				controller.Move(predictedPosition - transform.position);
			}

			else
			{
				timeGap = networkObject.Client.calcuatedServerTime - packet.Timestamp + (interval * 1000);

				predictedPosition = packetPosition + (velocity * timeGap);

				Timing.KillCoroutines(remoteUpdatePosition);

				remoteUpdatePosition = Timing.RunCoroutine(LerpPosition(predictedPosition, interval * 1000));
			}
		}

		private IEnumerator<float> LerpPosition(Vector3 position, float totalTime)
		{
			Vector3 prevPosition = transform.position;

			float delTime = 0.0f;

			do
			{
				delTime += Time.deltaTime * 1000;

				controller.Move(Vector3.Lerp(prevPosition, position, Math.Min(delTime / totalTime, 1f)) - transform.position);

				yield return Timing.WaitForOneFrame;

			} while (delTime <= totalTime);

			remoteUpdatePosition = Timing.RunCoroutine(RemoteUpdatePosition());
		}

		void Handle_S_SET_FPS_ROTATION(Protocol.S_SET_FPS_ROTATION packet)
		{
			if (packet.PlayerId != networkObject.id)
			{
				return;
			}

			if (remoteUpdateRotation.IsRunning)
			{
				_ = Timing.KillCoroutines(remoteUpdateRotation);
			}

			remoteUpdateRotation = Timing.RunCoroutine(
				RemoteUpdateRotation(
						NetworkUtils.ProtocolVector3ToUnityQuaternion(packet.Rotation)
					)
				);
		}

		private IEnumerator<float> RemoteUpdateRotation(Quaternion endRotation)
		{
			float delTime = 0.0f;

			Quaternion startRotation = transform.rotation;

			while (delTime < interval)
			{
				delTime += Time.deltaTime;
				transform.rotation = Quaternion.Lerp(startRotation, endRotation, delTime / interval);
				yield return Timing.WaitForOneFrame;
			}

			transform.rotation = endRotation;
		}
	}
}