using UnityEngine;
using System.Linq;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Tutorial_IInputHandler : MonoBehaviour
    {
        protected ViveSR_Experience_IButton Button;
        protected ViveSR_Experience_ISubMenu SubMenu;
        protected ViveSR_Experience_Tutorial tutorial;
 
        private void Awake()
        {
             AwakeToDo();
        }

        private void Start()
        {   
            ViveSR_Experience.instance.WaitForOneFrame(()=>SubMenu = Button.SubMenu);
            tutorial = ViveSR_Experience_Demo.instance.Tutorial;
            StartToDo();
        }

        private void Update()
        {
            UpdateToDo();
        }
        protected virtual void AwakeToDo() { }
        protected virtual void StartToDo() { }
        protected virtual void UpdateToDo() { }
        public virtual void Touched(Vector2 touchpad)
        {       
            tutorial.currentInput = tutorial.GetCurrentSprite(touchpad);

           // if (tutorial.currentInput == ControllerInputIndex.mid && ViveSR_Experience_Demo.instance.ButtonScripts[ButtonType].disabled) return;

            tutorial.RunSpriteAnimation();

            if (tutorial.currentInput != ControllerInputIndex.none)
            {
                ViveSR_Experience_Tutorial_Line TextLineFound = null;

                if (SubMenu == null && Button.isOn)
                {
                    if (!tutorial.isTriggerPressed)
                        TextLineFound = tutorial.MainLineManagers[Button.ButtonType].controllerTexts.FirstOrDefault(x => x.messageType == ControllerIndexToString(tutorial.currentInput));
                    else
                        TextLineFound = tutorial.MainLineManagers[Button.ButtonType].triggerFunctionControllerTexts.FirstOrDefault(x => x.messageType == ControllerIndexToString(tutorial.currentInput));
                }
                else if (SubMenu != null && SubMenu.subBtnScripts[SubMenu.HoverredButton].isOn)
                {
                    if (SubMenu.HoverredButton < tutorial.SubLineManagers[Button.ButtonType].controllerTexts.Count)
                    {
                        if (!tutorial.isTriggerPressed)
                            TextLineFound = tutorial.SubLineManagers[Button.ButtonType].controllerTexts[SubMenu.HoverredButton].lines.FirstOrDefault(x => x.messageType == ControllerIndexToString(tutorial.currentInput));
                        else
                            TextLineFound = tutorial.SubLineManagers[Button.ButtonType].triggerFunctionControllerTexts[SubMenu.SelectedButton].lines.FirstOrDefault(x => x.messageType == ControllerIndexToString(tutorial.currentInput));
                    }
                }

                if (TextLineFound != null)
                {
                    tutorial.SetCanvasText(TextCanvas.onTouchPad, TextLineFound.text, ViveSR_Experience_Demo.instance.AttentionColor);
                }
                else tutorial.SetCanvasText(TextCanvas.onTouchPad, GetDefaultControllerTextsMessage(tutorial.currentInput), ViveSR_Experience_Demo.instance.OriginalEmissionColor);
            }
        }

        string ControllerIndexToString(ControllerInputIndex inputIndex)
        {
            switch(inputIndex)
            {
                case ControllerInputIndex.right: return "Right";
                case ControllerInputIndex.left: return "Left";
                case ControllerInputIndex.up: return "Up";
                case ControllerInputIndex.down: return "Down";
                case ControllerInputIndex.mid: return "Mid";
                case ControllerInputIndex.trigger: return "Trigger";
                case ControllerInputIndex.grip: return "Grip";
                default: return "";
            }
        }

        public virtual void TouchedUp()
        {
            ResetSprite();
        }

        public virtual void ResetSprite()
        {
            tutorial.SetCanvas(TextCanvas.onTouchPad, false);
            tutorial.currentInput = ControllerInputIndex.none;
            tutorial.RunSpriteAnimation();  //Clean up after sprite is none.
        }

        public virtual void Pressed()
        {                              
            if (tutorial.currentInput == ControllerInputIndex.left || tutorial.currentInput == ControllerInputIndex.right)
            {
                if (!ViveSR_Experience_Demo.instance.Rotator.isRotateOn &&  ViveSR_Experience_Demo.instance.Rotator.isRotateDown) LeftRightPressedDown();
            }
            else if (tutorial.currentInput == ControllerInputIndex.up || tutorial.currentInput == ControllerInputIndex.down)
            {   
                if (SubMenu != null) SetSubBtnMessage();
            }
        }

        public virtual void PressedUp()
        {
            tutorial.isTouchpadPressed = false;
        }

        public virtual void PressedDown()
        {
            tutorial.isTouchpadPressed = true;
            if (!tutorial.isTriggerPressed)
            {   
                if (tutorial.currentInput == ControllerInputIndex.mid)
                    MidPressedDown();        
            }
        }
        
        protected void SetSubBtnMessage()
        {
            string subMsgType = "";
            if (SubMenu.subBtnScripts[SubMenu.HoverredButton].disabled) subMsgType = "Disabled";
            else if (SubMenu.subBtnScripts[SubMenu.HoverredButton].isOn) subMsgType = "On";
            else subMsgType = "Available";

            ViveSR_Experience_Tutorial_Line TextLineFound = null;

            TextLineFound = tutorial.SubLineManagers[Button.ButtonType].SubBtns[SubMenu.HoverredButton].lines.FirstOrDefault(x => x.messageType == subMsgType);
            if (TextLineFound != null) tutorial.SetCanvasText(TextCanvas.onRotator, TextLineFound.text);
        }

        protected void SetSubBtnMessage(string subMsgType)
        {
            ViveSR_Experience_Tutorial_Line TextLineFound = null;

            TextLineFound = tutorial.SubLineManagers[Button.ButtonType].SubBtns[SubMenu.SelectedButton].lines.FirstOrDefault(x => x.messageType == subMsgType);
            if (TextLineFound != null) tutorial.SetCanvasText(TextCanvas.onRotator, TextLineFound.text);
        }

        protected virtual void LeftRightPressedDown()
        {
            ViveSR_Experience_IButton CurrentButton = ViveSR_Experience_Demo.instance.Rotator.CurrentButton;
            if (SubMenu == null || !CurrentButton.SubMenu.isSubMenuOn)
            {
                tutorial.SetMainMessage();

                //Set mid btn to grey if the button is disabled
                tutorial.touchpadImages[(int)ControllerInputIndex.mid].color = Button.disabled ?
                     ViveSR_Experience_Demo.instance.DisableColor : ViveSR_Experience_Demo.instance.OriginalEmissionColor;

                tutorial.SetTouchpadSprite(true, false, ControllerInputIndex.left, ControllerInputIndex.right, ControllerInputIndex.mid);
                tutorial.SetTouchpadSprite(false, false, ControllerInputIndex.up, ControllerInputIndex.down);

                tutorial.SetCanvas(TextCanvas.onRotator, true);
                tutorial.SetCanvas(TextCanvas.onTrigger, false);
                tutorial.SetCanvas(TextCanvas.onGrip, false);
            }
        }
        protected virtual void MidPressedDown()
        {
            if (SubMenu == null) tutorial.SetMainMessage();
            else
            {
                tutorial.SetTouchpadSprite(true, ControllerInputIndex.up, ControllerInputIndex.down);
                SetSubBtnMessage();
            }     

            ViveSR_Experience_Tutorial_Line TextLineFound_Trigger = null; 
            ViveSR_Experience_Tutorial_Line TextLineFound_Grip = null;
            if (SubMenu == null)
            {
                TextLineFound_Trigger = tutorial.MainLineManagers[Button.ButtonType].controllerTexts.FirstOrDefault(x => x.messageType == ControllerIndexToString(ControllerInputIndex.trigger));
                TextLineFound_Grip = tutorial.MainLineManagers[Button.ButtonType].controllerTexts.FirstOrDefault(x => x.messageType == ControllerIndexToString(ControllerInputIndex.trigger));
            }
            else if (SubMenu.subBtnScripts[SubMenu.SelectedButton].isOn)
            {
                if (SubMenu.SelectedButton < tutorial.SubLineManagers[Button.ButtonType].controllerTexts.Count)
                {
                    TextLineFound_Trigger = tutorial.SubLineManagers[Button.ButtonType].controllerTexts[SubMenu.SelectedButton].lines.FirstOrDefault(x => x.messageType == ControllerIndexToString(ControllerInputIndex.trigger));
                    TextLineFound_Grip = tutorial.SubLineManagers[Button.ButtonType].controllerTexts[SubMenu.SelectedButton].lines.FirstOrDefault(x => x.messageType == ControllerIndexToString(ControllerInputIndex.grip));
                }
            }

            if(TextLineFound_Trigger != null) tutorial.SetCanvasText(TextCanvas.onTrigger, TextLineFound_Trigger.text, ViveSR_Experience_Demo.instance.AttentionColor);
            else tutorial.SetCanvasText(TextCanvas.onTrigger, GetDefaultControllerTextsMessage(ControllerInputIndex.trigger), ViveSR_Experience_Demo.instance.OriginalEmissionColor);

            if(TextLineFound_Grip != null) tutorial.SetCanvasText(TextCanvas.onGrip, TextLineFound_Grip.text, ViveSR_Experience_Demo.instance.AttentionColor);
            else tutorial.SetCanvasText(TextCanvas.onGrip, GetDefaultControllerTextsMessage(ControllerInputIndex.grip), ViveSR_Experience_Demo.instance.OriginalEmissionColor);

        } 
        public virtual void TriggerDown()
        {
            tutorial.isTriggerPressed = true;
            tutorial.wasTriggerCanvasActive = tutorial.IsCanvasActive(TextCanvas.onTrigger);
            tutorial.SetCanvas(TextCanvas.onTrigger, false);
        }
        public virtual void TriggerUp()
        {
            tutorial.isTriggerPressed = false;
            tutorial.SetCanvas(TextCanvas.onTrigger, tutorial.wasTriggerCanvasActive);
            tutorial.wasTriggerCanvasActive = false;
        }

        public string GetDefaultControllerTextsMessage(ControllerInputIndex defaultStringIndex)
        {
            switch (defaultStringIndex)
            {
                case ControllerInputIndex.right: return "[Click] Rotate right";
                case ControllerInputIndex.left: return "[Click] Rotate left";
                case ControllerInputIndex.up: return "[Click] Move up";
                case ControllerInputIndex.down: return "[Click] Move down";
                case ControllerInputIndex.mid: return "[Click] Confirm";
                case ControllerInputIndex.trigger: return "Hold Trigger";
                default: return "";
            }
        }
    }
}