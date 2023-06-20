//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using UnityEngine;

namespace Vive.Plugin.SR.Experience.Chaperone
{
    [ExecuteInEditMode]
    public class ViveSR_Experience_Chaperone : ViveSR
    {
        void Start()
        {
#if UNITY_EDITOR
            UnityEditor.PlayerSettings.virtualRealitySupported = false;
#endif
            FrameworkStatus = FrameworkStatus.STOP;
        }

        void OnDestroy()
        {
            if (Application.isPlaying)
            {
                StopFramework();
            }
        }        

        protected override int ViveSR_InitialFramework()
        {
            int result = (int)Error.FAILED;
            if (UnityEngine.XR.XRSettings.enabled) return result; // do not use bulit-in vr support

            result = ViveSR_Framework.Initial();
            result = ViveSR_Framework.SetLogLevel((int)LogLevel._2);

            result = ViveSR_Framework.CreateModule((int)ModuleType.ENGINE_SEETHROUGH, ref ViveSR_Framework.MODULE_ID_SEETHROUGH);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule Error " + result); return result; }

            result = ViveSR_Framework.CreateModule((int)ModuleType.ENGINE_HUMAN_DETECTION, ref ViveSR_Framework.MODULE_ID_HUMAN_DETECTION);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule Error " + result); return result; }
            return result;
        }

        protected override int ViveSR_StartFramework()
        {
            int result = (int)Error.FAILED;

            ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_SEETHROUGH, (int)SeeThroughParam.VR_INIT, true);
            ViveSR_Framework.SetParameterInt(ViveSR_Framework.MODULE_ID_SEETHROUGH, (int)SeeThroughParam.VR_INIT_TYPE, 2);
            //ViveSR_Framework.SetParameterInt(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)HumanDetectionParam.MODEL_TYPE, 1); //1: cpu, 2: gpu

#if UNITY_EDITOR
            string ModelPath = Application.dataPath + "/ViveSR/Plugins";
#else
            string ModelPath = Application.dataPath + "/Plugins";
#endif

            HumanDetectionModuleInfo Info = new HumanDetectionModuleInfo();
            ViveSR_Framework.GetParameterStruct(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)HumanDetectionParam.MODULEINFO, ref Info);
            Info.Model_Path = ModelPath;
            Info.Model_PathLength = ModelPath.Length;
            Info.ProcessUnit = 1;
            ViveSR_Framework.SetParameterStruct(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)HumanDetectionParam.MODULEINFO, Info);


            result = ViveSR_Framework.StartModule(ViveSR_Framework.MODULE_ID_SEETHROUGH);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule MODULE_ID_SEETHROUGH Error " + result); return result; }

            result = ViveSR_Framework.StartModule(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION);
            if (result == 1) Debug.LogWarning("[ViveSR] Please put the model folder in the assigned path: " + ModelPath + "/model");
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] StartModule MODULE_ID_HUMAN_DETECTION Error " + result); return result; }

            result = ViveSR_Framework.ModuleLink(ViveSR_Framework.MODULE_ID_SEETHROUGH, ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)WorkLinkMethod.ACTIVE);
            if (result != (int)Error.WORK) { Debug.Log("[ViveSR] ModuleLink MODULE_ID_SEETHROUGH to MODULE_ID_HUMAN_DETECTION Error " + result); return result; }




            // Fixed module(depth module in this case) to constant N FPS(1 in this case).
            result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)Module_Param.ENABLE_FPSCONTROL, true);
            result = ViveSR_Framework.SetParameterInt(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)Module_Param.SET_FPS, 15);
            return result;
        }
    }
}