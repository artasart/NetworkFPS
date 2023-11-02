using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Panel_HUD : Panel_Base
{
    TMP_Text txtmp_BulletCount;
    TMP_Text txtmp_Health;

    protected override void Awake()
    {
        base.Awake();

        txtmp_BulletCount = transform.Search("txtmp_BulletCount").GetComponent<TMP_Text>();
        txtmp_Health = transform.Search("txtmp_Health").GetComponent<TMP_Text>();
    }

    public void Clear()
    {
        txtmp_BulletCount.text = "";
        txtmp_Health.text = "";
    }

    public void UpdateBulletCount(int currentBullet, int maxBullet)
    {
        txtmp_BulletCount.text = $"{currentBullet} / {maxBullet}";
    }

    public void UpdateHealth(int _health)
    {
        txtmp_Health.text = $"HP : {_health}";
    }

    public override void OnOpen()
    {
        Show(true);
        Cursor.lockState = CursorLockMode.Locked;
        FindObjectOfType<FPSController>()?.LockCameraInput(false);
    }

    public override void OnClose()
    {
        Show(false);
        Cursor.lockState = CursorLockMode.None;
        FindObjectOfType<FPSController>()?.LockCameraInput(true);
    }
}
