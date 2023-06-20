using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(ViveSR_Experience))]
    public class Sample5_ChairSegmentation : MonoBehaviour
    {   
        [SerializeField] protected Text ScanText, StopText, SaveText, PlayText, HintText;

        ViveSR_Experience_NPCGenerator npcGenerator;

        [SerializeField] GameObject VRChairGroup;

        List<ViveSR_Experience_Chair> VR_Chairs = new List<ViveSR_Experience_Chair>();
        List<ViveSR_Experience_Chair> MR_Chairs = new List<ViveSR_Experience_Chair>();

        ViveSR_Experience_StaticMesh StaticMeshScript;

        List<SceneUnderstandingObjects.Element> SegResults;

        bool isPlaying, isTesting;

        ViveSR_Experience_ActionSequence ActionSequence;

        private void Awake()
        {
            StaticMeshScript = FindObjectOfType<ViveSR_Experience_StaticMesh>();

            if (VRChairGroup.activeSelf)
            {
                for (int i = 0; i < VRChairGroup.transform.childCount; i++)
                {
                    GameObject VR_Chair = VRChairGroup.transform.GetChild(i).gameObject;
                    ViveSR_Experience_Chair chair = VR_Chair.GetComponent<ViveSR_Experience_Chair>();
                    VR_Chairs.Add(chair);
                }
            }

            npcGenerator = GetComponent<ViveSR_Experience_NPCGenerator>();

            ViveSR_Experience.instance.CheckHandStatus(() =>
            {
                FindObjectOfType<ViveSR_PortalMgr>().TurnOnCamera();
                if (VRChairGroup.activeSelf)
                {
                    ViveSR_Experience_ControllerDelegate.touchpadDelegate += handleTouchpad_VRChair;
                    SetColor(Color.white, ScanText, SaveText, PlayText, StopText);
                    ScanText.text = SaveText.text = PlayText.text = StopText.text = "[Play]";
                }
                else ViveSR_Experience_ControllerDelegate.touchpadDelegate += handleTouchpad_MRChair;
                if (StaticMeshScript.GetSegmentationInfo(SceneUnderstandingObjectType.CHAIR).Count > 0) PlayText.color = Color.white;
            });
        }

        public void handleTouchpad_VRChair(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {
                case ButtonStage.PressDown:

                    npcGenerator.Play(VR_Chairs);

                    break;
            }
        }

        public void SetColor(Color color, params Text[] texts)
        {
            foreach(Text text in texts) text.color = color;
        }

        public void handleTouchpad_MRChair(ButtonStage buttonStage, Vector2 axis)
        {
            TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, false);

            switch (buttonStage)
            {
                case ButtonStage.PressDown:
                    switch (touchpadDirection)
                    {
                        case TouchpadDirection.Up: //scan
                            if (!ViveSR_RigidReconstruction.IsScanning && !ViveSR_RigidReconstruction.IsExportingMesh)
                            {
                                HintText.text = "";

                                StaticMeshScript.LoadMesh(false);
                                StaticMeshScript.ClearHintLocators();
                                npcGenerator.ClearScene();

                                StaticMeshScript.SetScanning(true);
                                StaticMeshScript.SetSegmentation(true);

                                SetColor(Color.gray, ScanText, StopText, PlayText);
                                SetColor(Color.white, SaveText, StopText);
                            }
                            break;

                        case TouchpadDirection.Right: //test                    
                            if (!isTesting && ViveSR_RigidReconstruction.IsScanning && !ViveSR_RigidReconstruction.IsExportingMesh)
                            {

                                StaticMeshScript.SetChairSegmentationConfig(true);

                                SetColor(Color.gray, ScanText, StopText, SaveText, PlayText);

                                isTesting = true;

                                ActionSequence = ViveSR_Experience_ActionSequence.CreateActionSequence(gameObject);

                                ActionSequence.AddAction(() => StaticMeshScript.TestSegmentationResult(UpdatePercentage_Testing, ActionSequence.ActionFinished));

                                //waiting for testSegmentation
                                ActionSequence.AddAction(() =>
                                {
                                    SegResults = StaticMeshScript.GetSegmentationInfo(SceneUnderstandingObjectType.CHAIR);
                                    if (SegResults.Count == 0)
                                    {
                                        HintText.text = "No Chair Identified";
                                        SetColor(Color.white, StopText, SaveText);
                                        ActionSequence.ActionFinished();
                                    }
                                    else
                                    {
                                        StaticMeshScript.SetSegmentation(false);
                                        LoadChair();
                                        StaticMeshScript.GenerateHintLocators(SegResults);
                                        StaticMeshScript.ExportModel(UpdatePercentage_SaveMesh, ActionSequence.ActionFinished);
                                    }
                                });

                                //waiting for export model
                                ActionSequence.AddAction(() =>
                                {
                                    if (SegResults.Count > 0)
                                    {                                     
                                        SetColor(Color.white, ScanText, PlayText);
                                        SetColor(Color.gray, StopText, SaveText);
                                    }
                                    isTesting = false;
                                    ActionSequence.ActionFinished();
                                    
                                }); 

                                ActionSequence.StartSequence();
                            }
                            break;

                        case TouchpadDirection.Down: //play                                   
                            SegResults = StaticMeshScript.GetSegmentationInfo(SceneUnderstandingObjectType.CHAIR);
                            if (!ViveSR_RigidReconstruction.IsScanning && !ViveSR_RigidReconstruction.IsExportingMesh && SegResults.Count > 0)
                            {
                                isPlaying = true;

                                SetColor(Color.gray, ScanText, StopText, SaveText, PlayText);

                                ActionSequence = ViveSR_Experience_ActionSequence.CreateActionSequence(gameObject);

                                ActionSequence.AddAction(() => StaticMeshScript.LoadMesh(true, () => HintText.text = "Loading...", ActionSequence.ActionFinished));
                                ActionSequence.AddAction(() =>
                                {
                                    HintText.text = "Mesh Loaded!\nInit Play...";

                                    LoadChair();
                                    ActionSequence.ActionFinished();
                                });
                                ActionSequence.AddAction(() =>
                                {
                                    HintText.text = "";

                                    if (MR_Chairs.Count > 0) npcGenerator.Play(MR_Chairs);

                                    SetColor(Color.white, ScanText, PlayText);
                                    isPlaying = false;
                                    ActionSequence.ActionFinished();
                                });

                                ActionSequence.StartSequence();
                            }
                            break;

                        case TouchpadDirection.Left: //stop
                            if (ViveSR_RigidReconstruction.IsScanning && !ViveSR_RigidReconstruction.IsExportingMesh && !isPlaying)
                            {
                                if (isTesting)
                                {
                                    ActionSequence.StopSequence();
                                    isTesting  = false;
                                }

                                StaticMeshScript.SetSegmentation(false);
                                StaticMeshScript.SetScanning(false);

                                SetColor(Color.white, ScanText);
                                SetColor(Color.gray, SaveText, StopText);

                                if (StaticMeshScript.GetSegmentationInfo(SceneUnderstandingObjectType.CHAIR).Count > 0) SetColor(Color.white, PlayText);
                            }

                            break;
                    }
                break;
            }
        } 
                                        
        void UpdatePercentage_Testing(int percentage)
        {
            HintText.text = "Testing..." + percentage.ToString() + "%";
        }

        void UpdatePercentage_SaveMesh(int percentage)
        {
            if(percentage < 100) HintText.text = MR_Chairs.Count + (MR_Chairs.Count == 1 ? " Chair Identified!" : " Chairs Identified!") + "\nSaving Mesh..." + percentage.ToString() + "%";
            else HintText.text = MR_Chairs.Count + (MR_Chairs.Count == 1 ? " Chair Identified!" : " Chairs Identified!") + "\nMesh Saved!";
        }

        void LoadChair()
        {
            foreach (ViveSR_Experience_Chair MR_Chair in MR_Chairs) Destroy(MR_Chair.gameObject);
            MR_Chairs.Clear();
            StaticMeshScript.ClearHintLocators();

            List<SceneUnderstandingObjects.Element> ChairElements = StaticMeshScript.GetSegmentationInfo(SceneUnderstandingObjectType.CHAIR);

            for (int i = 0; i < ChairElements.Count; i++)
            {
                GameObject go = new GameObject("MR_Chair" + i, typeof(ViveSR_Experience_Chair));
                ViveSR_Experience_Chair chair = go.GetComponent<ViveSR_Experience_Chair>();
                chair.CreateChair(new Vector3(ChairElements[i].position[0].x, ChairElements[i].position[0].y, ChairElements[i].position[0].z), new Vector3(ChairElements[i].forward.x, ChairElements[i].forward.y, ChairElements[i].forward.z));
                MR_Chairs.Add(chair);
            }
        }
    }
}