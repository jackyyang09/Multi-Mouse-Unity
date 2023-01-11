using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiMouse.Example
{
    public class ViewmodelRotator : MonoBehaviour
    {
        [SerializeField] int deviceID;

        [SerializeField] float gunYAxisMin, gunYAxisMax;
        [SerializeField] float gunXAxisMin, gunXAxisMax;

        [SerializeField] Renderer[] viewModelMeshes;

        private void OnEnable()
        {
            MultiMouseWrapper.OnDeviceFound += OnDeviceFound;
        }

        private void OnDisable()
        {
            MultiMouseWrapper.OnDeviceFound -= OnDeviceFound;
        }

        private void Start()
        {
            SetViewModelVisible(false);
        }

        void OnDeviceFound(int id)
        {
            if (deviceID == id)
            {
                SetViewModelVisible(true);
            }
        }

        void SetViewModelVisible(bool visible)
        {
            foreach (var item in viewModelMeshes)
            {
                item.enabled = visible;
            }
        }

        private void Update()
        {
            var mousePos = MultiMouseWrapper.Instance.GetMousePosition(deviceID);
            var eulerAngles = transform.localEulerAngles;

            // Due to how rotation works, we set X to Y and Y to X
            var screenWidth = Screen.width;
            var lerpY = Mathf.InverseLerp(0, screenWidth, mousePos.x);
            eulerAngles.y = Mathf.Lerp(gunYAxisMin, gunYAxisMax, lerpY);

            var screenHeight = Screen.width;
            var lerpX = Mathf.InverseLerp(screenHeight, 0, mousePos.y);
            eulerAngles.x = Mathf.Lerp(gunYAxisMin, gunYAxisMax, lerpX);

            transform.localEulerAngles = eulerAngles;
        }
    }
}