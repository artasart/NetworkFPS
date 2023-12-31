using Google.Protobuf;
using System;
using System.Collections.Generic;

namespace Framework.Network
{
    public class PacketHandler
    {
        public Dictionary<ushort, Action<IMessage>> Handlers = new();
        private Action<Protocol.S_ENTER> S_ENTER_Handler;
        private Action<Protocol.S_REENTER> S_REENTER_Handler;
        private Action<Protocol.S_ADD_CLIENT> S_ADD_CLIENT_Handler;
        private Action<Protocol.S_REMOVE_CLIENT> S_REMOVE_CLIENT_Handler;
        private Action<Protocol.S_DISCONNECT> S_DISCONNECT_Handler;
        private Action<Protocol.S_PING> S_PING_Handler;
        private Action<Protocol.S_SERVERTIME> S_SERVERTIME_Handler;
        private Action<Protocol.S_TEST> S_TEST_Handler;
        private Action<Protocol.S_INSTANTIATE_GAME_OBJECT> S_INSTANTIATE_GAME_OBJECT_Handler;
        private Action<Protocol.S_ADD_GAME_OBJECT> S_ADD_GAME_OBJECT_Handler;
        private Action<Protocol.S_DESTORY_GAME_OBJECT> S_DESTORY_GAME_OBJECT_Handler;
        private Action<Protocol.S_REMOVE_GAME_OBJECT> S_REMOVE_GAME_OBJECT_Handler;
        private Action<Protocol.S_SET_GAME_OBJECT_PREFAB> S_SET_GAME_OBJECT_PREFAB_Handler;
        private Action<Protocol.S_SET_GAME_OBJECT_OWNER> S_SET_GAME_OBJECT_OWNER_Handler;
        private Action<Protocol.S_SET_TRANSFORM> S_SET_TRANSFORM_Handler;
        private Action<Protocol.S_SET_ANIMATION> S_SET_ANIMATION_Handler;
        private Action<Protocol.S_FPS_INSTANTIATE> S_FPS_INSTANTIATE_Handler;
        private Action<Protocol.S_FPS_POSITION> S_FPS_POSITION_Handler;
        private Action<Protocol.S_FPS_ROTATION> S_FPS_ROTATION_Handler;
        private Action<Protocol.S_FPS_SHOOT> S_FPS_SHOOT_Handler;
        private Action<Protocol.S_FPS_ATTACKED> S_FPS_ATTACKED_Handler;
        private Action<Protocol.S_FPS_CHANGE_WEAPON> S_FPS_CHANGE_WEAPON_Handler;
        private Action<Protocol.S_FPS_RELOAD> S_FPS_RELOAD_Handler;
        private Action<Protocol.S_FPS_ANIMATION> S_FPS_ANIMATION_Handler;
        private Action<Protocol.S_FPS_LOAD> S_FPS_LOAD_Handler;
        private Action<Protocol.S_FPS_START> S_FPS_START_Handler;
        private Action<Protocol.S_FPS_FINISH> S_FPS_FINISH_Handler;
        private Action<Protocol.S_FPS_ANNOUNCE> S_FPS_ANNOUNCE_Handler;
        private Action<Protocol.S_FPS_SPAWN_ITEM> S_FPS_SPAWN_ITEM_Handler;
        private Action<Protocol.S_FPS_SPAWN_DESTINATION> S_FPS_SPAWN_DESTINATION_Handler;
        private Action<Protocol.S_FPS_DESTROY_DESTINATION> S_FPS_DESTROY_DESTINATION_Handler;
        private Action<Protocol.S_FPS_ITEM_OCCUPY_PROGRESS_STATE> S_FPS_ITEM_OCCUPY_PROGRESS_STATE_Handler;
        private Action<Protocol.S_FPS_ITEM_OCCUPIED> S_FPS_ITEM_OCCUPIED_Handler;
        private Action<Protocol.S_FPS_SCORED> S_FPS_SCORED_Handler;

        public PacketHandler()
        {
            Handlers.Add(1, _Handle_S_ENTER);
            Handlers.Add(3, _Handle_S_REENTER);
            Handlers.Add(6, _Handle_S_ADD_CLIENT);
            Handlers.Add(7, _Handle_S_REMOVE_CLIENT);
            Handlers.Add(8, _Handle_S_DISCONNECT);
            Handlers.Add(11, _Handle_S_PING);
            Handlers.Add(13, _Handle_S_SERVERTIME);
            Handlers.Add(15, _Handle_S_TEST);
            Handlers.Add(101, _Handle_S_INSTANTIATE_GAME_OBJECT);
            Handlers.Add(103, _Handle_S_ADD_GAME_OBJECT);
            Handlers.Add(105, _Handle_S_DESTORY_GAME_OBJECT);
            Handlers.Add(106, _Handle_S_REMOVE_GAME_OBJECT);
            Handlers.Add(108, _Handle_S_SET_GAME_OBJECT_PREFAB);
            Handlers.Add(110, _Handle_S_SET_GAME_OBJECT_OWNER);
            Handlers.Add(112, _Handle_S_SET_TRANSFORM);
            Handlers.Add(114, _Handle_S_SET_ANIMATION);
            Handlers.Add(200, _Handle_S_FPS_INSTANTIATE);
            Handlers.Add(202, _Handle_S_FPS_POSITION);
            Handlers.Add(204, _Handle_S_FPS_ROTATION);
            Handlers.Add(206, _Handle_S_FPS_SHOOT);
            Handlers.Add(207, _Handle_S_FPS_ATTACKED);
            Handlers.Add(209, _Handle_S_FPS_CHANGE_WEAPON);
            Handlers.Add(211, _Handle_S_FPS_RELOAD);
            Handlers.Add(213, _Handle_S_FPS_ANIMATION);
            Handlers.Add(215, _Handle_S_FPS_LOAD);
            Handlers.Add(217, _Handle_S_FPS_START);
            Handlers.Add(218, _Handle_S_FPS_FINISH);
            Handlers.Add(219, _Handle_S_FPS_ANNOUNCE);
            Handlers.Add(220, _Handle_S_FPS_SPAWN_ITEM);
            Handlers.Add(221, _Handle_S_FPS_SPAWN_DESTINATION);
            Handlers.Add(222, _Handle_S_FPS_DESTROY_DESTINATION);
            Handlers.Add(223, _Handle_S_FPS_ITEM_OCCUPY_PROGRESS_STATE);
            Handlers.Add(224, _Handle_S_FPS_ITEM_OCCUPIED);
            Handlers.Add(225, _Handle_S_FPS_SCORED);
        }
        public void AddHandler( Action<Protocol.S_ENTER> handler )
        {
            S_ENTER_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_ENTER> handler )
        {
            S_ENTER_Handler -= handler;
        }
        private void _Handle_S_ENTER( IMessage message )
        {
            S_ENTER_Handler?.Invoke((Protocol.S_ENTER)message);
        }
        public void AddHandler( Action<Protocol.S_REENTER> handler )
        {
            S_REENTER_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_REENTER> handler )
        {
            S_REENTER_Handler -= handler;
        }
        private void _Handle_S_REENTER( IMessage message )
        {
            S_REENTER_Handler?.Invoke((Protocol.S_REENTER)message);
        }
        public void AddHandler( Action<Protocol.S_ADD_CLIENT> handler )
        {
            S_ADD_CLIENT_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_ADD_CLIENT> handler )
        {
            S_ADD_CLIENT_Handler -= handler;
        }
        private void _Handle_S_ADD_CLIENT( IMessage message )
        {
            S_ADD_CLIENT_Handler?.Invoke((Protocol.S_ADD_CLIENT)message);
        }
        public void AddHandler( Action<Protocol.S_REMOVE_CLIENT> handler )
        {
            S_REMOVE_CLIENT_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_REMOVE_CLIENT> handler )
        {
            S_REMOVE_CLIENT_Handler -= handler;
        }
        private void _Handle_S_REMOVE_CLIENT( IMessage message )
        {
            S_REMOVE_CLIENT_Handler?.Invoke((Protocol.S_REMOVE_CLIENT)message);
        }
        public void AddHandler( Action<Protocol.S_DISCONNECT> handler )
        {
            S_DISCONNECT_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_DISCONNECT> handler )
        {
            S_DISCONNECT_Handler -= handler;
        }
        private void _Handle_S_DISCONNECT( IMessage message )
        {
            S_DISCONNECT_Handler?.Invoke((Protocol.S_DISCONNECT)message);
        }
        public void AddHandler( Action<Protocol.S_PING> handler )
        {
            S_PING_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_PING> handler )
        {
            S_PING_Handler -= handler;
        }
        private void _Handle_S_PING( IMessage message )
        {
            S_PING_Handler?.Invoke((Protocol.S_PING)message);
        }
        public void AddHandler( Action<Protocol.S_SERVERTIME> handler )
        {
            S_SERVERTIME_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_SERVERTIME> handler )
        {
            S_SERVERTIME_Handler -= handler;
        }
        private void _Handle_S_SERVERTIME( IMessage message )
        {
            S_SERVERTIME_Handler?.Invoke((Protocol.S_SERVERTIME)message);
        }
        public void AddHandler( Action<Protocol.S_TEST> handler )
        {
            S_TEST_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_TEST> handler )
        {
            S_TEST_Handler -= handler;
        }
        private void _Handle_S_TEST( IMessage message )
        {
            S_TEST_Handler?.Invoke((Protocol.S_TEST)message);
        }
        public void AddHandler( Action<Protocol.S_INSTANTIATE_GAME_OBJECT> handler )
        {
            S_INSTANTIATE_GAME_OBJECT_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_INSTANTIATE_GAME_OBJECT> handler )
        {
            S_INSTANTIATE_GAME_OBJECT_Handler -= handler;
        }
        private void _Handle_S_INSTANTIATE_GAME_OBJECT( IMessage message )
        {
            S_INSTANTIATE_GAME_OBJECT_Handler?.Invoke((Protocol.S_INSTANTIATE_GAME_OBJECT)message);
        }
        public void AddHandler( Action<Protocol.S_ADD_GAME_OBJECT> handler )
        {
            S_ADD_GAME_OBJECT_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_ADD_GAME_OBJECT> handler )
        {
            S_ADD_GAME_OBJECT_Handler -= handler;
        }
        private void _Handle_S_ADD_GAME_OBJECT( IMessage message )
        {
            S_ADD_GAME_OBJECT_Handler?.Invoke((Protocol.S_ADD_GAME_OBJECT)message);
        }
        public void AddHandler( Action<Protocol.S_DESTORY_GAME_OBJECT> handler )
        {
            S_DESTORY_GAME_OBJECT_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_DESTORY_GAME_OBJECT> handler )
        {
            S_DESTORY_GAME_OBJECT_Handler -= handler;
        }
        private void _Handle_S_DESTORY_GAME_OBJECT( IMessage message )
        {
            S_DESTORY_GAME_OBJECT_Handler?.Invoke((Protocol.S_DESTORY_GAME_OBJECT)message);
        }
        public void AddHandler( Action<Protocol.S_REMOVE_GAME_OBJECT> handler )
        {
            S_REMOVE_GAME_OBJECT_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_REMOVE_GAME_OBJECT> handler )
        {
            S_REMOVE_GAME_OBJECT_Handler -= handler;
        }
        private void _Handle_S_REMOVE_GAME_OBJECT( IMessage message )
        {
            S_REMOVE_GAME_OBJECT_Handler?.Invoke((Protocol.S_REMOVE_GAME_OBJECT)message);
        }
        public void AddHandler( Action<Protocol.S_SET_GAME_OBJECT_PREFAB> handler )
        {
            S_SET_GAME_OBJECT_PREFAB_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_SET_GAME_OBJECT_PREFAB> handler )
        {
            S_SET_GAME_OBJECT_PREFAB_Handler -= handler;
        }
        private void _Handle_S_SET_GAME_OBJECT_PREFAB( IMessage message )
        {
            S_SET_GAME_OBJECT_PREFAB_Handler?.Invoke((Protocol.S_SET_GAME_OBJECT_PREFAB)message);
        }
        public void AddHandler( Action<Protocol.S_SET_GAME_OBJECT_OWNER> handler )
        {
            S_SET_GAME_OBJECT_OWNER_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_SET_GAME_OBJECT_OWNER> handler )
        {
            S_SET_GAME_OBJECT_OWNER_Handler -= handler;
        }
        private void _Handle_S_SET_GAME_OBJECT_OWNER( IMessage message )
        {
            S_SET_GAME_OBJECT_OWNER_Handler?.Invoke((Protocol.S_SET_GAME_OBJECT_OWNER)message);
        }
        public void AddHandler( Action<Protocol.S_SET_TRANSFORM> handler )
        {
            S_SET_TRANSFORM_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_SET_TRANSFORM> handler )
        {
            S_SET_TRANSFORM_Handler -= handler;
        }
        private void _Handle_S_SET_TRANSFORM( IMessage message )
        {
            S_SET_TRANSFORM_Handler?.Invoke((Protocol.S_SET_TRANSFORM)message);
        }
        public void AddHandler( Action<Protocol.S_SET_ANIMATION> handler )
        {
            S_SET_ANIMATION_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_SET_ANIMATION> handler )
        {
            S_SET_ANIMATION_Handler -= handler;
        }
        private void _Handle_S_SET_ANIMATION( IMessage message )
        {
            S_SET_ANIMATION_Handler?.Invoke((Protocol.S_SET_ANIMATION)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_INSTANTIATE> handler )
        {
            S_FPS_INSTANTIATE_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_INSTANTIATE> handler )
        {
            S_FPS_INSTANTIATE_Handler -= handler;
        }
        private void _Handle_S_FPS_INSTANTIATE( IMessage message )
        {
            S_FPS_INSTANTIATE_Handler?.Invoke((Protocol.S_FPS_INSTANTIATE)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_POSITION> handler )
        {
            S_FPS_POSITION_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_POSITION> handler )
        {
            S_FPS_POSITION_Handler -= handler;
        }
        private void _Handle_S_FPS_POSITION( IMessage message )
        {
            S_FPS_POSITION_Handler?.Invoke((Protocol.S_FPS_POSITION)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_ROTATION> handler )
        {
            S_FPS_ROTATION_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_ROTATION> handler )
        {
            S_FPS_ROTATION_Handler -= handler;
        }
        private void _Handle_S_FPS_ROTATION( IMessage message )
        {
            S_FPS_ROTATION_Handler?.Invoke((Protocol.S_FPS_ROTATION)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_SHOOT> handler )
        {
            S_FPS_SHOOT_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_SHOOT> handler )
        {
            S_FPS_SHOOT_Handler -= handler;
        }
        private void _Handle_S_FPS_SHOOT( IMessage message )
        {
            S_FPS_SHOOT_Handler?.Invoke((Protocol.S_FPS_SHOOT)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_ATTACKED> handler )
        {
            S_FPS_ATTACKED_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_ATTACKED> handler )
        {
            S_FPS_ATTACKED_Handler -= handler;
        }
        private void _Handle_S_FPS_ATTACKED( IMessage message )
        {
            S_FPS_ATTACKED_Handler?.Invoke((Protocol.S_FPS_ATTACKED)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_CHANGE_WEAPON> handler )
        {
            S_FPS_CHANGE_WEAPON_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_CHANGE_WEAPON> handler )
        {
            S_FPS_CHANGE_WEAPON_Handler -= handler;
        }
        private void _Handle_S_FPS_CHANGE_WEAPON( IMessage message )
        {
            S_FPS_CHANGE_WEAPON_Handler?.Invoke((Protocol.S_FPS_CHANGE_WEAPON)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_RELOAD> handler )
        {
            S_FPS_RELOAD_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_RELOAD> handler )
        {
            S_FPS_RELOAD_Handler -= handler;
        }
        private void _Handle_S_FPS_RELOAD( IMessage message )
        {
            S_FPS_RELOAD_Handler?.Invoke((Protocol.S_FPS_RELOAD)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_ANIMATION> handler )
        {
            S_FPS_ANIMATION_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_ANIMATION> handler )
        {
            S_FPS_ANIMATION_Handler -= handler;
        }
        private void _Handle_S_FPS_ANIMATION( IMessage message )
        {
            S_FPS_ANIMATION_Handler?.Invoke((Protocol.S_FPS_ANIMATION)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_LOAD> handler )
        {
            S_FPS_LOAD_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_LOAD> handler )
        {
            S_FPS_LOAD_Handler -= handler;
        }
        private void _Handle_S_FPS_LOAD( IMessage message )
        {
            S_FPS_LOAD_Handler?.Invoke((Protocol.S_FPS_LOAD)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_START> handler )
        {
            S_FPS_START_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_START> handler )
        {
            S_FPS_START_Handler -= handler;
        }
        private void _Handle_S_FPS_START( IMessage message )
        {
            S_FPS_START_Handler?.Invoke((Protocol.S_FPS_START)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_FINISH> handler )
        {
            S_FPS_FINISH_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_FINISH> handler )
        {
            S_FPS_FINISH_Handler -= handler;
        }
        private void _Handle_S_FPS_FINISH( IMessage message )
        {
            S_FPS_FINISH_Handler?.Invoke((Protocol.S_FPS_FINISH)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_ANNOUNCE> handler )
        {
            S_FPS_ANNOUNCE_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_ANNOUNCE> handler )
        {
            S_FPS_ANNOUNCE_Handler -= handler;
        }
        private void _Handle_S_FPS_ANNOUNCE( IMessage message )
        {
            S_FPS_ANNOUNCE_Handler?.Invoke((Protocol.S_FPS_ANNOUNCE)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_SPAWN_ITEM> handler )
        {
            S_FPS_SPAWN_ITEM_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_SPAWN_ITEM> handler )
        {
            S_FPS_SPAWN_ITEM_Handler -= handler;
        }
        private void _Handle_S_FPS_SPAWN_ITEM( IMessage message )
        {
            S_FPS_SPAWN_ITEM_Handler?.Invoke((Protocol.S_FPS_SPAWN_ITEM)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_SPAWN_DESTINATION> handler )
        {
            S_FPS_SPAWN_DESTINATION_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_SPAWN_DESTINATION> handler )
        {
            S_FPS_SPAWN_DESTINATION_Handler -= handler;
        }
        private void _Handle_S_FPS_SPAWN_DESTINATION( IMessage message )
        {
            S_FPS_SPAWN_DESTINATION_Handler?.Invoke((Protocol.S_FPS_SPAWN_DESTINATION)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_DESTROY_DESTINATION> handler )
        {
            S_FPS_DESTROY_DESTINATION_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_DESTROY_DESTINATION> handler )
        {
            S_FPS_DESTROY_DESTINATION_Handler -= handler;
        }
        private void _Handle_S_FPS_DESTROY_DESTINATION( IMessage message )
        {
            S_FPS_DESTROY_DESTINATION_Handler?.Invoke((Protocol.S_FPS_DESTROY_DESTINATION)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_ITEM_OCCUPY_PROGRESS_STATE> handler )
        {
            S_FPS_ITEM_OCCUPY_PROGRESS_STATE_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_ITEM_OCCUPY_PROGRESS_STATE> handler )
        {
            S_FPS_ITEM_OCCUPY_PROGRESS_STATE_Handler -= handler;
        }
        private void _Handle_S_FPS_ITEM_OCCUPY_PROGRESS_STATE( IMessage message )
        {
            S_FPS_ITEM_OCCUPY_PROGRESS_STATE_Handler?.Invoke((Protocol.S_FPS_ITEM_OCCUPY_PROGRESS_STATE)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_ITEM_OCCUPIED> handler )
        {
            S_FPS_ITEM_OCCUPIED_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_ITEM_OCCUPIED> handler )
        {
            S_FPS_ITEM_OCCUPIED_Handler -= handler;
        }
        private void _Handle_S_FPS_ITEM_OCCUPIED( IMessage message )
        {
            S_FPS_ITEM_OCCUPIED_Handler?.Invoke((Protocol.S_FPS_ITEM_OCCUPIED)message);
        }
        public void AddHandler( Action<Protocol.S_FPS_SCORED> handler )
        {
            S_FPS_SCORED_Handler += handler;
        }
        public void RemoveHandler( Action<Protocol.S_FPS_SCORED> handler )
        {
            S_FPS_SCORED_Handler -= handler;
        }
        private void _Handle_S_FPS_SCORED( IMessage message )
        {
            S_FPS_SCORED_Handler?.Invoke((Protocol.S_FPS_SCORED)message);
        }
    }
}