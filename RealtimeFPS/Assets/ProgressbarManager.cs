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

    //    // 월드 좌표를 뷰포트 좌표로 변환
    //    Vector3 viewportPoint = Camera.main.WorldToViewportPoint(objectWorldPosition);

    //    // 오브젝트가 카메라 뷰포트 내에 있는지 확인
    //    bool isVisible = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;

    //    if (isVisible)
    //    {
    //        // 오브젝트가 보일 때, 스크린 좌표로 UI 위치 조정
    //        Vector3 screenPoint = Camera.main.WorldToScreenPoint(objectWorldPosition);
    //        testUI.GetComponent<RectTransform>().position = screenPoint + Vector3.up * 100f;

    //        // UI 활성화
    //        testUI.SetActive(true);
    //    }
    //    else
    //    {
    //        // 오브젝트가 보이지 않을 때, UI 비활성화
    //        testUI.SetActive(false);
    //    }
    //}
}
