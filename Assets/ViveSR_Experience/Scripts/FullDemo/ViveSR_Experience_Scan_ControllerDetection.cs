using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Scan_ControllerDetection : MonoBehaviour
    {                                                                                     
        ViveSR_Experience_StaticMesh StaticMeshScript;

        private void Start()
        {
            StaticMeshScript = ViveSR_Experience_Demo.instance.StaticMeshScript;
        }

        void OnBecameInvisible()
        {   
            if (ViveSR_Experience_Demo.instance.SubButtonScripts[SubMenuButton._3DPreview_Scan].isOn && !ViveSR_RigidReconstruction.IsScanning)
            {
                ViveSR_Experience_HintMessage.instance.HintTextFadeOff(hintType.onHeadSet, 0f);

                StaticMeshScript.ClearHintLocators();
                StaticMeshScript.SetScanning(true);
                StaticMeshScript.SetSegmentation(true);
                ViveSR_Experience_Demo.instance.SubButtonScripts[SubMenuButton._3DPreview_Save].EnableButton(true);
                ((ViveSR_Experience_Tutorial_InputHandler_3DPreview)ViveSR_Experience_Demo.instance.Tutorial.InputHandlers[MenuButton._3DPreview]).SetSaveSubMessage();
            }
        }

        void OnBecameVisible()
        {
            if (!ViveSR_RigidReconstruction.IsScanning)
            {
                ViveSR_Experience_HintMessage.instance.SetHintMessage(hintType.onHeadSet, "Put the controller out of sight to start scanning.", false);
            }
        }

    }
}