using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace MultiMouseUnity
{
    /// <summary>
    /// MultiMouseLib created by Mystery Wizard
    /// </summary>
    class MultiMouse
    {
        #region External Methods
        private static IntPtr _currentWindowHandle = IntPtr.Zero;

        static IntPtr UnityWindowHandle
        {
            get
            {
                if (_currentWindowHandle == IntPtr.Zero)
#if UNITY_EDITOR
                    _currentWindowHandle = GetActiveWindow();
#else
                    _currentWindowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
#endif
                return _currentWindowHandle;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        /// <summary>
        /// In some cases in Unity based games or other games where you do not have control over the window, you may want to set bUseInternalWindow to false or 0.
        /// This will cause the library to use the Windows API SetWindowLong() in order to hijack the current window's window input handling.
        /// The hWnd handle is that of the Unity window, there is example code for finding the Unity window within this test application.
        /// </summary>
        /// <param name="bUseInternalWindow"></param>
        /// <param name="bImmediateCapture"></param>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("MultiMouseLib.dll")]
        static extern bool MultiMouse_Init(byte bUseInternalWindow, byte bImmediateCapture, IntPtr hWnd);

        /*
         * DevId is the PID/VID combination as a string for the device, MultiMouse_DetectDevice facilitates a way to grab this automatically when a button is pressed on any device.
         */
        [DllImport("MultiMouseLib.dll")]
        static extern void MultiMouse_Poll(string mDevId);

        /// <summary>
        /// This will only trigger once per Poll event if the button transition state changes, this will not tell you if the button is still down only when it has been pressed.
        /// </summary>
        /// <param name="mDevId"></param>
        /// <param name="buttonIdx"></param>
        /// <returns></returns>
        [DllImport("MultiMouseLib.dll")]
        static extern bool MultiMouse_ButtonDown(string mDevId, byte buttonIdx);

        /// <summary>
        /// After receiving a button down event, this can be listened/polled for in order to determine if a previously pressed button has been lifted.
        /// </summary>
        /// <param name="mDevId"></param>
        /// <param name="buttonIdx"></param>
        /// <returns></returns>
        [DllImport("MultiMouseLib.dll")]
        static extern bool MultiMouse_ButtonUp(string mDevId, byte buttonIdx);

        /// <summary>
        /// Used to facilitate getting the most accurate/most recent and up to date absolute coordinates for the pointer device.
        /// This will not take any older values into account, and will return whatever has most recently been received for the specified device.</summary>
        /// <param name="mDevId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [DllImport("MultiMouseLib.dll")]
        static extern void MultiMouse_GetAbsCords(string mDevId, ref int x, ref int y);

        /// <summary>
        /// This is for non-pointing devices which return relative coordinates, all coordinates sent since the last poll are automatically summarized in a single delta.
        /// </summary>
        /// <param name="mDevId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [DllImport("MultiMouseLib.dll")]
        static extern void MultiMouse_GetRelativeCords(string mDevId, ref int x, ref int y);

        [DllImport("MultiMouseLib.dll")]
        static extern bool MultiMouse_Destroy(IntPtr hWnd);

        /// <summary>
        /// Warning this will automatically poll for you, you do not need to call MultiMouse_Poll while calling this but, You should still be calling it once per frame.
        /// This should be considered a universal "MultiMouse_ButtonDown" utilized for device detection.
        /// 
        /// The string builder object utilized as the second parameter will be filled with the device pressing the specified button at which point you've "Detected" the device and can stop calling this method.
        /// The string builder object specified should be pre-allocated before being sent to the method like so; StringBuilder sb = new StringBuilder(18);
        /// The 18 characters is a hard limit, it can be no less than this.
        /// </summary>
        /// <param name="buttonIdx"></param>
        /// <param name="devId"></param>
        [DllImport("MultiMouseLib.dll")]
        static extern void MultiMouse_DetectDevice(byte buttonIdx, StringBuilder devId);
        #endregion

        const byte bUseInternalWindow = 1;
        const byte bImmediateCapture = 1;

        static bool initialized;
        public static bool Initialized => initialized;

        static bool Init_MultiMouse()
        {
            bool success = MultiMouse_Init(bUseInternalWindow, bImmediateCapture, UnityWindowHandle);
            if (success) Debug.Log("MultiMouse initialized successfully");
            else Debug.LogWarning("MultiMouse failed to initialize!");
            return success;
        }

        public static void Initialize()
        {
            initialized = Init_MultiMouse();
        }

        public static void MultiMousePoll(string mDevId)
        {
            MultiMouse_Poll(mDevId);
        }

        public static string GetAnyMousePressingButton(int mouseButton)
        {
            StringBuilder sb = new StringBuilder(18);

            MultiMouse_DetectDevice((byte)mouseButton, sb);

            return sb.ToString();
        }

        public static bool GetMouseButtonDown(string deviceID, int button)
        {
            return MultiMouse_ButtonDown(deviceID, (byte)button);
        }

        public static bool GetMouseButtonUp(string deviceID, int button)
        {
            return MultiMouse_ButtonUp(deviceID, (byte)button);
        }

        public static Vector2 GetAbsoluteMousePosition(string deviceID)
        {
            int x = 0, y = 0;
            MultiMouse_GetAbsCords(deviceID, ref x, ref y);
            return new Vector2(x, y);
        }

        public static Vector2 GetRelativeMousePosition(string deviceID)
        {
            int x = 0, y = 0;
            MultiMouse_GetRelativeCords(deviceID, ref x, ref y);
            return new Vector2(x, -y);
        }

        public static void Destroy()
        {
            MultiMouse_Destroy(UnityWindowHandle);
        }
    }
}