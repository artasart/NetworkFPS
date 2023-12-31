// Designed by Kinemation, 2023

using UnityEngine;

namespace Demo.Scripts.Runtime
{
    public class AnimEventReceiver_Dummy : MonoBehaviour
    {
        [SerializeField] private FPSController_Remote controller;

        private void Start()
        {
            if (controller == null)
            {
                controller = GetComponentInParent<FPSController_Remote>();
            }
        }
        
        //public void SetActionActive(int isActive)
        //{
        //    controller.SetActionActive(isActive);
        //}

        //public void ChangeWeapon()
        //{
        //    //controller.EquipWeapon();
        //}

        //public void RefreshStagedState()
        //{
        //    controller.RefreshStagedState();
        //}

        //public void ResetStagedState()
        //{
        //    controller.ResetStagedState();
        //}
    }
}