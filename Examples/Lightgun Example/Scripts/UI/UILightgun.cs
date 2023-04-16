using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiMouseUnity.Example
{
    /// <summary>
    /// Component that binds itself to a specific device to give gunshot feedback in Menus.
    /// Works in conjunction with the Crosshair class, so that 
    /// every time the Mouse clicks, create a gunshot effect at the Crosshair position.
    /// Unlike the Lightgun class, there is no weapon system, so we don't need to consider ammo etc.
    /// </summary>
    public class UILightgun : MonoBehaviour
    {
        [SerializeField] int deviceID;
        [SerializeField] Crosshair crosshair;
        [SerializeField] GameObject muzzleFlashPrefab;
        [SerializeField] Transform uiTransform;

        private void OnEnable()
        {
            MultiMouseWrapper.OnLeftMouseButtonDown[deviceID] += OnLeftClick;
        }

        private void OnDisable()
        {
            MultiMouseWrapper.OnLeftMouseButtonDown[deviceID] -= OnLeftClick;
        }

        private void OnLeftClick()
        {
            // Instantiating and destroying muzzle flash effects can be computationally expensive!
            // In a real game project, you should consider other methods of creating these effects
            var muzzleFlash = Instantiate(muzzleFlashPrefab, uiTransform);
            muzzleFlash.transform.position = crosshair.transform.position;
            Destroy(muzzleFlash, 1);
        }
    }
}