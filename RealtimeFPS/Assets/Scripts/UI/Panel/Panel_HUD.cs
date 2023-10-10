using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Panel_HUD : Panel_Base
{
    TMP_Text txtmp_BulletCount;
    TMP_Text txtmp_Health;



    bool isTab = false;



    private void OnEnable()
    {
        isTab = false;
    }

    void Start()
    {

    }

    protected override void Awake()
    {
        base.Awake();

        txtmp_BulletCount = GetUI_TMPText(nameof(txtmp_BulletCount), "30 / 30");
        txtmp_Health = GetUI_TMPText(nameof(txtmp_Health), "HP : 100");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isTab = !isTab;

            if (isTab)
            {
                this.Show(false);


                GameManager.UI.StackPanel<Panel_Setting>();
                Cursor.lockState = CursorLockMode.None;

                FindObjectOfType<FPSController>().LockCameraInput(true);
            }

            else
            {
                this.Show(true);

                GameManager.UI.PopPanel();
                Cursor.lockState = CursorLockMode.Locked;

                FindObjectOfType<FPSController>().LockCameraInput(false);
            }
        }
    }

    public void UpdateBulletCount(int _bullet)
    {
        var maxBullet = FindObjectOfType<FPSController>().maxBulletCount;

        txtmp_BulletCount.text = _bullet.ToString() + " / " + maxBullet;
    }

    public void UpdateHealth(int _health)
    {
        txtmp_Health.text = "HP : " + _health.ToString();
    }
}
