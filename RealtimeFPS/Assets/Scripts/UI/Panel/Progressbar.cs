using MEC;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Progressbar : Panel_Base
{
    Image Progressbar_Left;
    Image Progressbar_Right;

    int halfProgress = 1500;
    float currentProgress = 0;

    float interval = 0.05f;

    CoroutineHandle updateProgressState;

    protected override void Awake()
    {
        base.Awake();

        Progressbar_Left = transform.Search(nameof(Progressbar_Left)).GetComponent<Image>();
        Progressbar_Right = transform.Search(nameof(Progressbar_Right)).GetComponent<Image>();
    }

    private void Start()
    {
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnItemOccupyProgressState);
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnItemOccupyProgressState);
    }

    public override void OnOpen()
    {
        Progressbar_Left.fillAmount = 0;
        Progressbar_Right.fillAmount = 0;
    }

    private void OnItemOccupyProgressState( Protocol.S_FPS_ITEM_OCCUPY_PROGRESS_STATE pkt )
    {
        if (updateProgressState.IsRunning)
            Timing.KillCoroutines(updateProgressState);

        updateProgressState = Timing.RunCoroutine(UpdateProgressBar(pkt.OccupyProgressState));
    }

    private IEnumerator<float> UpdateProgressBar( float endProgresss )
    {
        float delTime = 0.0f;

        float startProgress = currentProgress;

        while (delTime < interval)
        {
            delTime += UnityEngine.Time.deltaTime;

            currentProgress = Mathf.Lerp(startProgress, endProgresss, delTime / interval);

            Progressbar_Left.fillAmount = Mathf.Min(currentProgress / halfProgress, 1);
            Progressbar_Right.fillAmount = Mathf.Max((float)(currentProgress - halfProgress) / halfProgress, 0);

            yield return Timing.WaitForOneFrame;
        }
    }
}
