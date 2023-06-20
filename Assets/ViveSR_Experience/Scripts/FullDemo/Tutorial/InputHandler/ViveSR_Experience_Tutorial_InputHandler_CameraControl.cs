﻿namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Tutorial_InputHandler_CameraControl : ViveSR_Experience_Tutorial_IInputHandler
    {
        protected override void StartToDo()
        {
            Button = ViveSR_Experience_Demo.instance.ButtonScripts[MenuButton.CameraControl];
        }

        protected override void MidPressedDown()
        {
            base.MidPressedDown();
            tutorial.SetCanvas(TextCanvas.onTrigger, Button.isOn);
            tutorial.SetTouchpadSprite(Button.isOn, Button.isOn, ControllerInputIndex.up);
            tutorial.SetTouchpadSprite(true, Button.isOn, ControllerInputIndex.mid);
        }

        public override void TriggerDown()
        {
            base.TriggerDown();       
            SetUIDisplay(false);
        }

        public override void TriggerUp()
        {
            base.TriggerUp();
            if(!tutorial.isTouchpadPressed) SetUIDisplay(true);
        }

        public override void PressedDown()
        {
            base.PressedDown();

            if (tutorial.currentInput == ControllerInputIndex.up)
            {
                SetUIDisplay(false);
            }
        }
        public override void PressedUp()
        {
            base.PressedUp();

            if (!tutorial.isTriggerPressed && Button.isOn) SetUIDisplay(true);
        }

        void SetUIDisplay(bool isOn)
        {
            tutorial.SetCanvas(TextCanvas.onTrigger, isOn);
            tutorial.SetCanvas(TextCanvas.onRotator, isOn);
        }
    }
}