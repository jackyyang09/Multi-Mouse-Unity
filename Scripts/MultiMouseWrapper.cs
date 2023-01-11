using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiMouse
{
    //public enum DeviceDetectionMethod
    //{
    //    Movement,
    //    MouseButton,
    //    MovementOrMouseButton
    //}

    [System.Serializable]
    public class MultiMouseDevice
    {
        public string DeviceID;
        public Vector2 Position;
        public bool IsLightgun;
    }

    [RequireComponent(typeof(MultiMouse))]
    public class MultiMouseWrapper : MonoBehaviour
    {
        [SerializeField] bool unclampMousePosition;
        [SerializeField] bool detectDevicesOnStart = true;
        [SerializeField] int maxDevices = 4;
        [SerializeField] bool keepDetectingUntilFull = true;

        List<MultiMouseDevice> activeDevices = new List<MultiMouseDevice>();
        Dictionary<string, MultiMouseDevice> idToDevice = new Dictionary<string, MultiMouseDevice>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public MultiMouseDevice TryGetDeviceAtIndex(int index)
        {
            if (index >= activeDevices.Count) return null;
            return activeDevices[index];
        }

        const int MAX_BUTTONS = 4;
        /// <summary>
        /// Only supports up to 4 buttons
        /// for performance and practicality
        /// </summary>
        List<bool[]> isButtonDown = new List<bool[]>();

        bool mouseHeld;

        static MultiMouseWrapper instance;
        public static MultiMouseWrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MultiMouseWrapper>();
                    if (instance == null)
                    {
                        Debug.LogWarning("MultiMouseWrapper not found in this scene!");
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// <para>Invoked after a device is found with StartDetectingDevice()</para>
        /// int mouseID - The ID of the device found
        /// </summary>
        public static System.Action<int> OnDeviceFound;

        #region Events
        static System.Action[] onLeftMouseButtonDown;
        public static System.Action[] OnLeftMouseButtonDown
        {
            get
            {
                if (onLeftMouseButtonDown == null)
                {
                    onLeftMouseButtonDown = new System.Action[Instance.maxDevices];
                }
                return onLeftMouseButtonDown;
            }
        }

        static System.Action[] onLeftMouseButtonUp;
        public static System.Action[] OnLeftMouseButtonUp
        {
            get
            {
                if (onLeftMouseButtonUp == null)
                {
                    onLeftMouseButtonUp = new System.Action[Instance.maxDevices];
                }
                return onLeftMouseButtonUp;
            }
        }

        static System.Action[] onRightMouseButtonDown;
        public static System.Action[] OnRightMouseButtonDown
        {
            get
            {
                if (onRightMouseButtonDown == null)
                {
                    onRightMouseButtonDown = new System.Action[Instance.maxDevices];
                }
                return onRightMouseButtonDown;
            }
        }

        static System.Action[] onRightMouseButtonUp;
        public static System.Action[] OnRightMouseButtonUp
        {
            get
            {
                if (onRightMouseButtonUp == null)
                {
                    onRightMouseButtonUp = new System.Action[Instance.maxDevices];
                }
                return onRightMouseButtonUp;
            }
        }
        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            UnityEngine.LowLevel.PlayerLoopSystem playerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
            // Debug.Assert(PlayerLoopUtils.AddToPlayerLoop(EndOfFrameUpdate, typeof(LightgunInput), ref playerLoop, typeof(PreUpdate.NewInputUpdate), PlayerLoopUtils.AddMode.End));
            Debug.Assert(PlayerLoopUtils.AddToPlayerLoop(StaticUpdate, typeof(MultiMouseWrapper), ref playerLoop, typeof(UnityEngine.PlayerLoop.EarlyUpdate.UpdateInputManager), PlayerLoopUtils.AddMode.Beginning));
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private void Start()
        {
            if (transform.parent != null)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(this);
            }

            if (detectDevicesOnStart)
            {
                detectDeviceRoutine = StartCoroutine(DetectDevice());
            }
        }

        static void StaticUpdate()
        {
            if (instance) instance.UpdateDevices();
        }

        private void UpdateDevices()
        {
            for (int i = 0; i < activeDevices.Count; i++)
            {
                if (detectDeviceRoutine == null)
                {
                    MultiMouse.MultiMouse_Poll(activeDevices[i].DeviceID);
                }

                for (int j = 1; j <= MAX_BUTTONS; j++)
                {
                    if (MultiMouse.GetMouseButtonDown(activeDevices[i].DeviceID, j))
                    {
                        isButtonDown[i][j] = true;
                        switch (j)
                        {
                            case 1:
                                OnLeftMouseButtonDown[i]?.Invoke();
                                break;
                            case 2:
                                OnRightMouseButtonDown[i]?.Invoke();
                                break;
                        }
                    }
                    else if (MultiMouse.GetMouseButtonUp(activeDevices[i].DeviceID, j))
                    {
                        isButtonDown[i][j] = false;
                        switch (j)
                        {
                            case 1:
                                OnLeftMouseButtonUp[i]?.Invoke();
                                break;
                            case 2:
                                OnRightMouseButtonUp[i]?.Invoke();
                                break;
                        }
                    }
                }
            }
        }

        [ContextMenu(nameof(StartDetectingDevice))]
        public void StartDetectingDevice()
        {
            if (detectDeviceRoutine != null) StopCoroutine(detectDeviceRoutine);
            detectDeviceRoutine = StartCoroutine(DetectDevice());
        }

        Coroutine detectDeviceRoutine;
        IEnumerator DetectDevice()
        {
            do
            {
                string id = "";

                while (string.IsNullOrEmpty(id))
                {
                    id = MultiMouse.GetAnyMousePressingButton(1);
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (idToDevice.ContainsKey(id)) id = "";
                    }
                    yield return null;
                }

                Debug.Log("MultiMouse: Found device of ID " + id);
                var newDevice = new MultiMouseDevice();
                newDevice.DeviceID = id;
                newDevice.IsLightgun = MultiMouse.GetAbsoluteMousePosition(id) != Vector2.zero;
                newDevice.Position = new Vector2(
                    (float)Screen.width / 2f,
                    (float)Screen.height / 2f);
                activeDevices.Add(newDevice);
                idToDevice.Add(id, newDevice);
                isButtonDown.Add(new bool[MAX_BUTTONS]);

                OnDeviceFound?.Invoke(activeDevices.Count - 1);
            }
            while (keepDetectingUntilFull && activeDevices.Count < maxDevices);

            detectDeviceRoutine = null;
        }

        /// <summary>
        /// Returns true if the button is currently held down
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="buttonIndex"></param>
        /// <returns></returns>
        public bool GetMouseButton(int deviceID, int buttonIndex)
        {
            if (isButtonDown.Count <= deviceID) return false;
            return isButtonDown[deviceID][buttonIndex + 1];
        }

        /// <summary>
        /// Returns true on the frame the button is pressed
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="buttonIndex"></param>
        /// <returns></returns>
        public bool GetMouseButtonDown(int deviceID, int buttonIndex)
        {
            var device = TryGetDeviceAtIndex(deviceID);
            if (device == null) return false;

            return MultiMouse.GetMouseButtonDown(device.DeviceID, buttonIndex + 1);
        }

        /// <summary>
        /// Returns true on the frame the button is released
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="buttonIndex"></param>
        /// <returns></returns>
        public bool GetMouseButtonUp(int deviceID, int buttonIndex)
        {
            var device = TryGetDeviceAtIndex(deviceID);
            if (device == null) return false;

            return MultiMouse.GetMouseButtonUp(device.DeviceID, buttonIndex + 1);
        }

        public Vector2 GetMousePosition(int deviceID)
        {
            var device = TryGetDeviceAtIndex(deviceID);
            if (device == null) return Vector2.zero;

            if (device.IsLightgun)
            {
                var pos = MultiMouse.GetAbsoluteMousePosition(device.DeviceID);
                var rect = GetNormalizedUISpaceContainerRect();

                pos.x = pos.x / (float)ushort.MaxValue;
                pos.y = pos.y / (float)ushort.MaxValue;

                var x = Mathf.InverseLerp(rect.xMin, rect.xMax, pos.x);
                var y = Mathf.InverseLerp(rect.yMax, rect.yMin, pos.y);

                device.Position = new Vector2(x * Screen.width, y * Screen.height);
                Debug.Log(device.Position + " " + rect);
            }   
            else
            {
                device.Position += MultiMouse.GetRelativeMousePosition(device.DeviceID);
                if (!unclampMousePosition)
                {
                    device.Position = new Vector2(
                        Mathf.Clamp(device.Position.x, 0, Screen.width),
                        Mathf.Clamp(device.Position.y, 0, Screen.height));
                }
            }
            return device.Position;
        }

#if UNITY_EDITOR
        static EditorWindow gameView;
        static EditorWindow GameView
        {
            get
            {
                if (gameView == null)
                {
                    gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
                }
                return gameView;
            }
        }

        const int TAB_HEIGHT = 22;
#endif

        /// <summary>
        /// Uses top left as 0,0 and bottom right as 1,1, as standard mouse input/unity editor ui does.
        /// Thanks tomkail
        /// </summary>
        /// <returns></returns>
        static Rect GetNormalizedUISpaceContainerRect()
        {
            var containerRect = new Rect(0, 0, Screen.width, Screen.height);
#if UNITY_EDITOR
            containerRect = new Rect(GameView.position.x, GameView.position.y + TAB_HEIGHT, GameView.position.width, GameView.position.height - TAB_HEIGHT);
#elif UNITY_STANDALONE_WIN
            containerRect = WindowsUtil.GetWindowPosition();
#endif
            var displayInfo = Screen.mainWindowDisplayInfo;
            return new Rect(containerRect.x / displayInfo.width, containerRect.y / displayInfo.height, containerRect.width / displayInfo.width, containerRect.height / displayInfo.height);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MultiMouse))]
    class MultiMouseInterfaceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
        }
    }
#endif
}