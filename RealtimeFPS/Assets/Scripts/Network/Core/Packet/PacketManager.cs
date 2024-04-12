using Google.Protobuf;
using Protocol;
using System;
using System.Collections.Generic;

namespace Framework.Network
{
    public enum MsgId : ushort
    {
        PKT_C_ENTER = 0,
        PKT_S_ENTER = 1,
        PKT_C_REENTER = 2,
        PKT_S_REENTER = 3,
        PKT_C_LEAVE = 4,
        PKT_C_GET_CLIENT = 5,
        PKT_S_ADD_CLIENT = 6,
        PKT_S_REMOVE_CLIENT = 7,
        PKT_S_DISCONNECT = 8,
        PKT_C_HEARTBEAT = 9,
        PKT_C_PING = 10,
        PKT_S_PING = 11,
        PKT_C_SERVERTIME = 12,
        PKT_S_SERVERTIME = 13,
        PKT_C_TEST = 14,
        PKT_S_TEST = 15,
        PKT_C_INSTANTIATE_GAME_OBJECT = 100,
        PKT_S_INSTANTIATE_GAME_OBJECT = 101,
        PKT_C_GET_GAME_OBJECT = 102,
        PKT_S_ADD_GAME_OBJECT = 103,
        PKT_C_DESTORY_GAME_OBJECT = 104,
        PKT_S_DESTORY_GAME_OBJECT = 105,
        PKT_S_REMOVE_GAME_OBJECT = 106,
        PKT_C_SET_GAME_OBJECT_PREFAB = 107,
        PKT_S_SET_GAME_OBJECT_PREFAB = 108,
        PKT_C_SET_GAME_OBJECT_OWNER = 109,
        PKT_S_SET_GAME_OBJECT_OWNER = 110,
        PKT_C_SET_TRANSFORM = 111,
        PKT_S_SET_TRANSFORM = 112,
        PKT_C_SET_ANIMATION = 113,
        PKT_S_SET_ANIMATION = 114,
        PKT_S_FPS_INSTANTIATE = 200,
        PKT_C_FPS_POSITION = 201,
        PKT_S_FPS_POSITION = 202,
        PKT_C_FPS_ROTATION = 203,
        PKT_S_FPS_ROTATION = 204,
        PKT_C_FPS_SHOOT = 205,
        PKT_S_FPS_SHOOT = 206,
        PKT_S_FPS_ATTACKED = 207,
        PKT_C_FPS_CHANGE_WEAPON = 208,
        PKT_S_FPS_CHANGE_WEAPON = 209,
        PKT_C_FPS_RELOAD = 210,
        PKT_S_FPS_RELOAD = 211,
        PKT_C_FPS_ANIMATION = 212,
        PKT_S_FPS_ANIMATION = 213,
        PKT_C_FPS_READY = 214,
        PKT_S_FPS_LOAD = 215,
        PKT_C_FPS_LOAD_COMPLETE = 216,
        PKT_C_FPS_START = 217,
        PKT_S_FPS_START = 218,
        PKT_S_FPS_FINISH = 219,
        PKT_S_FPS_ANNOUNCE = 220,
        PKT_S_FPS_SPAWN_ITEM = 221,
        PKT_S_FPS_SPAWN_DESTINATION = 222,
        PKT_S_FPS_DESTROY_DESTINATION = 223,
        PKT_S_FPS_ITEM_OCCUPY_PROGRESS_STATE = 224,
        PKT_S_FPS_ITEM_OCCUPIED = 225,
        PKT_S_FPS_SCORED = 226,
        PKT_S_FPS_REPLAY = 300,
        PKT_C_FPS_REPLAY = 301,
    }

    public static class PacketManager
    {
        private static readonly Dictionary<ushort, Action<ArraySegment<byte>, ushort, Connection>> onRecv = new();

        static PacketManager()
        {
            onRecv.Add((ushort)MsgId.PKT_S_ENTER, MakePacket<S_ENTER>);
            onRecv.Add((ushort)MsgId.PKT_S_REENTER, MakePacket<S_REENTER>);
            onRecv.Add((ushort)MsgId.PKT_S_ADD_CLIENT, MakePacket<S_ADD_CLIENT>);
            onRecv.Add((ushort)MsgId.PKT_S_REMOVE_CLIENT, MakePacket<S_REMOVE_CLIENT>);
            onRecv.Add((ushort)MsgId.PKT_S_DISCONNECT, MakePacket<S_DISCONNECT>);
            onRecv.Add((ushort)MsgId.PKT_S_PING, MakePacket<S_PING>);
            onRecv.Add((ushort)MsgId.PKT_S_SERVERTIME, MakePacket<S_SERVERTIME>);
            onRecv.Add((ushort)MsgId.PKT_S_TEST, MakePacket<S_TEST>);
            onRecv.Add((ushort)MsgId.PKT_S_INSTANTIATE_GAME_OBJECT, MakePacket<S_INSTANTIATE_GAME_OBJECT>);
            onRecv.Add((ushort)MsgId.PKT_S_ADD_GAME_OBJECT, MakePacket<S_ADD_GAME_OBJECT>);
            onRecv.Add((ushort)MsgId.PKT_S_DESTORY_GAME_OBJECT, MakePacket<S_DESTORY_GAME_OBJECT>);
            onRecv.Add((ushort)MsgId.PKT_S_REMOVE_GAME_OBJECT, MakePacket<S_REMOVE_GAME_OBJECT>);
            onRecv.Add((ushort)MsgId.PKT_S_SET_GAME_OBJECT_PREFAB, MakePacket<S_SET_GAME_OBJECT_PREFAB>);
            onRecv.Add((ushort)MsgId.PKT_S_SET_GAME_OBJECT_OWNER, MakePacket<S_SET_GAME_OBJECT_OWNER>);
            onRecv.Add((ushort)MsgId.PKT_S_SET_TRANSFORM, MakePacket<S_SET_TRANSFORM>);
            onRecv.Add((ushort)MsgId.PKT_S_SET_ANIMATION, MakePacket<S_SET_ANIMATION>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_INSTANTIATE, MakePacket<S_FPS_INSTANTIATE>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_POSITION, MakePacket<S_FPS_POSITION>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_ROTATION, MakePacket<S_FPS_ROTATION>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_SHOOT, MakePacket<S_FPS_SHOOT>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_ATTACKED, MakePacket<S_FPS_ATTACKED>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_CHANGE_WEAPON, MakePacket<S_FPS_CHANGE_WEAPON>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_RELOAD, MakePacket<S_FPS_RELOAD>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_ANIMATION, MakePacket<S_FPS_ANIMATION>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_LOAD, MakePacket<S_FPS_LOAD>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_START, MakePacket<S_FPS_START>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_FINISH, MakePacket<S_FPS_FINISH>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_ANNOUNCE, MakePacket<S_FPS_ANNOUNCE>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_SPAWN_ITEM, MakePacket<S_FPS_SPAWN_ITEM>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_SPAWN_DESTINATION, MakePacket<S_FPS_SPAWN_DESTINATION>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_DESTROY_DESTINATION, MakePacket<S_FPS_DESTROY_DESTINATION>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_ITEM_OCCUPY_PROGRESS_STATE, MakePacket<S_FPS_ITEM_OCCUPY_PROGRESS_STATE>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_ITEM_OCCUPIED, MakePacket<S_FPS_ITEM_OCCUPIED>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_SCORED, MakePacket<S_FPS_SCORED>);
            onRecv.Add((ushort)MsgId.PKT_S_FPS_REPLAY, MakePacket<S_FPS_REPLAY>);
        }

        public static void OnRecv( ArraySegment<byte> buffer, Connection connection )
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            if (onRecv.TryGetValue(id, out Action<ArraySegment<byte>, ushort, Connection> action))
            {
                action.Invoke(buffer, id, connection);
            }
        }

        private static void MakePacket<T>( ArraySegment<byte> buffer, ushort id, Connection connection ) where T : IMessage, new()
        {
            T pkt = new();
            pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

            if (id == (ushort)MsgId.PKT_S_PING)
            {
                Protocol.S_PING ping = pkt as Protocol.S_PING;
                connection.Handle_S_PING(ping);
            }
            if (id == (ushort)MsgId.PKT_S_SERVERTIME)
            {
                Protocol.S_SERVERTIME serverTime = pkt as Protocol.S_SERVERTIME;
                connection.Handle_S_SERVERTIME(serverTime);
            }

            connection.PacketQueue.Push(id, pkt);
        }
        
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_ENTER pkt ) { return MakeSendBuffer(pkt, 0); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_REENTER pkt ) { return MakeSendBuffer(pkt, 2); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_LEAVE pkt ) { return MakeSendBuffer(pkt, 4); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_GET_CLIENT pkt ) { return MakeSendBuffer(pkt, 5); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_HEARTBEAT pkt ) { return MakeSendBuffer(pkt, 9); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_PING pkt ) { return MakeSendBuffer(pkt, 10); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_SERVERTIME pkt ) { return MakeSendBuffer(pkt, 12); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_TEST pkt ) { return MakeSendBuffer(pkt, 14); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_INSTANTIATE_GAME_OBJECT pkt ) { return MakeSendBuffer(pkt, 100); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_GET_GAME_OBJECT pkt ) { return MakeSendBuffer(pkt, 102); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_DESTORY_GAME_OBJECT pkt ) { return MakeSendBuffer(pkt, 104); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_SET_GAME_OBJECT_PREFAB pkt ) { return MakeSendBuffer(pkt, 107); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_SET_GAME_OBJECT_OWNER pkt ) { return MakeSendBuffer(pkt, 109); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_SET_TRANSFORM pkt ) { return MakeSendBuffer(pkt, 111); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_SET_ANIMATION pkt ) { return MakeSendBuffer(pkt, 113); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_FPS_POSITION pkt ) { return MakeSendBuffer(pkt, 201); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_FPS_ROTATION pkt ) { return MakeSendBuffer(pkt, 203); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_FPS_SHOOT pkt ) { return MakeSendBuffer(pkt, 205); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_FPS_CHANGE_WEAPON pkt ) { return MakeSendBuffer(pkt, 208); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_FPS_RELOAD pkt ) { return MakeSendBuffer(pkt, 210); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_FPS_ANIMATION pkt ) { return MakeSendBuffer(pkt, 212); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_FPS_READY pkt ) { return MakeSendBuffer(pkt, 214); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_FPS_LOAD_COMPLETE pkt ) { return MakeSendBuffer(pkt, 216); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_FPS_START pkt ) { return MakeSendBuffer(pkt, 217); }
        public static ArraySegment<byte> MakeSendBuffer( Protocol.C_FPS_REPLAY pkt ) { return MakeSendBuffer(pkt, 301); }

        private static ArraySegment<byte> MakeSendBuffer( IMessage pkt, ushort pktId )
        {
            ushort size = (ushort)pkt.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];

            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes(pktId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(pkt.ToByteArray(), 0, sendBuffer, 4, size);

            return sendBuffer;
        }
    }
}