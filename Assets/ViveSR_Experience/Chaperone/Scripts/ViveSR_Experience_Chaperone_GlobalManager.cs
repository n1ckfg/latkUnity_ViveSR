using UnityEngine;
using Valve.VR;
using System;
using System.Runtime.InteropServices;

namespace Vive.Plugin.SR.Experience.Chaperone
{
    public enum Status { Idle, Start, Work, Error }
    public class ViveSR_Experience_Chaperone_GlobalManager : MonoBehaviour
    {

        public ViveSR_Experience_Chaperone viveSR;
        public ViveSR_Experience_Chaperone_UIManager UIMgr;

        public static Status EnvironmentStatus { get; private set; }
        public static Status ChaperoneStatus { get; private set; }
        public string LastErrMsg;

        #region singleton
        private ViveSR_Experience_Chaperone_GlobalManager() { }
        private static ViveSR_Experience_Chaperone_GlobalManager Mgr = null;
        public static ViveSR_Experience_Chaperone_GlobalManager Instance
        {
            get
            {
                if (Mgr == null)
                {
                    Mgr = FindObjectOfType<ViveSR_Experience_Chaperone_GlobalManager>();
                }
                if (Mgr == null)
                {
                    Debug.LogError("GlobalManager does not be attached on GameObject");
                }
                return Mgr;
            }
        }
        #endregion

        private Rect WindowRect = new Rect(10, 10, 210, 140);
        private MousePosition MouseDonwPosition;
        private MousePosition MouseMovePosition;

        private int lastValue = 0;

        // Use this for initialization
        void Start()
        {
            Screen.SetResolution((int)WindowRect.width, (int)WindowRect.height, false);
            SetPosition((int)WindowRect.x, (int)WindowRect.y);

            viveSR.OnStartFailed.Add(StartChaperoneCallback);
            viveSR.OnStartComplete.Add(StartChaperoneCallback);
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                GetCursorPos(out MouseDonwPosition);
            }
            else if (Input.GetMouseButton(0))
            {
                GetCursorPos(out MouseMovePosition);
                WindowRect.x += MouseMovePosition.x - MouseDonwPosition.x;
                WindowRect.y += MouseMovePosition.y - MouseDonwPosition.y;
                MouseDonwPosition = MouseMovePosition;
                SetPosition((int)WindowRect.x, (int)WindowRect.y);
            }

            switch (EnvironmentStatus)
            {
                case Status.Idle:
                    if (IsAvailable(out LastErrMsg)) EnvironmentStatus = Status.Work;
                    else EnvironmentStatus = Status.Error;
                    break;
                case Status.Start:
                    break;
                case Status.Work:
                    break;
                case Status.Error:
                    if (IsAvailable(out LastErrMsg)) EnvironmentStatus = Status.Work;
                    break;
            }
            switch (ChaperoneStatus)
            {
                case Status.Idle:
                    break;
                case Status.Start:
                    break;
                case Status.Work:
                    break;
                case Status.Error:
                    break;
            }

            if (EnvironmentStatus != Status.Error && ChaperoneStatus != Status.Error) LastErrMsg = "";
        }

        private bool IsAvailable(out string errMsg)
        {
            if (viveSR == null || UIMgr == null)
            {
                errMsg = "Please check necessary references";
                Debug.Log(errMsg);
                return false;
            }

            var err = EVRInitError.None;
            OpenVR.Init(ref err, EVRApplicationType.VRApplication_Overlay);
            if (err != EVRInitError.None)
            {
                errMsg = "SteamVR service is null";
                Debug.Log(errMsg);
                return false;
            }
            //if (SteamVR.instance == null)
            //{
            //    errMsg = "SteamVR service is null";
            //    Debug.Log(errMsg);
            //    return false;
            //}
            if (OpenVR.Overlay == null)
            {
                errMsg = "Overlay service is null";
                Debug.Log(errMsg);
                return false;
            }
            else if (OpenVR.TrackedCamera == null)
            {
                errMsg = "TrackedCamera service is null";
                Debug.Log(errMsg);
                return false;
            }
            else
            {
                uint deviceIndex = OpenVR.k_unTrackedDeviceIndex_Hmd;
                ulong _handle = 0;
                bool _hasCamera = false;
                EVRTrackedCameraError error = EVRTrackedCameraError.None;

                error = OpenVR.TrackedCamera.HasCamera(deviceIndex, ref _hasCamera);
                if (error != EVRTrackedCameraError.None || !_hasCamera)
                {
                    if (!_hasCamera) errMsg = "Could not found the camera";
                    else errMsg = "Found the camera error " + (int)error;
                    return false;
                }
                error = OpenVR.TrackedCamera.AcquireVideoStreamingService(deviceIndex, ref _handle);
                if (error != EVRTrackedCameraError.None || _handle == 0)
                {
                    errMsg = "Video Streaming Service error " + (int)error;
                    return false;
                }

                CameraVideoStreamFrameHeader_t header = new CameraVideoStreamFrameHeader_t();
                uint width = 0, height = 0, frameSize = 0, headerSize = (uint)Marshal.SizeOf(header.GetType());
                error = OpenVR.TrackedCamera.GetCameraFrameSize(deviceIndex, EVRTrackedCameraFrameType.Distorted, ref width, ref height, ref frameSize);
                IntPtr ptrFrame = Marshal.AllocCoTaskMem((int)frameSize);
                if (error != EVRTrackedCameraError.None)
                {
                    errMsg = "Get Frame Size error " + (int)error;
                    return false;
                }

                error = OpenVR.TrackedCamera.GetVideoStreamFrameBuffer(_handle, EVRTrackedCameraFrameType.Distorted, ptrFrame, frameSize, ref header, headerSize);
                int[] rgba = new int[1];
                Marshal.Copy(ptrFrame, rgba, 0, rgba.Length);
                Marshal.FreeCoTaskMem(ptrFrame);
                bool pixel = lastValue != 0 && lastValue != rgba[0];
                lastValue = rgba[0];
                if (error != EVRTrackedCameraError.None || !pixel)
                {
                    errMsg = "Get Frame Buffer error " + (int)error;
                    return false;
                }
            }
            errMsg = "";
            return true;
        }

        public void StartChaperone()
        {
            if (!IsAvailable(out LastErrMsg))
            {
                EnvironmentStatus = Status.Error;
                return;
            }
            if (viveSR == null || EnvironmentStatus != Status.Work ||
                ChaperoneStatus == Status.Start ||
                ChaperoneStatus == Status.Work) return;
            ChaperoneStatus = Status.Start;
            viveSR.StartFramework();
        }

        private void StartChaperoneCallback()
        {
            if (ViveSR_Experience_Chaperone.FrameworkStatus == Vive.Plugin.SR.FrameworkStatus.WORKING && ViveSR_Experience_Chaperone_DualCameraRig.DualCameraStatus == Vive.Plugin.SR.DualCameraStatus.WORKING)
            {
                ChaperoneStatus = Status.Work;
            }
            else
            {
                ChaperoneStatus = Status.Error;
                if (ViveSR_Experience_Chaperone.FrameworkStatus != Vive.Plugin.SR.FrameworkStatus.WORKING) LastErrMsg = ViveSR_Experience_Chaperone.LastError;
                else if (ViveSR_Experience_Chaperone_DualCameraRig.DualCameraStatus != Vive.Plugin.SR.DualCameraStatus.WORKING) LastErrMsg = ViveSR_Experience_Chaperone_DualCameraRig.LastError;
                else LastErrMsg = "Start Chaperone error";
            }
        }

        public void StopChaperone()
        {
            if (viveSR == null) return;
            viveSR.StopFramework();
            ChaperoneStatus = Status.Idle;
        }

        public void MinimizeWindow()
        {
            ShowWindow(EShowWindow.SW_MINIMIZE);
        }

        public void QuitApplication()
        {
            Application.Quit();
        }



        [StructLayout(LayoutKind.Sequential)]
        public struct MousePosition
        {
            public int x;
            public int y;
            public override string ToString()
            {
                return "[" + x + ", " + y + "]";
            }
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        public enum EShowWindow
        {
            SW_HIDE,
            SW_SHOWNORMAL,
            SW_SHOWMINIMIZED,
            SW_MAXIMIZE,
            SW_SHOWNOACTIVATE,
            SW_SHOW,
            SW_MINIMIZE,
            SW_SHOWMINNOACTIVE,
            SW_SHOWNA,
            SW_RESTORE,
            SW_SHOWDEFAULT,
            SW_FORCEMINIMIZE,
        }
        const int GWL_STYLE = -16;  // Sets a new window style.
        const long WS_VISIBLE = 0x10000000;

        [DllImport("user32.dll")] public static extern IntPtr FindWindow(System.String className, System.String windowName);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll")] private static extern bool GetCursorPos(out MousePosition lpMousePosition);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hwnd, int nIndex, long dwNewLong);
        public static void SetPosition(int x, int y, int resX = 0, int resY = 0)
        {
            SetWindowPos(FindWindow(null, Application.productName), 0, x, y, resX, resY, resX * resY == 0 ? 1 : 0);
        }
        public static bool ShowWindow(EShowWindow eShowWindow)
        {
            return ShowWindow(FindWindow(null, Application.productName), (int)eShowWindow);
        }
#endif
    }
}