using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiMouseUnity.Example
{
    public class Crosshair : MonoBehaviour
    {
        [SerializeField] int deviceID;

        private void OnEnable()
        {
            MultiMouseWrapper.OnLeftMouseButtonDown[deviceID] += HideCursor;
        }

        private void OnDisable()
        {
            MultiMouseWrapper.OnLeftMouseButtonDown[deviceID] -= HideCursor;
        }

        public void SetDeviceID(int id) => deviceID = id;

        private void HideCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        void Update()
        {
            if (!MultiMouseWrapper.Instance.IsMouseActive(deviceID)) return;
            transform.position = MultiMouseWrapper.Instance.GetMousePosition(deviceID);
        }
    }
}