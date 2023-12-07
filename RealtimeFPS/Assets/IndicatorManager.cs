using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class IndicatorManager : MonoBehaviour
{
    GameObject indicator;
    Vector3 position;

    CoroutineHandle updateUI;

    void Start()
    {
        indicator = transform.Find("Indicator").gameObject;
        indicator.SetActive(false);

        NetworkManager.Instance.Client.packetHandler.AddHandler(OnSpawnItem);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnItemOccupied);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnSpawnDestination);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnDestroyDestination);
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnSpawnItem);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnItemOccupied);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnSpawnDestination);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnDestroyDestination);

        if(updateUI.IsRunning)
            Timing.KillCoroutines(updateUI);
    }

    private void OnSpawnItem( Protocol.S_FPS_SPAWN_ITEM pkt )
    {
        position = NetworkUtils.ConvertVector3(pkt.Position);
        updateUI = Timing.RunCoroutine(UpdateUI());
        indicator.SetActive(true);
    }

    private void OnItemOccupied( Protocol.S_FPS_ITEM_OCCUPIED pkt )
    {
        Timing.KillCoroutines(updateUI);
        indicator.SetActive(false);
    }

    private void OnSpawnDestination(Protocol.S_FPS_SPAWN_DESTINATION pkt )
    {
        position = NetworkUtils.ConvertVector3(pkt.Position);
        updateUI = Timing.RunCoroutine(UpdateUI());
        indicator.SetActive(true);
    }

    private void OnDestroyDestination(Protocol.S_FPS_DESTROY_DESTINATION pkt)
    {
        Timing.KillCoroutines(updateUI);
        indicator.SetActive(false);
    }

    //private IEnumerator<float> UpdateUI()
    //{
    //    while (true)
    //    {
    //        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(position);

    //        bool isVisible = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;

    //        if (isVisible)
    //        {
    //            Vector3 screenPoint = Camera.main.WorldToScreenPoint(position);
    //            transform.position = screenPoint + Vector3.up * 100f;

    //            indicator.SetActive(true);
    //        }
    //        else
    //        {
    //            indicator.SetActive(false);
    //        }

    //        yield return Timing.WaitForOneFrame;
    //    }
    //}

    private IEnumerator<float> UpdateUI()
    {
        while (true)
        {
            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(position);

            // 화면 밖으로 나간 경우의 처리
            bool isOffScreen = viewportPoint.z <= 0 || viewportPoint.x <= 0 || viewportPoint.x >= 1 || viewportPoint.y <= 0 || viewportPoint.y >= 1;

            if (isOffScreen || viewportPoint.z <= 0)
            {
                // 화면 가장자리에 맞게 조정
                viewportPoint.x = Mathf.Clamp(viewportPoint.x, 0, 1);
                viewportPoint.y = Mathf.Clamp(viewportPoint.y, 0, 1);

                // 카메라 뒤에 있는 경우
                if (viewportPoint.z <= 0)
                {
                    viewportPoint.y = 0f;
                    viewportPoint.x = 1f - viewportPoint.x;
                }

                Vector3 screenPoint = new Vector3(
                    viewportPoint.x * Screen.width,
                    viewportPoint.y * Screen.height,
                    0);

                transform.position = screenPoint + Vector3.up * 50f;
            }
            else
            {
                // 화면 안에 있는 경우
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(position);
                transform.position = screenPoint + Vector3.up * 100f;
            }

            yield return Timing.WaitForOneFrame;
        }
    }


}
