using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_SubBtn_3DPreview_Scan : ViveSR_Experience_ISubBtn
    {
        [SerializeField] _3DPreview_SubBtn SubBtnType; 

        [SerializeField] GameObject ControllerVisibilityDetector;

        ViveSR_Experience_StaticMesh StaticMeshScript;

        bool isTesting;
        int chairNum = 0;

        ViveSR_Experience_ActionSequence actionSequence;

        protected override void AwakeToDo()
        {
            ThisButtonTypeNum = (int)SubBtnType;
            ViveSR_RigidReconstructionRenderer.LiveMeshDisplayMode = ReconstructionDisplayMode.ADAPTIVE_MESH;
        }

        protected override void StartToDo()
        {
            StaticMeshScript = ViveSR_Experience_Demo.instance.StaticMeshScript;
        }

        public override void ExecuteToDo()
        {
#if UNITY_EDITOR
            ViveSR_Experience_Demo.instance.StaticMeshScript.SetScanning(isOn);
            ViveSR_Experience_Demo.instance.StaticMeshScript.SetSegmentation(isOn);
            ViveSR_Experience_Demo.instance.SubButtonScripts[SubMenuButton._3DPreview_Save].EnableButton(isOn);
            if (isOn) StaticMeshScript.ClearHintLocators();
#else
            ControllerVisibilityDetector.SetActive(isOn);
            if (!isOn)
            {
                ViveSR_Experience_HintMessage.instance.HintTextFadeOff(hintType.onHeadSet, 0f);
                if (ViveSR_RigidReconstruction.IsScanning)
                {
                    ViveSR_Experience_Demo.instance.StaticMeshScript.SetScanning(false);
                    ViveSR_Experience_Demo.instance.StaticMeshScript.SetSegmentation(false);
                    ViveSR_Experience_Demo.instance.SubButtonScripts[SubMenuButton._3DPreview_Save].EnableButton(false);
                }
            }  
#endif
        }

        private void OnEnable()
        {
            ViveSR_Experience.instance.CheckHandStatus(() =>
            {
                ViveSR_Experience_ControllerDelegate.triggerDelegate += HandleTrigger_TestingSegmentationItemUI;
            });
        }
        void RenderMenuSubButtons(MenuButton menu, bool isOn)
        {
            foreach (ViveSR_Experience_ISubBtn sub in ViveSR_Experience_Demo.instance.ButtonScripts[menu].SubMenu.subBtnScripts)
            {
                sub.renderer.enabled = isOn;
            }
        }
        void HandleTrigger_TestingSegmentationItemUI(ButtonStage buttonStage, Vector2 axis)
        {
            if (ViveSR_RigidReconstruction.IsScanning && !ViveSR_RigidReconstruction.IsExportingMesh)
            {
                switch (buttonStage)
                {
                    case ButtonStage.PressDown:

                        ViveSR_Experience_Demo.instance.Rotator.RenderButtons(false);
                        RenderMenuSubButtons(MenuButton._3DPreview, false);
                        

                        actionSequence = ViveSR_Experience_ActionSequence.CreateActionSequence(gameObject);

                        ViveSR_Experience_Demo.instance.StaticMeshScript.SetChairSegmentationConfig(true);

                        isTesting = true;

                        actionSequence.AddAction(() => StaticMeshScript.TestSegmentationResult(UpdatePercentage_Segmentation, actionSequence.ActionFinished));
                        actionSequence.AddAction(() =>
                        {
                            List<SceneUnderstandingObjects.Element> SegResults = ViveSR_Experience_Demo.instance.StaticMeshScript.GetSegmentationInfo(SceneUnderstandingObjectType.CHAIR);
                            chairNum = SegResults.Count;
                            StaticMeshScript.SetSegmentation(false);
                            StaticMeshScript.GenerateHintLocators(SegResults);
                            actionSequence.ActionFinished();
                        });
                        actionSequence.AddAction(() =>
                        {
                            ViveSR_Experience_HintMessage.instance.SetHintMessage(hintType.onController, "Finding " + chairNum.ToString()+ " Chair(s)", false);
                            actionSequence.ActionFinished();
                        });

                       actionSequence.StartSequence();


                        break;

                    case ButtonStage.PressUp:

                        if (isTesting)
                        {
                            ViveSR_Experience_Demo.instance.Rotator.RenderButtons(true);
                            RenderMenuSubButtons(MenuButton._3DPreview, true);
                            actionSequence.StopSequence();
                            ViveSR_Experience_HintMessage.instance.SetHintMessage(hintType.onController, "", false);
                            StaticMeshScript.ClearHintLocators();
                            isTesting = false;
                        }
                        break;
                }
            }
        }
        void UpdatePercentage_Segmentation(int percentage)
        {
            ViveSR_Experience_HintMessage.instance.SetHintMessage(hintType.onController, "Finding " + "Chair(" + percentage.ToString() + "%)", false);
        }
    }
}