using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Calibration : MonoBehaviour
    {
        public bool isCalibrating;

        //AxisZ
        float startingAngle;
        float currentAngle;
        float temptime;

        //spinning
        public bool isSpinning { get; private set; }
        public bool hasSpinningReset { get; private set; }
        public float rotatingAngle {get; private set; }

        //AxisXY
        bool isMoving;
        bool isTouchPadPressed;
        [SerializeField] ViveSR_Experience_SubMenu_Calibration CalibrationSubMenu;

        private void OnEnable()
        {
            ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpadInput_Calibrating; //Layer 3: calibrating
        }

        private void OnDisable()
        {
            ViveSR_Experience_ControllerDelegate.touchpadDelegate -= HandleTouchpadInput_Calibrating; //Layer 3: calibrating
        }

        void HandleTouchpadInput_Calibrating(ButtonStage buttonStage, Vector2 axis)
        {
            TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, true);

            switch (buttonStage)
            {
                case ButtonStage.PressDown:
                    isTouchPadPressed = true;

                    switch (touchpadDirection)
                    {
                        case TouchpadDirection.Mid:
                            if (!hasSpinningReset) CalibrationSubMenu.ReturnToSubMenu(); 
                            break;
                        default:
                            ResetCalibrationTouch(axis);
                            break;
                    }

                    break;
                case ButtonStage.PressUp:
                    isTouchPadPressed = false;
                    break;

                case ButtonStage.Press:
                    if (Time.time - temptime > 0.5) //Long press
                    {
                        isMoving = true;

                        ViveSR_DualCameraCalibrationTool calibrationTool = ViveSR_DualCameraRig.Instance.DualCameraCalibration;

                        switch (touchpadDirection)
                        {
                            case TouchpadDirection.Right:
                                calibrationTool.Calibration(CalibrationAxis.Y, Time.deltaTime * 2); //Right
                                break;
                            case TouchpadDirection.Left:
                                calibrationTool.Calibration(CalibrationAxis.Y, -Time.deltaTime * 2); //Left
                                break;
                            case TouchpadDirection.Up:
                                calibrationTool.Calibration(CalibrationAxis.X, -Time.deltaTime * 2); //Up
                                break;
                            case TouchpadDirection.Down:
                                calibrationTool.Calibration(CalibrationAxis.X, Time.deltaTime * 2); //Down
                                break;
                        }
                    }
                    break;

                case ButtonStage.TouchDown:
                    ResetCalibrationTouch(axis);
                    break;

                case ButtonStage.TouchUp:
                    isSpinning = false;
                    hasSpinningReset = false;
                    break;

                case ButtonStage.Touch:
                    if (!isTouchPadPressed)
                    {
                        switch (touchpadDirection)
                        {
                            case TouchpadDirection.Mid:
                                break;
                            default:
                                RotateAxisZ(axis);
                                break;
                        }
                    }
                    break;
            } 
        }

        void ResetCalibrationTouch(Vector2 touchPad)
        {
            //Set startingAngle and convert Vector2 to degree.
            startingAngle = Vector2.Angle(new Vector2(1, 0), touchPad);
            if (touchPad.y > 0) startingAngle = 360 - startingAngle;

            //For detecting long press.
            temptime = Time.timeSinceLevelLoad;

            //Make changing Axis Z and Axis XY mutual excusive.
            isSpinning = false;
            hasSpinningReset = false;
            isMoving = false;
        }

        void RotateAxisZ(Vector2 touchPad)
        {
            //Set currentAngle and convert Vector2 to degree.
            currentAngle = Vector2.Angle(new Vector2(1, 0), touchPad);
            if (touchPad.y > 0) currentAngle = 360 - currentAngle;

            if (!isMoving)
            {
                rotatingAngle = 0;
                
                //Only works when moving more than 5 degrees.
                if (Mathf.Abs(currentAngle - startingAngle) < 300f)
                {
                    if (currentAngle > startingAngle + 5) RotateAxisZ_SetAngle(true);
                    else if (currentAngle < startingAngle - 5) RotateAxisZ_SetAngle(false);
                    else isSpinning = false;
                } 
                else
                {
                    if (currentAngle < 10 && currentAngle + 360 > startingAngle + 5) RotateAxisZ_SetAngle(true);
                    else if (currentAngle > 300 && currentAngle < 360 + startingAngle - 5) RotateAxisZ_SetAngle(false);
                    else isSpinning = false;
                }

                if (isSpinning) ViveSR_DualCameraRig.Instance.DualCameraCalibration.Calibration(CalibrationAxis.Z, rotatingAngle);
                
                startingAngle = currentAngle;
            }
        }

        void RotateAxisZ_SetAngle(bool isClockwise)
        {
            int shouldInvert = CalibrationSubMenu.SelectedButton == (int)Calibration_SubBtn.Focus ? 1 : -1;
            rotatingAngle = shouldInvert * (isClockwise ? -1 : 1) * Time.deltaTime * 5;
            isSpinning = true;
            hasSpinningReset = true;
        }
    }
}