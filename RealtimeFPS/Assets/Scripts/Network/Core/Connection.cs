using Cysharp.Threading.Tasks;
using Google.Protobuf;
using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Network
{
    public enum ConnectionState
    {
        Idle,
        Connected
    }

    public class Connection
    {
        public string ConnectionId { get; set; }

        protected ServerSession session;
        
        public ServerSession Session
        {
            get => session;
            set
            {
                session = value;
                session.connectedHandler += _OnConnected;
                session.disconnectedHandler += _OnDisconnected;
                session.receivedHandler += _OnRecv;
            }
        }
        public PacketQueue PacketQueue { get; }
        public PacketHandler packetHandler { get; }

        protected ConnectionState state;
        public ConnectionState State => state;

        protected Action connectedHandler;
        protected Action disconnectedHandler;

        CoroutineHandle ping;
        CoroutineHandle updateServerTime;
        CoroutineHandle packetUpdate;

        private readonly Queue<long> pings;
        public long pingAverage;

        private long serverTime;
        public long calcuatedServerTime;
        private float deltaTime;
        readonly System.Diagnostics.Stopwatch stopwatch;

        public Connection()
        {
            state = ConnectionState.Idle;

            packetHandler = new PacketHandler();
            packetHandler.AddHandler(Handle_S_ENTER);
            packetHandler.AddHandler(Handle_S_DISCONNECTED);

            PacketQueue = new();
            pings = new();
            pingAverage = 0;

            serverTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            calcuatedServerTime = serverTime;
            deltaTime = 0f;

            stopwatch = new();

            ping = Timing.RunCoroutine(Ping());
            updateServerTime = Timing.RunCoroutine(UpdateServerTime());
            //순서 중요, UpdateServerTime 이후에 PacketUpdate 실행
            packetUpdate = Timing.RunCoroutine(PacketUpdate());
        }

        ~Connection()
        {
            UnityEngine.Debug.Log("Connection Destructor");

            Timing.KillCoroutines(packetUpdate);
            Timing.KillCoroutines(updateServerTime);
            Timing.KillCoroutines(ping);
        }

        private void _OnConnected()
        {
            state = ConnectionState.Connected;

            connectedHandler?.Invoke();
        }

        private void _OnDisconnected()
        {
            state = ConnectionState.Idle;

            disconnectedHandler?.Invoke();
        }

        protected void _OnRecv( ArraySegment<byte> buffer )
        {
            PacketManager.OnRecv(buffer, this);
        }

        public void Send( ArraySegment<byte> pkt )
        {
            if (state == ConnectionState.Connected)
                Session.Send(pkt);
        }

        private void Handle_S_ENTER( Protocol.S_ENTER pkt )
        {
            if (pkt.Result == "SUCCESS")
            {
                Protocol.C_SERVERTIME servertime = new();
                Send(PacketManager.MakeSendBuffer(servertime));
            }
        }

        private void Handle_S_DISCONNECTED( Protocol.S_DISCONNECT pkt )
        {
            UnityEngine.Debug.Log("Handle S Disconnect : " + pkt.Code);

            Close();
        }

        public void Handle_S_PING( Protocol.S_PING pkt )
        {
            pings.Enqueue((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds - pkt.Tick);

            if (pings.Count > 10)
            {
                _ = pings.Dequeue();
            }

            long sum = 0;
            foreach (long item in pings)
            {
                sum += item;
            }

            pingAverage = sum / pings.Count;
        }

        public void Handle_S_SERVERTIME( Protocol.S_SERVERTIME pkt )
        {
            serverTime = pkt.Tick + (pingAverage / 2);

            stopwatch.Start();
        }

        public void Close()
        {
            if (state == ConnectionState.Idle)
            {
                return;
            }

            state = ConnectionState.Idle;

            ConnectionManager.RemoveConnection(this);

            session?.RegisterDisconnect();
        }

        IEnumerator<float> Ping()
        {
            Protocol.C_PING ping = new();

            while (true)
            {
                if (state == ConnectionState.Connected)
                {
                    ping.Tick = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
                    Send(PacketManager.MakeSendBuffer(ping));
                }

                yield return Timing.WaitForSeconds(0.2f);
            }
        }

        private IEnumerator<float> PacketUpdate()
        {
            while (true)
            {
                if(state == ConnectionState.Connected || !PacketQueue.Empty())
                {
                    System.Collections.Generic.List<PacketMessage> packets = PacketQueue.PopAll();

                    for (int i = 0; i < packets.Count; i++)
                    {
                        PacketMessage packet = packets[i];

                        packetHandler.Handlers.TryGetValue(packet.Id, out Action<IMessage> handler);

                        handler?.Invoke(packet.Message);
                    }
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        private IEnumerator<float> UpdateServerTime()
        {
            while (true)
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();

                    serverTime += stopwatch.ElapsedMilliseconds;
                    calcuatedServerTime = serverTime;
                    deltaTime = 0;

                    stopwatch.Reset();
                }
                else
                {
                    deltaTime += Time.deltaTime;
                    calcuatedServerTime = serverTime + (long)Math.Round(deltaTime * 1000, 1);
                }

                yield return Timing.WaitForOneFrame;
            }
        }
    }
}
