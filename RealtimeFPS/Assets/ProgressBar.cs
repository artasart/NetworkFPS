using MEC;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    Image Progressbar_Right;
    Image Progressbar_Left;
    
    TMP_Text time;

    int halfProgress = 1500;
    int maxProgress = 3000;
    float currentProgress = 0;

    float interval = 0.05f;

    CoroutineHandle updateProgressState;

    void Start()
    {
        Progressbar_Right = transform.Search(nameof(Progressbar_Right)).GetComponent<Image>();
        Progressbar_Left = transform.Search(nameof(Progressbar_Left)).GetComponent<Image>();
        time = transform.Search(nameof(Time)).GetComponent<TMP_Text>();

        NetworkManager.Instance.Client.packetHandler.AddHandler(OnItemOccupyProgressState);   
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnItemOccupyProgressState);
    }

    private void OnItemOccupyProgressState(Protocol.S_FPS_ITEM_OCCUPY_PROGRESS_STATE pkt)
    {
        if (updateProgressState.IsRunning)
            Timing.KillCoroutines(updateProgressState);

        updateProgressState = Timing.RunCoroutine(UpdateProgressBar(pkt.OccupyProgressState));
    }

    private IEnumerator<float> UpdateProgressBar(float endProgresss)
    {
        float delTime = 0.0f;

        float startProgress = currentProgress;

        while (delTime < interval)
        {
            delTime += Time.deltaTime;

            currentProgress = Mathf.Lerp(startProgress, endProgresss, delTime / interval);

            Progressbar_Right.fillAmount = Mathf.Min(currentProgress / halfProgress, 1);
            Progressbar_Left.fillAmount = Mathf.Max((float)(currentProgress - halfProgress) / halfProgress, 0);

            float remainTime = (float)(maxProgress - currentProgress) / 1000;
            time.text = remainTime.ToString("F2");

            yield return Timing.WaitForOneFrame;
        }
    }
}
