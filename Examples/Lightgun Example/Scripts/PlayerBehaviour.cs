using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiMouseUnity.Example
{
    public class PlayerBehaviour : MonoBehaviour
    {
        [SerializeField] WeaponObject weapon;
        int ammo;

        [SerializeField] int playerIndex;
        [SerializeField] int fireButton = 0;
        [SerializeField] int reloadButton = 1;
        int deviceID = -1;

        [Header("Object References")]
        [SerializeField] Canvas canvas;
        [SerializeField] Lightgun lightgun;
        [SerializeField] Crosshair crossHair;
        [SerializeField] ViewModelRotator viewModel;

        [Header("UI")]
        [SerializeField] CanvasGroup waitingUI;
        [SerializeField] CanvasGroup gameUI;
        [SerializeField] UnityEngine.UI.Text weaponLabel;
        [SerializeField] UnityEngine.UI.Text ammoCount;
        [SerializeField] UnityEngine.UI.Image ammoFill;

        float lastFireTime;

        private void Start()
        {
            ammo = weapon.ammoCapacity;
            UpdateAmmoUI();

            if (MainMenuUI.PlayData == null)
            {
                deviceID = playerIndex;
            }
            else
            {
                if (MultiMouseWrapper.Instance.ActiveDeviceCount > 0)
                {
                    deviceID = MainMenuUI.PlayData.playerIDs[playerIndex];
                    if (deviceID > -1)
                    {
                        OnDeviceFound(deviceID);
                    }
                }
            }
        }

        private void OnEnable()
        {
            MultiMouseWrapper.OnDeviceFound += OnDeviceFound;
        }

        private void OnDisable()
        {
            MultiMouseWrapper.OnDeviceFound -= OnDeviceFound;
            if (deviceID > -1)
            {
                MultiMouseWrapper.OnLeftMouseButtonDown[deviceID] -= HideCursor;
                MultiMouseWrapper.OnRightMouseButtonDown[deviceID] -= Reload;
            }
        }

        private void Update()
        {
            if (Time.time < lastFireTime + weapon.fireDelay) return;
            if (!MultiMouseWrapper.Instance.IsMouseActive(deviceID)) return;

            switch (weapon.weaponType)
            {
                case FireType.SemiAuto:
                    if (MultiMouseWrapper.Instance.GetMouseButtonDown(deviceID, fireButton))
                    {
                        PullTrigger();
                    }
                    break;
                case FireType.FullAuto:
                    if (MultiMouseWrapper.Instance.GetMouseButton(deviceID, fireButton))
                    {
                        PullTrigger();
                    }
                    break;
            }
        }

        void PullTrigger()
        {
            if (ammo == 0) return;

            lightgun.Shoot(weapon);
            ammo--;
            UpdateAmmoUI();
            lastFireTime = Time.time;
        }

        private void OnDeviceFound(int mouseID)
        {
            if (mouseID == deviceID)
            {
                waitingUI.alpha = 0;
                gameUI.alpha = 1;
                crossHair.SetDeviceID(deviceID);
                viewModel.Initialize(deviceID, lightgun);
                MultiMouseWrapper.OnLeftMouseButtonDown[deviceID] += HideCursor;
                MultiMouseWrapper.OnRightMouseButtonDown[deviceID] += Reload;
            }
        }

        private void Reload()
        {
            ammo = weapon.ammoCapacity;
            UpdateAmmoUI();
        }

        private void HideCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void UpdateAmmoUI()
        {
            weaponLabel.text = weapon.name;

            ammoCount.text = ammo.ToString();
            ammoFill.fillAmount = (float)ammo / (float)weapon.ammoCapacity;

            ammoCount.color = ammoFill.fillAmount <= 0.2f ? Color.red : Color.white;
            ammoFill.color = ammoFill.fillAmount <= 0.2f ? Color.red : Color.white;
        }
    }
}