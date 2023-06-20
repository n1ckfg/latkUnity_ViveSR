using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Demo : MonoBehaviour
    {
        private static ViveSR_Experience_Demo _instance;
        public static ViveSR_Experience_Demo instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience_Demo>();
                }
                return _instance;
            }
        }

        public ViveSR_Experience_Rotator Rotator { get; private set; } 
        public ViveSR_Experience_Tutorial Tutorial { get; private set; }
        public ViveSR_Experience_StaticMesh StaticMeshScript { get; private set; }
        public ViveSR_Experience_Calibration CalibrationScript { get; private set; }
        public ViveSR_Experience_Portal PortalScript { get; private set; }
        public Dictionary<MenuButton, ViveSR_Experience_IButton> ButtonScripts = new Dictionary<MenuButton, ViveSR_Experience_IButton>();
        public Dictionary<SubMenuButton, ViveSR_Experience_ISubBtn> SubButtonScripts = new Dictionary<SubMenuButton, ViveSR_Experience_ISubBtn>();
        Dictionary<MenuButton, Renderer> ButtonRenderers = new Dictionary<MenuButton, Renderer>();

        [SerializeField] List<ViveSR_Experience_DartGeneratorMgr> _DartGeneratorMgrs;
        public Dictionary<DartGeneratorIndex, ViveSR_Experience_DartGeneratorMgr> DartGeneratorMgrs = new Dictionary<DartGeneratorIndex, ViveSR_Experience_DartGeneratorMgr>();

        PlayerHandUILaserPointer LaserPointer; 

        public Color OriginalEmissionColor;
        public Color BrightColor;
        public Color DisableColor;
        public Color AttentionColor;

        public GameObject bg, realWorldFloor;
              
        private void Awake()
        {
            ViveSR_Experience.instance.CheckHandStatus(()=>
            {
                PlayerHandUILaserPointer.CreateLaserPointer();
                PlayerHandUILaserPointer.EnableLaserPointer(false);       
            });

            Rotator = FindObjectOfType<ViveSR_Experience_Rotator>();
            Tutorial = FindObjectOfType<ViveSR_Experience_Tutorial>();
            StaticMeshScript = FindObjectOfType<ViveSR_Experience_StaticMesh>();
            CalibrationScript = FindObjectOfType<ViveSR_Experience_Calibration>();
            PortalScript = FindObjectOfType<ViveSR_Experience_Portal>();

            ButtonScripts[MenuButton.DepthControl] = FindObjectOfType<ViveSR_Experience_Button_DepthControl>();
            ButtonScripts[MenuButton._3DPreview] = FindObjectOfType<ViveSR_Experience_Button_3DPreview>();
            ButtonScripts[MenuButton.EnableMesh] = FindObjectOfType<ViveSR_Experience_Button_EnableMesh>();
            ButtonScripts[MenuButton.Segmentation] = FindObjectOfType<ViveSR_Experience_Button_Segmentation>();
            ButtonScripts[MenuButton.Portal] = FindObjectOfType<ViveSR_Experience_Button_Portal>(); 
            ButtonScripts[MenuButton.Effects] = FindObjectOfType<ViveSR_Experience_Button_Effects>();
            ButtonScripts[MenuButton.CameraControl] = FindObjectOfType<ViveSR_Experience_Button_CameraControl>();
            ButtonScripts[MenuButton.Calibration] = FindObjectOfType<ViveSR_Experience_Button_Calibration>();

            SubButtonScripts[SubMenuButton._3DPreview_Save] = FindObjectOfType<ViveSR_Experience_SubBtn_3DPreview_Save>();
            SubButtonScripts[SubMenuButton._3DPreview_Scan] = FindObjectOfType<ViveSR_Experience_SubBtn_3DPreview_Scan>();
            SubButtonScripts[SubMenuButton.Calibration_Alignment] = FindObjectOfType<ViveSR_Experience_SubBtn_Calibration_Alignment>();
            SubButtonScripts[SubMenuButton.Calibration_Focus] = FindObjectOfType<ViveSR_Experience_SubBtn_Calibration_Focus>();
            SubButtonScripts[SubMenuButton.Calibration_Reset] = FindObjectOfType<ViveSR_Experience_SubBtn_Calibration_Reset>();
            SubButtonScripts[SubMenuButton.EnableMesh_StaticMR] = FindObjectOfType<ViveSR_Experience_SubBtn_EnableMesh_StaticMR>();
            SubButtonScripts[SubMenuButton.EnableMesh_StaticVR] = FindObjectOfType<ViveSR_Experience_SubBtn_EnableMesh_StaticVR>();
            SubButtonScripts[SubMenuButton.EnableMesh_Dynamic] = FindObjectOfType<ViveSR_Experience_SubBtn_EnableMesh_Dynamic>();

            for (int i = 0; i < (int)DartGeneratorIndex.MaxNum; i++)
            {
                DartGeneratorMgrs[(DartGeneratorIndex)i] = _DartGeneratorMgrs[i];                                       
            }

            for (int i = 0; i < (int)MenuButton.MaxNum; i++)
            {
                MenuButton MenuButton = (MenuButton)i;
                ButtonRenderers[MenuButton] = ButtonScripts[MenuButton].GetComponentInChildren<Renderer>();
            }
        }
    }
}






