using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiMouseUnity.Example
{
    public class MultiMouseInputModule : BaseInputModule
    {
        public int submitButton;

        List<PointerEventData> pointerData = new List<PointerEventData>();

        MultiMouseWrapper multiMouse => MultiMouseWrapper.Instance;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnEnable()
        {
            while (pointerData.Count < multiMouse.ActiveDeviceCount)
            {
                pointerData.Add(new PointerEventData(eventSystem));
            }
            MultiMouseWrapper.OnDeviceFound += OnDeviceFound;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            MultiMouseWrapper.OnDeviceFound -= OnDeviceFound;
            base.OnDisable();
        }

        private void OnDeviceFound(int obj)
        {
            pointerData.Add(new PointerEventData(eventSystem));
        }

        public override void Process()
        {
            for (int i = 0; i < multiMouse.ActiveDeviceCount; i++)
            {
                // Reset data, set camera
                pointerData[i].Reset();

                pointerData[i].position = multiMouse.GetMousePosition(i);

                // Raycast
                eventSystem.RaycastAll(pointerData[i], m_RaycastResultCache);
                pointerData[i].pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
                var selectedObject = pointerData[i].pointerCurrentRaycast.gameObject;

                m_RaycastResultCache.Clear();

                HandlePointerExitAndEnter(pointerData[i], selectedObject);

                if (multiMouse.GetMouseButtonDown(i, submitButton))
                {
                    ProcessPress(pointerData[i], selectedObject);
                } 
                else if (multiMouse.GetMouseButtonUp(i, submitButton))
                {
                    ProcessRelease(pointerData[i], selectedObject);
                }
            }
        }

        void ProcessPress(PointerEventData data, GameObject selectedObject)
        {
            // Set raycast
            data.pointerPressRaycast = data.pointerCurrentRaycast;

            // Check for object hit, get the down handler, call
            GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(selectedObject, data, ExecuteEvents.pointerDownHandler);

            // If no down handler, try and get click handler
            if (newPointerPress == null)
            {
                newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(selectedObject);
            }

            // Set data
            data.pressPosition = data.position;
            data.pointerPress = newPointerPress;
            data.rawPointerPress = selectedObject;
        }

        private void ProcessRelease(PointerEventData data, GameObject selectedObject)
        {
            // Execute pointer up
            ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

            // Check click handler
            GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(selectedObject);

            // Check if actual
            if (data.pointerPress == pointerUpHandler)
            {
                ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
            }

            // Reset data
            data.pressPosition = Vector2.zero;
            data.pointerPress = null;
            data.rawPointerPress = null;
        }
    }
}