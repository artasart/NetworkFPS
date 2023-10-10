using Framework.Network;
using MEC;
using Protocol;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.PackageManager;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FrameWork.Network
{
    public class NetworkTransform_FPS_DeadReckoning : NetworkComponent
    {
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
            controller = GetComponent<CharacterController>();

            velocity = new();

            if (isMine)
            {
                calculateVelocity = Timing.RunCoroutine(CalculeateVelocity(), nameof(CalculeateVelocity));

                Timing.RunCoroutine(UpdatePosition(), nameof(UpdatePosition));
                Timing.RunCoroutine(UpdateRotation(), nameof(UpdateRotation));
            }
            else
            {
                client.packetHandler.AddHandler(Handle_S_SET_FPS_POSITION);
                client.packetHandler.AddHandler(Handle_S_SET_FPS_ROTATION);

                remoteUpdatePosition = Timing.RunCoroutine(RemoteUpdatePosition());
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (isMine)
            {
                Timing.KillCoroutines(nameof(CalculeateVelocity));
                Timing.KillCoroutines(nameof(UpdatePosition));
                Timing.KillCoroutines(nameof(UpdateRotation));
            }
            else
            {
                client.packetHandler.RemoveHandler(Handle_S_SET_FPS_POSITION);
                client.packetHandler.RemoveHandler(Handle_S_SET_FPS_ROTATION);

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
                            PlayerId = objectId,
                            Timestamp = client.calcuatedServerTime,
                            Position = NetworkUtils.UnityVector3ToProtocolVector3(transform.position),
                            Velocity = NetworkUtils.UnityVector3ToProtocolVector3(velocity)
                        };

                        client.Send(PacketManager.MakeSendBuffer(packet));

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
                            PlayerId = objectId,
                            Rotation = NetworkUtils.UnityVector3ToProtocolVector3(transform.eulerAngles)
                        };

                        client.Send(PacketManager.MakeSendBuffer(packet));

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

        private void Handle_S_SET_FPS_POSITION( S_SET_TRANSFORM packet )
        {
            if (packet.GameObjectId != objectId)
            {
                return;
            }

            float timeGap;

            Vector3 packetPosition = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Position);
            velocity = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Velocity);
            Vector3 predictedPosition;

            timeGap = client.calcuatedServerTime - packet.Timestamp;

            predictedPosition = packetPosition + (velocity * timeGap);

            float distance = Vector3.Distance(predictedPosition, transform.position);

            if (distance > hardsnapThreshold)
            {
                controller.Move(predictedPosition - transform.position);
            }

            else
            {
                timeGap = client.calcuatedServerTime - packet.Timestamp + (interval * 1000);

                predictedPosition = packetPosition + (velocity * timeGap);

                Timing.KillCoroutines(remoteUpdatePosition);

                remoteUpdatePosition = Timing.RunCoroutine(LerpPosition(predictedPosition, interval * 1000));
            }
        }

        private IEnumerator<float> LerpPosition( Vector3 position, float totalTime )
        {
            Vector3 prevPosition = transform.position;

            float delaTime = 0.0f;

            do
            {
                delaTime += Time.deltaTime * 1000;
                
                controller.Move(Vector3.Lerp(prevPosition, position, Math.Min(delaTime / totalTime, 1f)) - transform.position);
                
                yield return Timing.WaitForOneFrame;

            } while (delaTime <= totalTime);

            remoteUpdatePosition = Timing.RunCoroutine(RemoteUpdatePosition());
        }

        void Handle_S_SET_FPS_ROTATION( Protocol.S_SET_FPS_ROTATION packet)
        {
            if (packet.PlayerId != objectId)
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

        private IEnumerator<float> RemoteUpdateRotation( Quaternion endRotation )
        {
            float delTime = 0.0f;

            Quaternion startRotation = transform.rotation;

            while (delTime < interval)
            {
                delTime += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, delTime / interval);
                yield return Timing.WaitForOneFrame;
            }
        }
    }
}