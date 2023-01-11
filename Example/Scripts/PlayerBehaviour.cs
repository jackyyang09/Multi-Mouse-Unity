using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiMouse.Example
{
    public class PlayerBehaviour : MonoBehaviour
    {
        [SerializeField] WeaponObject weapon;
        int ammo;

        [SerializeField] int deviceID;
        [SerializeField] int fireButton = 0;
        [SerializeField] int reloadButton = 1;

        [Header("Object References")]
        [SerializeField] Canvas canvas;
        [SerializeField] Lightgun lightgun;

        [Header("UI")]
        [SerializeField] CanvasGroup waitingUI;
        [SerializeField] CanvasGroup gameUI;
        [SerializeField] UnityEngine.UI.Text weaponLabel;
        [SerializeField] UnityEngine.UI.Text ammoCount;
        [SerializeField] UnityEngine.UI.Image ammoFill;

        MultiMouseDevice multiMouseDevice;

        float lastFireTime;

        private void Start()
        {
            ammo = weapon.ammoCapacity;
            UpdateAmmoUI();
        }

        private void OnEnable()
        {
            MultiMouseWrapper.OnDeviceFound += OnDeviceFound;
            MultiMouseWrapper.OnLeftMouseButtonDown[deviceID] += HideCursor;
            MultiMouseWrapper.OnRightMouseButtonDown[deviceID] += Reload;
        }

        private void OnDisable()
        {
            MultiMouseWrapper.OnDeviceFound -= OnDeviceFound;
            MultiMouseWrapper.OnLeftMouseButtonDown[deviceID] -= HideCursor;
            MultiMouseWrapper.OnRightMouseButtonDown[deviceID] -= Reload;
        }

        private void Update()
        {
            if (Time.time < lastFireTime + weapon.fireDelay) return;

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
                multiMouseDevice = MultiMouseWrapper.Instance.TryGetDeviceAtIndex(mouseID);
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