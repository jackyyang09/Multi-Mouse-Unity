using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiMouseUnity.Example
{
    public class ViewModelRotator : MonoBehaviour
    {
        [SerializeField] int deviceID;

        [SerializeField] float gunYAxisMin, gunYAxisMax;
        [SerializeField] float gunXAxisMin, gunXAxisMax;

        [SerializeField] Renderer[] viewModelMeshes;
        [SerializeField] Animation anim;
        Lightgun lightgun;

        private void Start()
        {
            SetViewModelVisible(false);
        }

        public void Initialize(int id, Lightgun l)
        {
            deviceID = id;
            lightgun = l;
            lightgun.OnShoot += OnShoot;
            SetViewModelVisible(true);
        }

        private void OnDisable()
        {
            if (lightgun)
            {
                lightgun.OnShoot -= OnShoot;
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
            if (!MultiMouseWrapper.Instance.IsMouseActive(deviceID)) return;

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

        private void OnShoot()
        {
            if (anim.isPlaying) anim.Stop();
            anim.Play();
        }
    }
}