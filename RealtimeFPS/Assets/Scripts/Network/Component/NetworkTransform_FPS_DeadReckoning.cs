using Framework.Network;
using MEC;
using Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FrameWork.Network
{
    public class NetworkTransform_FPS_DeadReckoning : NetworkComponent
    {
        private readonly float interval = 0.05f;
        private readonly float hardsnapThreshold = 3f;

        private Vector3 velocity;
        private Vector3 angularVelocity;

        private CoroutineHandle updateTransform;
        private CoroutineHandle updateVelocity;
        private CoroutineHandle updateAngularVelocity;

        private CoroutineHandle remoteUpdatePosition;
        private CoroutineHandle remoteUpdateRotation;

        private CharacterController controller;

        protected void Start()
        {
            controller = GetComponent<CharacterController>();

            velocity = new();
            angularVelocity = new();

            if (isMine)
            {
                updateTransform = Timing.RunCoroutine(UpdateTransform());
                updateVelocity = Timing.RunCoroutine(UpdateVelocity());
                updateAngularVelocity = Timing.RunCoroutine(UpdateAngularVelocity());
            }
            else
            {
                client.packetHandler.AddHandler(S_SET_TRANSFORM);
                remoteUpdatePosition = Timing.RunCoroutine(RemoteUpdatePosition());
                remoteUpdateRotation = Timing.RunCoroutine(RemoteUpdateRotation());
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (isMine)
            {
                _ = Timing.KillCoroutines(updateTransform);
                _ = Timing.KillCoroutines(updateVelocity);
                _ = Timing.KillCoroutines(updateAngularVelocity);
            }
            else
            {
                client.packetHandler.RemoveHandler(S_SET_TRANSFORM);
                _ = Timing.KillCoroutines(remoteUpdatePosition);
                _ = Timing.KillCoroutines(remoteUpdateRotation);
            }
        }

        private IEnumerator<float> UpdateVelocity()
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

        private IEnumerator<float> UpdateAngularVelocity()
        {
            Quaternion prevRot = transform.rotation;
            Quaternion currentRot;
            float prevDeltaTime = Time.deltaTime;

            while (true)
            {
                currentRot = transform.rotation;

                Quaternion deltaRotation = currentRot * Quaternion.Inverse(prevRot);
                deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180)
                {
                    angle -= 360;
                }

                angularVelocity = angle * Mathf.Deg2Rad * axis.normalized / (prevDeltaTime * 1000);

                prevRot = currentRot;
                prevDeltaTime = Time.deltaTime;

                yield return Timing.WaitForOneFrame;
            }
        }

        private IEnumerator<float> UpdateTransform()
        {
            Vector3 prevVelocity = velocity;
            Vector3 prevAngularVelocity = angularVelocity;

            float delTime = 0;

            while (true)
            {
                delTime += Time.deltaTime;
                if (delTime > interval)
                {
                    delTime -= interval;

                    if (prevVelocity != velocity || prevAngularVelocity != angularVelocity)
                    {
                        C_SET_FPS_TRANSFORM();
                        prevVelocity = velocity;
                        prevAngularVelocity = angularVelocity;
                    }
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        private void C_SET_FPS_TRANSFORM()
        {
            Protocol.FPS_Transform fps_transform = new()
            {
                Position = NetworkUtils.UnityVector3ToProtocolVector3(transform.position),
                Velocity = NetworkUtils.UnityVector3ToProtocolVector3(velocity),
                Rotation = NetworkUtils.UnityVector3ToProtocolVector3(transform.eulerAngles),
                AngularVelocity = NetworkUtils.UnityVector3ToProtocolVector3(angularVelocity)
            };

            C_SET_FPS_TRANSFORM packet = new()
            {
                PlayerId = objectId,
                Timestamp = client.calcuatedServerTime,
                Transform = fps_transform
            };

            client.Send(PacketManager.MakeSendBuffer(packet));
        }

        private IEnumerator<float> RemoteUpdatePosition()
        {
            while (true)
            {
                _ = controller.Move(velocity * Time.deltaTime * 1000);
                yield return Timing.WaitForOneFrame;
            }
        }

        private IEnumerator<float> RemoteUpdateRotation()
        {
            while (true)
            {
                transform.rotation *= Quaternion.AngleAxis(angularVelocity.magnitude * Time.deltaTime * 1000 * Mathf.Rad2Deg, angularVelocity.normalized);
                yield return Timing.WaitForOneFrame;
            }
        }

        private void S_SET_TRANSFORM( S_SET_TRANSFORM packet )
        {
            if (packet.GameObjectId != objectId)
            {
                return;
            }

            float timeGap;

            Vector3 packetPosition = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Position);
            velocity = NetworkUtils.ProtocolVector3ToUnityVector3(packet.Velocity);
            Vector3 predictedPosition;

            Quaternion packetRotation = NetworkUtils.ProtocolVector3ToUnityQuaternion(packet.Rotation);
            angularVelocity = NetworkUtils.ProtocolVector3ToUnityVector3(packet.AngularVelocity);
            Quaternion predictedRotation;

            timeGap = client.calcuatedServerTime - packet.Timestamp;

            predictedRotation = packetRotation * Quaternion.AngleAxis(angularVelocity.magnitude * timeGap * Mathf.Rad2Deg, angularVelocity.normalized);

            if (2.0f * Mathf.Acos(Mathf.Clamp((transform.rotation * Quaternion.Inverse(predictedRotation)).w, -1.0f, 1.0f)) * Mathf.Rad2Deg > 3.0f)
            {
                transform.rotation = predictedRotation;
            }

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
    }
}