using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class Progressbar_Circular_Manager : MonoBehaviour
{
    GameObject progressbar;

    Vector3 itemPosition;

    bool isOn;

    CoroutineHandle updateUI;

    void Start()
    {
        progressbar = transform.Search(nameof(Progressbar_Circular)).gameObject;
        progressbar.SetActive(false);

        NetworkManager.Instance.Client.packetHandler.AddHandler(OnSpawnItem);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnItemOccupied);
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnSpawnItem);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnItemOccupied);
    }

    private void OnSpawnItem(Protocol.S_FPS_SPAWN_ITEM pkt)
    {
        progressbar.SetActive(true);
        progressbar.GetComponent<Progressbar_Circular>().Refresh();

        itemPosition = NetworkUtils.ConvertVector3(pkt.Position);

        isOn = true;

        updateUI = Timing.RunCoroutine(UpdateUI());
    }

    private void OnItemOccupied(Protocol.S_FPS_ITEM_OCCUPIED pkt)
    {
        isOn = false;
        progressbar.SetActive(false);
    }

    private IEnumerator<float> UpdateUI()
    {
        while (isOn)
        {
            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(itemPosition);

            bool isVisible = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;

            if (isVisible)
            {
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(itemPosition);
                transform.position = screenPoint + Vector3.up * 200f;

                progressbar.SetActive(true);
            }
            else
            {
                progressbar.SetActive(false);
            }

            yield return Timing.WaitForOneFrame;
        }
    }
}
