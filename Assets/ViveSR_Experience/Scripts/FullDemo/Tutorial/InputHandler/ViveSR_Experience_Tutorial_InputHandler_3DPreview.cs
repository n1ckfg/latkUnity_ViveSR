using UnityEngine;
using System.Collections;
namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Tutorial_InputHandler_3DPreview : ViveSR_Experience_Tutorial_IInputHandler
    {
        protected override void StartToDo()
        {
            Button = ViveSR_Experience_Demo.instance.ButtonScripts[MenuButton._3DPreview];
        }

        protected override void MidPressedDown()
        {      
            if (SubMenu.SelectedButton == (int)_3DPreview_SubBtn.Scan)
            {   
                base.MidPressedDown();
            }
            else if (SubMenu.SelectedButton == (int)_3DPreview_SubBtn.Save)
            {
                if (!ViveSR_Experience_Demo.instance.SubButtonScripts[SubMenuButton._3DPreview_Save].disabled)
                {
                    tutorial.ToggleTutorial(false); 
                    StartCoroutine(WaitForMeshSave());
                }
            }  
        }

        IEnumerator WaitForMeshSave()
        {
            while (ViveSR_SceneUnderstanding.IsExportingSceneUnderstandingInfo)
            {
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();

            while (ViveSR_RigidReconstruction.IsExportingMesh)
            {
                yield return new WaitForEndOfFrame();
            }

            tutorial.SetTouchpadSprite(true, ControllerInputIndex.left, ControllerInputIndex.right);

            tutorial.ToggleTutorial(true);
            if (ViveSR_Experience_Demo.instance.ButtonScripts[MenuButton._3DPreview].SubMenu.SelectedButton == (int)_3DPreview_SubBtn.Save) SetSubBtnMessage("Disabled");

        }

        public void SetSaveSubMessage()
        {
            SetSubBtnMessage();
        }                              
    }
}