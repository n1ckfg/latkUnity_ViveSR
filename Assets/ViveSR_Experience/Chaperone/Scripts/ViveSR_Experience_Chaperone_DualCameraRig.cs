//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using UnityEngine;

namespace Vive.Plugin.SR.Experience.Chaperone
{
    public class ViveSR_Experience_Chaperone_DualCameraRig : ViveSR_Module
    {
        public ViveSR_Experience_Chaperone_ImageRenderer DualCameraImageRenderer;
        public ViveSR_TrackedCamera TrackedCameraLeft;

        public static DualCameraStatus DualCameraStatus { get; private set; }
        public static string LastError { get; private set; }

        private ViveSR_Experience_Chaperone_DualCameraRig() { }
        private static ViveSR_Experience_Chaperone_DualCameraRig Mgr = null;
        public static ViveSR_Experience_Chaperone_DualCameraRig Instance
        {
            get
            {
                if (Mgr == null)
                {
                    Mgr = FindObjectOfType<ViveSR_Experience_Chaperone_DualCameraRig>();
                }
                if (Mgr == null)
                {
                    Debug.LogError("Chaperone_ViveSRDualCameraRig does not be attached on GameObject");
                }
                return Mgr;
            }
        }

        public override bool Initial()
        {
            DualCameraStatus = DualCameraStatus.IDLE;
            if (ViveSR.FrameworkStatus == FrameworkStatus.WORKING)
            {
                int result = ViveSR_DualCameraImageCapture.Initial();
                if (result != (int)Error.WORK)
                {
                    DualCameraStatus = DualCameraStatus.ERROR;
                    LastError = "[ViveSR] Initial Camera error " + result;
                    Debug.LogError(LastError);
                    return false;
                }
                result = ViveSR_Experience_Chaperone_ImageCapture.Initial();
                DualCameraStatus = DualCameraStatus.WORKING;
				return true;
            }
			return false;
        }

        public override bool Release()
        {
            DualCameraStatus = DualCameraStatus.IDLE;
			return true;
        }
    }
}