using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class ProgressbarManager : MonoBehaviour
{
    GameObject progressbar;
    
    Vector3 itemPosition;

    bool isOn;

    CoroutineHandle updateUI;

    void Start()
    {
        progressbar = transform.Search(nameof(ProgressBar)).gameObject;
        progressbar.SetActive(false);
    }

    private void OnSpawnItem(Protocol.S_FPS_SPAWN_ITEM pkt)
    {
        itemPosition = NetworkUtils.ConvertVector3(pkt.Position);

        isOn = true;
        
        progressbar.SetActive(true);

        updateUI = Timing.RunCoroutine(UpdateUI());
    }

    private void OnItemOccupied(Protocol.S_FPS_ITEM_OCCUPIED pkt)
    {
        isOn = false;
        progressbar.SetActive(false);
        Timing.KillCoroutines(updateUI);
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
                transform.position = screenPoint + Vector3.up * 100f;

                progressbar.SetActive(true);
            }
            else
            {
                progressbar.SetActive(false);
            }

            yield return Timing.WaitForOneFrame;
        }
    }

    //private void Update()
    //{
    //    if (testObject == null) return;

    //    Vector3 objectWorldPosition = testObject.transform.position;

    //    // ���� ��ǥ�� ����Ʈ ��ǥ�� ��ȯ
    //    Vector3 viewportPoint = Camera.main.WorldToViewportPoint(objectWorldPosition);

    //    // ������Ʈ�� ī�޶� ����Ʈ ���� �ִ��� Ȯ��
    //    bool isVisible = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;

    //    if (isVisible)
    //    {
    //        // ������Ʈ�� ���� ��, ��ũ�� ��ǥ�� UI ��ġ ����
    //        Vector3 screenPoint = Camera.main.WorldToScreenPoint(objectWorldPosition);
    //        testUI.GetComponent<RectTransform>().position = screenPoint + Vector3.up * 100f;

    //        // UI Ȱ��ȭ
    //        testUI.SetActive(true);
    //    }
    //    else
    //    {
    //        // ������Ʈ�� ������ ���� ��, UI ��Ȱ��ȭ
    //        testUI.SetActive(false);
    //    }
    //}
}
