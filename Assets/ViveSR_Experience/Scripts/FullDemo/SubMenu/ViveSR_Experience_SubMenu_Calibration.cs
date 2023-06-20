namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_SubMenu_Calibration : ViveSR_Experience_ISubMenu
    {
        ViveSR_Experience_Calibration calibrationScript;

        protected override void StartToDo()
        {
            calibrationScript = ViveSR_Experience_Demo.instance.CalibrationScript;
        }

        protected override void Execute()
        {
            if(SelectedButton != (int)Calibration_SubBtn.Reset) calibrationScript.enabled = true;
            base.Execute();
        }

        public void StartCalibration()
        {
            ViveSR_Experience_Demo.instance.Rotator.SetRotator(false);
            ViveSR_Experience_Demo.instance.ButtonScripts[MenuButton.Calibration].SubMenu.isSubMenuOn = false;
            ViveSR_Experience_Demo.instance.ButtonScripts[MenuButton.Calibration].SubMenu.subBtnScripts[HoverredButton].isOn = true;

            //Hide the sub menu
            RenderSubBtns(false);
            ViveSR_Experience_Demo.instance.Rotator.RenderButtons(false);

            //Activate the choosen calibration mode
            calibrationScript.isCalibrating = true;
            ViveSR_DualCameraRig.Instance.DualCameraCalibration.SetCalibrationMode(true, (CalibrationType)SelectedButton);

            //Show digital controller
            if (!ViveSR_Experience.instance.ShowControllerModel() && SelectedButton == (int)Calibration_SubBtn.Alignment) ViveSR_Experience.instance.SetControllerRenderer(true);
        }

        public void ReturnToSubMenu()
        {                                                          
            ViveSR_Experience_Demo.instance.Rotator.SetRotator(true);
            ViveSR_Experience_Demo.instance.ButtonScripts[MenuButton.Calibration].SubMenu.isSubMenuOn = true;
            ViveSR_Experience_Demo.instance.ButtonScripts[MenuButton.Calibration].SubMenu.subBtnScripts[SelectedButton].isOn = false;

            //isSubMenuOn = true;           
            if (!ViveSR_Experience.instance.ShowControllerModel() && SelectedButton == (int)Calibration_SubBtn.Alignment) ViveSR_Experience.instance.SetControllerRenderer(false);

            ViveSR_Experience_HintMessage.instance.HintTextFadeOff(hintType.onController, 0f);

            calibrationScript.isCalibrating = false;

            RenderSubBtns(true);
            ViveSR_Experience_Demo.instance.Rotator.RenderButtons(true);

            ViveSR_DualCameraRig.Instance.DualCameraCalibration.SetCalibrationMode(false);
            calibrationScript.enabled = false;
        }
    }
}
