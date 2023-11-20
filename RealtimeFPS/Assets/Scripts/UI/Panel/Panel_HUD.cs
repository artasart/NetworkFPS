using UnityEngine;
using TMPro;
using Demo.Scripts.Runtime;
using Newtonsoft.Json.Converters;
using Unity.VisualScripting;

public class Panel_HUD : Panel_Base
{
    TMP_Text txtmp_BulletCount;
    TMP_Text txtmp_Health;

    FPSController controllerComponent;

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

    public void SetController(FPSController controllerComponent)
    {
        this.controllerComponent = controllerComponent;
        this.controllerComponent.OnFire += OnFire;
        this.controllerComponent.OnReload += OnReload;
        this.controllerComponent.OnChangeWeapon += OnChangeWeapon;

        var gun = controllerComponent.GetGun();
        SetAmmoUI(gun.currentAmmo, gun.maxAmmo);
    }

    public void OnFire()
    {
        var gun = controllerComponent.GetGun();
        SetAmmoUI(gun.currentAmmo, gun.maxAmmo);
    }

    public void OnReload()
    {
        var gun = controllerComponent.GetGun();
        SetAmmoUI(gun.maxAmmo, gun.maxAmmo);
    }

    public void OnChangeWeapon(int weaponId)
    {
        var gun = controllerComponent.GetGun();
        SetAmmoUI(gun.currentAmmo, gun.maxAmmo);
    }

    public void SetAmmoUI(int current, int max)
    {
        txtmp_BulletCount.text = $"{current} / {max}";
    }

    public void UpdateHealth(int _health)
    {
        txtmp_Health.text = $"HP : {_health}";
    }

    public override void OnOpen()
    {
        Show(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        controllerComponent?.enableInput(true);
    }

    public override void OnClose()
    {
        Show(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        controllerComponent?.enableInput(false);
    }
}
