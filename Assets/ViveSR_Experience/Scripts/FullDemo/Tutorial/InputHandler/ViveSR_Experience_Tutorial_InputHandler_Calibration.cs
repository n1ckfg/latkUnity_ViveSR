using UnityEngine;using UnityEngine.UI;using System.Linq;namespace Vive.Plugin.SR.Experience{    public class ViveSR_Experience_Tutorial_InputHandler_Calibration : ViveSR_Experience_Tutorial_IInputHandler    {
        ViveSR_Experience_Calibration calibrationScript;

        protected override void StartToDo()
        {
            Button = ViveSR_Experience_Demo.instance.ButtonScripts[MenuButton.Calibration];
            calibrationScript = ViveSR_Experience_Demo.instance.CalibrationScript;
        }

        public override void Touched(Vector2 touchpad)        {
            if (calibrationScript.hasSpinningReset) //Only when spinning
            {    
                tutorial.currentInput = ControllerInputIndex.none;
                tutorial.SetTouchpadSprite(false, ControllerInputIndex.mid);
                tutorial.SetTouchpadImage(false);
                tutorial.SetCanvas(TextCanvas.onTouchPad, true);

                Spin();
            }
            else
            {
                base.Touched(touchpad);
            }
        }

        public override void TouchedUp()
        {
            base.TouchedUp();

            if (calibrationScript.isCalibrating && !calibrationScript.hasSpinningReset)
            {
                ResetSpinnerImages();
                tutorial.SetTouchpadSprite(true, ControllerInputIndex.mid);
            }
        }        protected override void LeftRightPressedDown()
        {
            if (!calibrationScript.isCalibrating)
            {
                base.LeftRightPressedDown();
            }
        }        public override void PressedDown()
        {
            if (tutorial.currentInput == ControllerInputIndex.left || tutorial.currentInput == ControllerInputIndex.right)
            {
                if (!calibrationScript.isCalibrating)
                {
                    base.PressedDown();
                }
                
                if (calibrationScript.isCalibrating && !calibrationScript.hasSpinningReset)
                {
                    ResetSpinnerImages();
                }
            }
            else if (tutorial.currentInput == ControllerInputIndex.up || tutorial.currentInput == ControllerInputIndex.down)
            {
                if (!calibrationScript.isCalibrating)
                {
                    tutorial.SetCanvas(TextCanvas.onRotator, true);

                    //Set rotator message for calibration
                    Calibration_SubBtn currentSubBtn = (Calibration_SubBtn)SubMenu.SelectedButton;

                    ViveSR_Experience_IButton CurrentButton = ViveSR_Experience_Demo.instance.Rotator.CurrentButton;

                    tutorial.SetCanvasText(TextCanvas.onRotator, tutorial.SubLineManagers[CurrentButton.ButtonType].SubBtns[SubMenu.HoverredButton].lines.First(x => x.messageType == "Available").text);
                 //   if (currentSubBtn == Calibration_SubBtn.Alignment) tutorial.SetRotatorCanvas(true);
                }
            }
            else if (tutorial.currentInput == ControllerInputIndex.mid)
            {                                       
                MidPressedDown();
            }
        }

        protected override void MidPressedDown()
        {                                              
            if (!calibrationScript.isActiveAndEnabled) //calibration menu
            {
                ResetSpinnerImages();
                foreach (Image img in tutorial.spinngerImage) img.enabled = false;

                base.MidPressedDown();
                tutorial.SetCanvas(TextCanvas.onRotator, true);
                tutorial.SetTouchpadSprite(true, false, ControllerInputIndex.left, ControllerInputIndex.right, ControllerInputIndex.up, ControllerInputIndex.down, ControllerInputIndex.mid);
            }
            else //Calibrating
            {
                tutorial.SetTouchpadSprite(true, true, ControllerInputIndex.left, ControllerInputIndex.right, ControllerInputIndex.up, ControllerInputIndex.down, ControllerInputIndex.mid);
                tutorial.SetCanvas(TextCanvas.onRotator, false);
            }
        }

        void ResetSpinnerImages()
        {
            tutorial.SetTouchpadImage(true);
            tutorial.SetCanvas(TextCanvas.onTouchPad, false);
            foreach (Image img in tutorial.spinngerImage) img.enabled = false;
            tutorial.targetSpinnerImageNumber_Prev = -1;
        }

        void Spin()
        {
            if (calibrationScript.rotatingAngle > 0) tutorial.targetSpinnerImageNumber = (SubMenu.SelectedButton == (int)Calibration_SubBtn.Focus)? 1 : 0;
            else if (calibrationScript.rotatingAngle < 0) tutorial.targetSpinnerImageNumber = (SubMenu.SelectedButton == (int)Calibration_SubBtn.Focus) ? 0:1;

            if (calibrationScript.isSpinning)
                if (tutorial.targetSpinnerImageNumber == 1)
                    tutorial.spinngerImage[1].gameObject.transform.localEulerAngles += new Vector3(0f, 0f, 1f);
                else if (tutorial.targetSpinnerImageNumber == 0)
                    tutorial.spinngerImage[0].gameObject.transform.localEulerAngles += new Vector3(0f, 0f, -1f);

            if (tutorial.targetSpinnerImageNumber_Prev != tutorial.targetSpinnerImageNumber)
            {
                bool isClockWise = tutorial.targetSpinnerImageNumber == 0;
                tutorial.SetCanvasText(TextCanvas.onTouchPad, isClockWise ? "[Spin] Rotate Clockwise" : "[Spin] Rotate Counter Clockwise");
                tutorial.spinngerImage[0].enabled = isClockWise;
                tutorial.spinngerImage[1].enabled = !isClockWise;
            }
            tutorial.targetSpinnerImageNumber_Prev = tutorial.targetSpinnerImageNumber;
        }
    }}