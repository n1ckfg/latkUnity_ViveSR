using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public enum ButtonStage
    {
        None,
        PressDown,
        Press,
        PressUp,
        TouchDown,
        TouchUp,
        Touch
    }     
    
    public enum TouchpadDirection
    {
        None,
        Up,
        Down,
        Left,
        Right,
        Mid
    }

    public class ViveSR_Experience_ControllerDelegate : MonoBehaviour
    {            
        public delegate void TriggerDelegate(ButtonStage buttonStage, Vector2 axis);
        public static TriggerDelegate triggerDelegate = null;
        public static TriggerDelegate triggerDelegate_Late = null;
        public delegate void TouchpadDelegate(ButtonStage buttonStage, Vector2 axis);
        public static TouchpadDelegate touchpadDelegate = null;
        public static TouchpadDelegate touchpadDelegate_Late = null;
        public delegate void GripDelegate(ButtonStage buttonStage, Vector2 axis);
        public static TouchpadDelegate gripDelegate = null;
        public static TouchpadDelegate gripDelegate_Late = null;

        bool isLate;

        private void Update()
        {
            if(triggerDelegate != null) UpdateController(SteamVR_Controller.ButtonMask.Trigger);
            if(touchpadDelegate != null) UpdateController(SteamVR_Controller.ButtonMask.Touchpad);
            if (gripDelegate != null) UpdateController(SteamVR_Controller.ButtonMask.Grip);

            //Late
            if (triggerDelegate_Late != null) UpdateController(SteamVR_Controller.ButtonMask.Trigger, true);
            if (touchpadDelegate_Late != null) UpdateController(SteamVR_Controller.ButtonMask.Touchpad, true);
            if (gripDelegate_Late != null) UpdateController(SteamVR_Controller.ButtonMask.Grip, true);
        }
        public static TouchpadDirection GetTouchpadDirection(Vector2 axis, bool includeMid)
        {
            float deg;
            TouchpadDirection touchpadDirection = TouchpadDirection.None;

            if (includeMid & Vector2.Distance(axis, Vector2.zero) < 0.5f) return TouchpadDirection.Mid;

            if (axis.x == 0) deg = axis.y >= 0 ? 90 : -90;
            else deg = Mathf.Atan(axis.y / axis.x) * Mathf.Rad2Deg;

            if (axis.x >= 0)
            {
                if (deg >= 45f) touchpadDirection = TouchpadDirection.Up;
                else if (deg < 45f && deg > -45) touchpadDirection = TouchpadDirection.Right;
                else if (deg <= -45) touchpadDirection = TouchpadDirection.Down;
            }
            else
            {
                if (deg >= 45f) touchpadDirection = TouchpadDirection.Down;                                                                                                     
                else if (deg < 45f && deg > -45) touchpadDirection = TouchpadDirection.Left;
                else if (deg <= -45) touchpadDirection = TouchpadDirection.Up;
            }

            return touchpadDirection;
        }
        void UpdateController(ulong buttonMask, bool isLate = false)
        {
            ButtonStage targetStage = ButtonStage.None;

            if (ViveSR_Experience.instance.targetHandScript.controller.GetPressDown(buttonMask))
                targetStage = ButtonStage.PressDown;
            else if (ViveSR_Experience.instance.targetHandScript.controller.GetPress(buttonMask))
                targetStage = ButtonStage.Press;
            else if (ViveSR_Experience.instance.targetHandScript.controller.GetPressUp(buttonMask))
                targetStage = ButtonStage.PressUp;
            else if (ViveSR_Experience.instance.targetHandScript.controller.GetTouchDown(buttonMask))
                targetStage = ButtonStage.TouchDown;
            else if (ViveSR_Experience.instance.targetHandScript.controller.GetTouchUp(buttonMask))
                targetStage = ButtonStage.TouchUp;
            else if (ViveSR_Experience.instance.targetHandScript.controller.GetTouch(buttonMask))
                targetStage = ButtonStage.Touch;

            if (targetStage != ButtonStage.None)
            {
                if (!isLate)
                {
                    if (buttonMask == SteamVR_Controller.ButtonMask.Trigger)
                        triggerDelegate(targetStage, ViveSR_Experience.instance.targetHandScript.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger));
                    else if (buttonMask == SteamVR_Controller.ButtonMask.Touchpad)
                        touchpadDelegate(targetStage, ViveSR_Experience.instance.targetHandScript.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad));
                    else if (buttonMask == SteamVR_Controller.ButtonMask.Grip)
                        gripDelegate(targetStage, ViveSR_Experience.instance.targetHandScript.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Grip));
                }
                else
                {
                    if (buttonMask == SteamVR_Controller.ButtonMask.Trigger)
                        triggerDelegate_Late(targetStage, ViveSR_Experience.instance.targetHandScript.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger));
                    else if (buttonMask == SteamVR_Controller.ButtonMask.Touchpad)
                        touchpadDelegate_Late(targetStage, ViveSR_Experience.instance.targetHandScript.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad));
                    else if (buttonMask == SteamVR_Controller.ButtonMask.Grip)
                        gripDelegate_Late(targetStage, ViveSR_Experience.instance.targetHandScript.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Grip));
                }
            }
        }
    }
}
