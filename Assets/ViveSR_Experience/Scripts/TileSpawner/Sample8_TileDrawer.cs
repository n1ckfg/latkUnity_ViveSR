using UnityEngine;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(ViveSR_Experience))]
    public class Sample8_TileDrawer : MonoBehaviour
    {
        enum ActionMode
        {
            MeshControl,
            TileControl,
            ColliderDisplay,
            MaxNum
        }

        ActionMode actionMode = ActionMode.MeshControl;

        [SerializeField] ViveSR_Experience_DartGeneratorMgr dartGeneratorMgr;

        ViveSR_Experience_TileSpawner TileSpawnerScript;

        public Text ScanText, StopText, SaveText, LoadText, HintText, DartText, GripText;

        bool isTriggerDown;
        bool occlusionEnabled = false;
        
        ViveSR_Experience_SwitchMode SwitchModeScript;
        ViveSR_Experience_StaticMesh StaticMeshScript;
        
        int ActionModeNum = (int)ActionMode.MaxNum;

        private void Awake()
        {
            StaticMeshScript = FindObjectOfType<ViveSR_Experience_StaticMesh>();
            SwitchModeScript = StaticMeshScript.SwitchModeScript;
            TileSpawnerScript = FindObjectOfType<ViveSR_Experience_TileSpawner>();
        }

        private void Start()
        {
            if (StaticMeshScript.CheckModelExist())
                LoadText.color = Color.white;

            ViveSR_Experience.instance.CheckHandStatus(() =>
            {
                ViveSR_RigidReconstructionRenderer.LiveMeshDisplayMode = ReconstructionDisplayMode.ADAPTIVE_MESH;
                ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpad_MeshOperation;
            });
        }

        public void HandleGrip_SwitchMode(ButtonStage buttonStage, Vector2 axis)
        {
            if (!isTriggerDown)
            {
                switch (buttonStage)
                {
                    case ButtonStage.PressDown:
                        if (StaticMeshScript.texturedMesh != null && !ViveSR_RigidReconstruction.IsExportingMesh && !ViveSR_RigidReconstruction.IsScanning)
                        {
                            MoveToNextActionMode();
                        }
                        break;
                }
            }
        }

        void MoveToNextActionMode()
        {
            int mode_int = (int)actionMode;
            ActionMode mode = (ActionMode)((++mode_int) % ActionModeNum);
            SwitchActionMode(mode);
        }

        void SwitchActionMode(ActionMode mode)
        {
            actionMode = mode;

            if (mode == ActionMode.MeshControl) // 0
            {
                dartGeneratorMgr.gameObject.SetActive(true);
                ViveSR_Experience_ControllerDelegate.touchpadDelegate -= HandleTouchpad_ColliderOperation;
                ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpad_MeshOperation;
                SwitchModeScript.SwithMode(DualCameraDisplayMode.MIX);
                StaticMeshScript.SwitchShowCollider(ShowMode.None);
                SetStaticMeshUI();
            }
            else if (mode == ActionMode.TileControl) // 1
            {
                TileSpawnerScript.enabled = true;
                TileSpawnerScript.SetCldPool(StaticMeshScript.cldPool);

                ViveSR_Experience_ControllerDelegate.triggerDelegate -= HandleTrigger_SetDartGeneratorUI;
                dartGeneratorMgr.gameObject.SetActive(false);
                ViveSR_Experience_ControllerDelegate.touchpadDelegate -= HandleTouchpad_MeshOperation;
                ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpad_TileOperation;
                ViveSR_Experience_ControllerDelegate.triggerDelegate += HandleTrigger_DrawTiles;
                StaticMeshScript.SwitchShowCollider(ShowMode.None);
                SetDrawTileUI();
            }
            else if (mode == ActionMode.ColliderDisplay) // 2
            {
                TileSpawnerScript.enabled = false;
                ViveSR_Experience_ControllerDelegate.triggerDelegate += HandleTrigger_SetDartGeneratorUI;
                dartGeneratorMgr.gameObject.SetActive(true);
                ViveSR_Experience_ControllerDelegate.triggerDelegate -= HandleTrigger_DrawTiles;
                ViveSR_Experience_ControllerDelegate.touchpadDelegate -= HandleTouchpad_TileOperation;
                ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpad_ColliderOperation;
                StaticMeshScript.SwitchShowCollider(ShowMode.None);
                SetShowColliderUI();

                EnableDepthOcclusion(false);
            }
        }
       
        /*------------------------------mesh-------------------------------------*/
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
                            if (!ViveSR_RigidReconstruction.IsScanning)
                            {
                                dartGeneratorMgr.DestroyObjs();
                                TileSpawnerScript.ClearTiles();
                                if (StaticMeshScript.texturedMesh != null)
                                {
                                    StaticMeshScript.LoadMesh(false);
                                    LoadText.color = Color.white;
                                }

                                DartText.text = "";
                                HintText.text = "";
                                GripText.text = "";
                                ViveSR_Experience_ControllerDelegate.gripDelegate -= HandleGrip_SwitchMode;

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
                                if(StaticMeshScript.CheckModelLoaded()) ViveSR_Experience_ControllerDelegate.gripDelegate += HandleGrip_SwitchMode;
                                ViveSR_RigidReconstruction.StopScanning(); //Stop scanning
                                ViveSR_DualCameraImageCapture.EnableDepthProcess(false); //Turn off depth engine
                                
                                DartText.text = StaticMeshScript.CheckModelLoaded() ? "Throw Item" : "";  
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
                                ViveSR_Experience_ControllerDelegate.touchpadDelegate -= HandleTouchpad_MeshOperation;

                                StaticMeshScript.ExportModel(UpdatePercentage, () =>
                                {
                                    DartText.text = "Throw Item";
                                    HintText.text = "Mesh Saved!";
                                    ScanText.color = Color.white;
                                    LoadText.color = Color.white;
                                    ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpad_MeshOperation; 
                                });
                            }
                        }
                        else if (touchpadDirection == TouchpadDirection.Down)//[Load]
                        {
                            if (!StaticMeshScript.CheckModelLoaded() && !ViveSR_RigidReconstruction.IsScanning && !StaticMeshScript.ModelIsLoading)
                            {
                                ViveSR_Experience_ActionSequence ActionSequence = ViveSR_Experience_ActionSequence.CreateActionSequence(gameObject);
                                ActionSequence.AddAction(() =>
                                {
                                    ViveSR_Experience_ControllerDelegate.gripDelegate -= HandleGrip_SwitchMode;

                                    StaticMeshScript.LoadMesh(true,
                                    () =>
                                    {
                                        LoadText.color = Color.grey;
                                        HintText.text = "Loading...";
                                        DartText.text = "";
                                    },
                                    () =>
                                    {
                                        ActionSequence.ActionFinished();
                                    });

                                });

                                ActionSequence.AddAction(()=>
                                {
                                    StaticMeshScript.WaitForCldPool(
                                        () =>
                                        {
                                            HintText.text = "Mesh Loaded!";
                                            ScanText.color = Color.white;
                                            DartText.text = "Lay Tiles";
                                            GripText.text = "Switch Control";
                                            ViveSR_Experience_ControllerDelegate.gripDelegate += HandleGrip_SwitchMode;

                                            SwitchActionMode(ActionMode.TileControl);

                                            ActionSequence.ActionFinished();
                                        }
                                    );
                                });

                                ActionSequence.StartSequence();
                            }
                        }
                        break;
                }
            }
        }

        void UpdatePercentage(int percentage)
        {
            HintText.text = "Loading..." + percentage + "%";
        }

        private void HandleTouchpad_TileOperation(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {
                case ButtonStage.PressDown:

                    TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, false);

                    if (touchpadDirection == TouchpadDirection.Up)
                    {
                        EnableDepthOcclusion(!occlusionEnabled);
                        SetDrawTileUI();
                    }
                    if (touchpadDirection == TouchpadDirection.Left)
                    {
                        if(!isTriggerDown) TileSpawnerScript.RotateFloatingTile(10.0f);
                    }
                    else if (touchpadDirection == TouchpadDirection.Right)
                    {
                        if (!isTriggerDown) TileSpawnerScript.RotateFloatingTile(-10.0f);
                    }
                    else if (touchpadDirection == TouchpadDirection.Down)
                    {
                        dartGeneratorMgr.DestroyObjs();
                        TileSpawnerScript.ClearTiles();
                    }
                    break;
            }
        }

        private void EnableDepthOcclusion(bool enable)
        {
            ViveSR_DualCameraImageCapture.ChangeDepthCase(enable ? DepthCase.CLOSE_RANGE : DepthCase.DEFAULT);
            ViveSR_DualCameraImageCapture.EnableDepthProcess(enable);
            ViveSR_DualCameraImageCapture.EnableDepthRefinement(enable);
            ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = enable;
            ViveSR_DualCameraImageRenderer.DepthImageOcclusion = enable;
            ViveSR_DualCameraImageRenderer.OcclusionNearDistance = 0.05f;
            occlusionEnabled = enable;
        }

        private void HandleTouchpad_ColliderOperation(ButtonStage buttonStage, Vector2 axis)
        {
            if (!isTriggerDown)
            {
                switch (buttonStage)
                {
                    case ButtonStage.PressDown:

                        TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, false);

                        if (touchpadDirection == TouchpadDirection.Up)
                        {
                            if (StaticMeshScript.MeshShowMode != ShowMode.None)
                            {
                                StaticMeshScript.SwitchShowCollider(ShowMode.None);
                            }
                            else
                            {
                                StaticMeshScript.SwitchShowCollider(ShowMode.All);
                            }
                        }
                        else if (touchpadDirection == TouchpadDirection.Left)
                        {
                            MoveToPreviousShowMode();
                        }
                        else if (touchpadDirection == TouchpadDirection.Right)
                        {
                            MoveToNextShowMode();
                        }
                        SetShowColliderUI();
                        break;
                }
            }
        }

        void MoveToNextShowMode()
        {
            int cur_idx = (int)StaticMeshScript.MeshShowMode;
            int num = (int)ShowMode.NumOfModes;

            ShowMode mode = StaticMeshScript.MeshShowMode;
            do
            {
                mode = (ShowMode)(++cur_idx % num);
            } while (mode == ShowMode.None); //skip none mode


            StaticMeshScript.SwitchShowCollider(mode);
        }

        void MoveToPreviousShowMode()
        {
            int cur_idx = (int)StaticMeshScript.MeshShowMode;
            int num = (int)ShowMode.NumOfModes;
            ShowMode mode;
            do
            {
                if (--cur_idx < 0) cur_idx += num;
                mode = (ShowMode)(cur_idx);
            } while (mode == ShowMode.None); //skip none mode

            StaticMeshScript.SwitchShowCollider(mode);
        }              

        void HandleTrigger_DrawTiles(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {
                case ButtonStage.PressDown:
                    StopText.text = "";
                    SaveText.text = "";
                    GripText.text = "";
                    TileSpawnerScript.TriggerPressDown();
                    isTriggerDown = true;
                    break;

                case ButtonStage.PressUp:
                    StopText.text = "<";
                    SaveText.text = ">"; 
                    GripText.text = "Switch Control";
                    isTriggerDown = false;
                    TileSpawnerScript.TriggerPressUp();
                    break;
            }
        }                   
        
        /*--------------------------------Dart----------------------------------*/

        void HandleTrigger_SetDartGeneratorUI(ButtonStage buttonStage, Vector2 axis)
        {
            if (!ViveSR_RigidReconstruction.IsExportingMesh && !ViveSR_RigidReconstruction.IsScanning && !StaticMeshScript.ModelIsLoading)
            {
                switch (buttonStage)
                {
                    case ButtonStage.PressDown:
                        isTriggerDown = true;
                        ViveSR_Experience_ControllerDelegate.touchpadDelegate += Touchpad_PressingTrigger;
                        SetThrowUI();
                        break;

                    case ButtonStage.PressUp:
                        isTriggerDown = false;
                        ViveSR_Experience_ControllerDelegate.touchpadDelegate -= Touchpad_PressingTrigger;
                        if (actionMode == ActionMode.MeshControl)
                            SetStaticMeshUI();
                        else if (actionMode == ActionMode.ColliderDisplay)
                            SetShowColliderUI();
                        break;
                }
            }
        }

        void Touchpad_PressingTrigger(ButtonStage buttonStage, Vector2 axis)
        {
            TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, false);

            switch (buttonStage)
            {
                case ButtonStage.PressDown:
                    switch (touchpadDirection)
                    {
                        case TouchpadDirection.Up: SetDartTypeUI(); break;
                        case TouchpadDirection.Down: dartGeneratorMgr.DestroyObjs(); break;
                    }
                       
                break;
            }
        }
        /*----------------------------------------------------------------------*/


        /*---------------------------------UI-----------------------------------*/
        void SetThrowUI()
        {
            StopText.color = ScanText.color = SaveText.color = LoadText.color = Color.white;

            SetDartTypeUI();

            StopText.text = "<";
            SaveText.text = ">";
            LoadText.text = "[Clear]";
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
            LoadText.color = Color.grey;
            ScanText.color = Color.white;

            HintText.text = "Static Mesh";
            StopText.text = "[Stop]";
            SaveText.text = "[Save]";
            LoadText.text = "[Load]";
            ScanText.text = "[Scan]";

            DartText.text = StaticMeshScript.CheckModelLoaded()? "Throw Item" : "";
            GripText.text = "Switch Control";
        }

        void SetDrawTileUI()
        {
            StopText.color = Color.white;
            SaveText.color = Color.white;
            LoadText.color = Color.white;
            ScanText.color = Color.white;

            StopText.text = "<";
            SaveText.text = ">";
            LoadText.text = "[Clear]";
            ScanText.text = occlusionEnabled? "Occlude OFF" : "Occlude ON";

            HintText.text = "Tile Control";
            DartText.text = "Lay Tiles";
        }

        void SetShowColliderUI()
        {
            StopText.color = Color.white;
            SaveText.color = Color.white;
            LoadText.color = Color.white;
            ScanText.color = Color.white;

            StopText.text = "[Prev]";
            SaveText.text = "[Next]";
            LoadText.text = "";

            DartText.text = "Throw Item";
            GripText.text = "Switch Control";

            ShowMode CurrentCldDisplayMode = StaticMeshScript.MeshShowMode;

            if (CurrentCldDisplayMode != ShowMode.None)
                ScanText.text = "[Hide All]";
            else
                ScanText.text = "[Show All]";

            if (StaticMeshScript.texturedMesh != null)
            {
                string cld_display = "Collider Display\n";
                string all = "All";
                string hor = "Horizontal";
                string ver = "Vertical";
                string near = " - Nearest";
                string far = " - Furthest";
                string large= " - Largest";

                if (CurrentCldDisplayMode == ShowMode.All)
                    HintText.text = cld_display + all;
                else if (CurrentCldDisplayMode == ShowMode.Horizon)
                    HintText.text = cld_display + hor;
                else if (CurrentCldDisplayMode == ShowMode.LargestHorizon)
                    HintText.text = cld_display + hor + large;
                else if (CurrentCldDisplayMode == ShowMode.AllVertical)
                    HintText.text = cld_display + ver;
                else if (CurrentCldDisplayMode == ShowMode.LargestVertical)
                    HintText.text = cld_display + ver + large;
                else if (CurrentCldDisplayMode == ShowMode.NearestHorizon)
                    HintText.text = cld_display + hor + near;
                else if (CurrentCldDisplayMode == ShowMode.FurthestHorizon)
                    HintText.text = cld_display + hor + far;
                else if (CurrentCldDisplayMode == ShowMode.NearestVertical)
                    HintText.text = cld_display + ver + near;
                else if (CurrentCldDisplayMode == ShowMode.FurthestVertical)
                    HintText.text = cld_display + ver + far;
                else
                    HintText.text = cld_display;

            }
        }
    /*----------------------------------------------------------------------*/

}
}