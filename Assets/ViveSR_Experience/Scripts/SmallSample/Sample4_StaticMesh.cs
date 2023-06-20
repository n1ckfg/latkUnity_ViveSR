using UnityEngine;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(ViveSR_Experience))]
    public class Sample4_StaticMesh : MonoBehaviour
    {
        enum MeshDisplayMode
        {
            None = 0,
            Collider,
            VRMode,
            MaxNum
        }

        [SerializeField] ViveSR_Experience_DartGeneratorMgr dartGeneratorMgr;

        public Text ScanText, StopText, SaveText, LoadText, HintText, DartText, GripText, MidText;

        bool isTriggerDown;

        MeshDisplayMode meshDisplayMode;

        ViveSR_Experience_StaticMesh StaticMeshScript;


        private void Awake()
        {
            StaticMeshScript = GetComponent<ViveSR_Experience_StaticMesh>();
        }

        private void Start()
        {
            if (StaticMeshScript.CheckModelExist()) LoadText.color = Color.white;

            ViveSR_Experience.instance.CheckHandStatus(() =>
            {
                ViveSR_RigidReconstructionRenderer.LiveMeshDisplayMode = ReconstructionDisplayMode.ADAPTIVE_MESH;
                ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpad_MeshOperation;
                ViveSR_Experience_ControllerDelegate.gripDelegate += HandleGrip_SwitchMeshDisplay; //Grip
                ViveSR_Experience_ControllerDelegate.triggerDelegate += HandleTrigger_SetDartGeneratorUI; //Toy
            });
        }

        public void HandleGrip_SwitchMeshDisplay(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {
                case ButtonStage.PressDown:
                    if (StaticMeshScript.CheckModelExist() && !ViveSR_RigidReconstruction.IsExportingMesh && !ViveSR_RigidReconstruction.IsScanning)
                        SwitchMeshDisplayType((MeshDisplayMode)(((int)meshDisplayMode + 1) % (int)MeshDisplayMode.MaxNum));
                    break;
            }
        }

        void SwitchMeshDisplayType(MeshDisplayMode meshDisplayMode)
        {
            this.meshDisplayMode = meshDisplayMode;

            switch (meshDisplayMode)
            {
                case MeshDisplayMode.None:  //Hidden
                    StaticMeshScript.SwitchModeScript.SwithMode(DualCameraDisplayMode.MIX);
                    StaticMeshScript.RenderMesh(false);
                    if (StaticMeshScript.CheckModelLoaded()) GripText.text = "View Colliders";
                    HintText.text = "See-Through";
                    break;
                case MeshDisplayMode.Collider:  
                    StaticMeshScript.SwitchShowCollider(ShowMode.All);
                    if (StaticMeshScript.CheckModelLoaded()) GripText.text = "View Texture";
                    HintText.text = "View Colliders";
                    break;
                case MeshDisplayMode.VRMode:
                    StaticMeshScript.SwitchShowCollider(ShowMode.None);
                    StaticMeshScript.SwitchModeScript.SwithMode(DualCameraDisplayMode.VIRTUAL);
                    StaticMeshScript.RenderMesh(true);
                    if (StaticMeshScript.CheckModelLoaded()) GripText.text = "See-Through";
                    HintText.text = "View Texture";
                    break;
            }
            
        }
   
        /*------------------------------mesh-------------------------------------*/

        void UpdatePercentage(int percentage)
        {
            HintText.text = "Loading..." + percentage + "%";
        }

        private void HandleTouchpad_MeshOperation(ButtonStage buttonStage, Vector2 axis)
        {
            if (!isTriggerDown)
            {
                switch (buttonStage)
                {
                    case ButtonStage.PressDown:

                        TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, false);

                        if (touchpadDirection == TouchpadDirection.Up)//[Scan]
                        {
                            if (!ViveSR_RigidReconstruction.IsScanning && !StaticMeshScript.ModelIsLoading)
                            {
                                SwitchMeshDisplayType(0);
                                dartGeneratorMgr.DestroyObjs();

                                if (StaticMeshScript.CheckModelLoaded())
                                {
                                    StaticMeshScript.LoadMesh(false);
                                    LoadText.color = Color.white;
                                }

                                DartText.text = "";
                                HintText.text = "";

                                StaticMeshScript.SetScanning(true);

                                LoadText.color = Color.gray;
                                ScanText.color = Color.gray;
                                SaveText.color = Color.white;
                                StopText.color = Color.white;
                            }
                        }
                        else if(touchpadDirection == TouchpadDirection.Left)//[Stop]
                        {
                            if (ViveSR_RigidReconstruction.IsScanning)
                            {
                                StaticMeshScript.SetScanning(false);

                                DartText.text = "Throw Item";

                                if (StaticMeshScript.CheckModelExist() && !StaticMeshScript.CheckModelLoaded()) LoadText.color = Color.white;
                                if (StaticMeshScript.CheckModelLoaded()) StaticMeshScript.LoadMesh(true);

                                ScanText.color = Color.white;
                                StopText.color = Color.grey;
                                SaveText.color = Color.grey;
                            }
                        }
                        else  if (touchpadDirection == TouchpadDirection.Right)// [Save]
                        {
                            if (ViveSR_RigidReconstruction.IsScanning)
                            {
                                LoadText.color = Color.grey;
                                ScanText.color = Color.grey;
                                StopText.color = Color.grey;
                                SaveText.color = Color.grey;

                                StaticMeshScript.ExportModel(UpdatePercentage, ()=> 
                                {
                                    DartText.text = "Throw Item";
                                    HintText.text = "Mesh Saved!";
                                    ScanText.color = Color.white;
                                    LoadText.color = Color.white;
                                });


                            }
                        }
                        else if (touchpadDirection == TouchpadDirection.Down)//[Load]
                        {
                            if (!StaticMeshScript.CheckModelLoaded() && !ViveSR_RigidReconstruction.IsScanning)
                            {               
                                StaticMeshScript.LoadMesh(true, 
                                ()=>
                                {
                                    LoadText.color = Color.grey;
                                    HintText.text = "Loading...";
                                },
                                ()=>
                                {
                                    HintText.text = "Mesh Loaded!";
                                    ScanText.color = Color.white;
                                    GripText.text = "View Collider";
                                });             
                            }
                        }
                        break;
                }
            }
        }

        /*--------------------------------Dart----------------------------------*/
        void HandleTrigger_SetDartGeneratorUI(ButtonStage buttonStage, Vector2 axis)
        {        
            if (!ViveSR_RigidReconstruction.IsExportingMesh && !ViveSR_RigidReconstruction.IsScanning)
            {
                switch (buttonStage)
                {
                    case ButtonStage.PressDown:
                        ViveSR_Experience_ControllerDelegate.touchpadDelegate += Touchpad_DartGeneratorTypeUI;
                        ViveSR_Experience_ControllerDelegate.gripDelegate -= HandleGrip_SwitchMeshDisplay;

                        SetThrowUI();
                        isTriggerDown = true;
                        break;
                    case ButtonStage.PressUp:
                        isTriggerDown = false;
                        ViveSR_Experience_ControllerDelegate.touchpadDelegate -= Touchpad_DartGeneratorTypeUI;
                        ViveSR_Experience_ControllerDelegate.gripDelegate += HandleGrip_SwitchMeshDisplay; //Grip
                        SetStaticMeshUI();
                        break;
                }
            }   
        } 

        void Touchpad_DartGeneratorTypeUI(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {
                case ButtonStage.PressDown:

                    TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, false);

                    switch (touchpadDirection)
                    {
                        case TouchpadDirection.Up: SetDartTypeUI(); break;
                        case TouchpadDirection.Down: dartGeneratorMgr.DestroyObjs(); break;
                    }

                    break;
            }
        }               

        /*---------------------------------UI-----------------------------------*/
        void SetThrowUI()
        {
            StopText.color = ScanText.color = SaveText.color = LoadText.color = Color.white;

            SetDartTypeUI();

            StopText.text = "<";
            SaveText.text = ">";
            LoadText.text = "[Clear]";
            MidText.text = "";
            DartText.text = "Throw Item";
            GripText.text = "";
        }

        void SetDartTypeUI()
        {
            if (HintText.text == "Raycast")
            {
                HintText.text = HintText.text = "Throw";
                ScanText.text = ScanText.text = "[Raycast]";
            }                
            else if(HintText.text == "Throw")
            {
                HintText.text = HintText.text = "Raycast";
                ScanText.text = ScanText.text = "[Throw]";
            }
            else
            {
                if (dartGeneratorMgr.dartPlacementMode == DartPlacementMode.Throwable)
                {
                    HintText.text = HintText.text = "Throw";
                    ScanText.text = ScanText.text = "[Raycast]";
                }
                else if (dartGeneratorMgr.dartPlacementMode == DartPlacementMode.Raycast)
                {
                    HintText.text = HintText.text = "Raycast";
                    ScanText.text = ScanText.text = "[Throw]";
                }
            }
        }

        void SetStaticMeshUI()
        {
            StopText.color = Color.grey;
            SaveText.color = Color.grey;
            LoadText.color = StaticMeshScript.CheckModelLoaded() && !StaticMeshScript.CheckChairExist() ? Color.grey : Color.white;
            ScanText.color = Color.white;

            HintText.text = "Static Mesh";
            StopText.text = "[Stop]";
            SaveText.text = "[Save]";
            LoadText.text = "[Load]";
            ScanText.text = "[Scan]";
            MidText.text = "";
            DartText.text = "Throw Item";
            if (StaticMeshScript.CheckModelLoaded())
            {
                if(meshDisplayMode == MeshDisplayMode.None) GripText.text = "View Collider";
                else if (meshDisplayMode == MeshDisplayMode.Collider) GripText.text = "View Texture";
                else if (meshDisplayMode == MeshDisplayMode.VRMode) GripText.text = "See-thru";
            }
        }
        /*----------------------------------------------------------------------*/  

    }
}