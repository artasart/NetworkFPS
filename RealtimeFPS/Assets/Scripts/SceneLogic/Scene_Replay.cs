using Framework.Network;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Scene_Replay : MonoBehaviour
{
    private Dictionary<int, GameObject> players;

    void Start()
    {
        players = new Dictionary<int, GameObject>();

        GameManager.Scene.Fade(false);

        Debug.Log("Scene_Replay Start");

        NetworkManager.Instance.Client.packetHandler.AddHandler(OnReplay);

        Protocol.C_FPS_REPLAY req = new Protocol.C_FPS_REPLAY();
        NetworkManager.Instance.Client.Send(PacketManager.MakeSendBuffer(req));
    }

    public void OnReplay( S_FPS_REPLAY pkt )
    {
        Debug.Log("OnReplay");
        Debug.Log(pkt.ReplayData.Count);

        StartCoroutine(Co_Replay(pkt));
    }


    IEnumerator Co_Replay( S_FPS_REPLAY pkt )
    {
        for (int i = 0; i < pkt.ReplayData.Count; i++)
        {
            for (int j = 0; j < pkt.ReplayData[i].Instantiate.Count; j++)
            {
                HandleInstantiate(pkt.ReplayData[i].Instantiate[j]);
            }

            for (int j = 0; j < pkt.ReplayData[i].Position.Count; j++)
            {
                HandlePosition(pkt.ReplayData[i].Position[j]);
            }

            for (int j = 0; j < pkt.ReplayData[i].Rotation.Count; j++)
            {
                HandleRotation(pkt.ReplayData[i].Rotation[j]);
            }

            for (int j = 0; j < pkt.ReplayData[i].Shoot.Count; j++)
            {
                HandleShoot(pkt.ReplayData[i].Shoot[j]);
            }

            for (int j = 0; j < pkt.ReplayData[i].Attacked.Count; j++)
            {
                HandleAttacked(pkt.ReplayData[i].Attacked[j]);
            }

            for (int j = 0; j < pkt.ReplayData[i].Death.Count; j++)
            {
                HandleDeath(pkt.ReplayData[i].Death[j]);
            }

            for (int j = 0; j < pkt.ReplayData[i].ItemSpawn.Count; j++)
            {
                HandleItemSpawn(pkt.ReplayData[i].ItemSpawn[j]);
            }

            for (int j = 0; j < pkt.ReplayData[i].ItemDestroy.Count; j++)
            {
                HandleItemDestroy(pkt.ReplayData[i].ItemDestroy[j]);
            }

            for (int j = 0; j < pkt.ReplayData[i].DestinationSpawn.Count; j++)
            {
                HandleDestinationSpawn(pkt.ReplayData[i].DestinationSpawn[j]);
            }

            for (int j = 0; j < pkt.ReplayData[i].DestinationDestroy.Count; j++)
            {
                HandleDestinationDestroy(pkt.ReplayData[i].DestinationDestroy[j]);
            }

            yield return new WaitForSeconds(0.05f);
        }

        GameManager.Scene.Fade(true);
        GameManager.Scene.LoadScene(SceneName.Lobby);
    }

    private void HandleInstantiate( Replay_Instantiate pkt )
    {
        Vector3 position = NetworkUtils.ConvertVector3(pkt.Position);
        Quaternion rotation = NetworkUtils.ConvertQuaternion(pkt.Rotation);

        GameObject player = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefab/Generic/PlayerCharacter_Remote"), position, rotation);

        player.GetComponent<NetworkObject>().Client = NetworkManager.Instance.Client;
        player.GetComponent<NetworkObject>().id = pkt.PlayerId;

        player.name = pkt.PlayerId.ToString();

        players.Add(pkt.PlayerId, player);
    }

    private void HandlePosition( Replay_Position pkt )
    {
        GameObject player;
        players.TryGetValue(pkt.PlayerId, out player);
        if(player == null)
            return;

        player.transform.position = NetworkUtils.ConvertVector3(pkt.Position);
    }

    private void HandleRotation( Replay_Rotation pkt )
    {
        GameObject player;
        players.TryGetValue(pkt.PlayerId, out player);
        if (player == null)
            return;

        player.transform.rotation = NetworkUtils.ConvertQuaternion(pkt.Rotation);
    }

    private void HandleShoot( Replay_Shoot pkt )
    {

    }

    private void HandleAttacked( Replay_Attacked pkt )
    {

    }

    private void HandleDeath( Replay_Death pkt )
    {

    }

    private void HandleItemSpawn( Replay_ItemSpawn pkt )
    {

    }

    private void HandleItemDestroy( Replay_ItemDestroy pkt )
    {

    }

    private void HandleDestinationSpawn( Replay_DestinationSpawn pkt )
    {

    }

    private void HandleDestinationDestroy( Replay_DestinationDestroy pkt )
    {

    }
}
