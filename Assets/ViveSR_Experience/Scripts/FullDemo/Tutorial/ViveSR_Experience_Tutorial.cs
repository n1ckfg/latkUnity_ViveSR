using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Vive.Plugin.SR.Experience
{    
    public class ViveSR_Experience_Tutorial : MonoBehaviour
    {     
        [SerializeField] bool IsTutorialOn;

        //For storing SteamVR touchpad gameobject so as to position Tutorial canvases
        GameObject touchpadGameObj;
        bool isTouchpadUISet;

        [SerializeField] List<GameObject> LineManagers = new List<GameObject>();
        [SerializeField] List<ViveSR_Experience_Tutorial_IInputHandler> _InputHandlers; //For handling inputs for differernt buttons

        public Dictionary<MenuButton, ViveSR_Experience_Tutorial_MainLineManager> MainLineManagers = new Dictionary<MenuButton, ViveSR_Experience_Tutorial_MainLineManager>();
        public Dictionary<MenuButton, ViveSR_Experience_Tutorial_SubLineManager> SubLineManagers = new Dictionary<MenuButton, ViveSR_Experience_Tutorial_SubLineManager>();
        public Dictionary<MenuButton, ViveSR_Experience_Tutorial_IInputHandler> InputHandlers = new Dictionary<MenuButton, ViveSR_Experience_Tutorial_IInputHandler>();

        public ControllerInputIndex currentInput = ControllerInputIndex.none;
        public ControllerInputIndex previousSprite = ControllerInputIndex.none;
        public GameObject touchpadImageGroup;
        public List<Image> touchpadImages;
        List<bool> touchpadImages_isFocused = new List<bool>();

        [SerializeField] List<ViveSR_Experience_Tutorial_TextureSwap> touchpadScripts;
        IEnumerator currentTouchPadCoroutine;

        [Header("Spinner")]
        [SerializeField] GameObject touchpadSpinnerImageGroup;
        public List<Image> spinngerImage;
        public int targetSpinnerImageNumber;
        public int targetSpinnerImageNumber_Prev = -1;

        [Header("Canvases")]
        [SerializeField] List<GameObject> tutorialCanvases;
        [SerializeField] List<Text> tutorialTexts;                
        
        public bool isTriggerPressed, isTouchpadPressed, wasTriggerCanvasActive;
        private void Awake()
        {
            for (int i = 0; i < touchpadImages.Count; i++)
                touchpadImages_isFocused.Add(false);
             
            for (int i = 0; i < (int)MenuButton.MaxNum; i++)
            {
                MainLineManagers[(MenuButton)i] = LineManagers[i].GetComponent<ViveSR_Experience_Tutorial_MainLineManager>();
                SubLineManagers[(MenuButton)i] = LineManagers[i].GetComponent<ViveSR_Experience_Tutorial_SubLineManager>();
                InputHandlers[(MenuButton)i] = _InputHandlers[i];
               
            }
        }

        private void Start()
        {
            foreach (Text text in tutorialTexts)
            {
                text.color = ViveSR_Experience_Demo.instance.OriginalEmissionColor;
                text.transform.parent.SetParent(ViveSR_Experience.instance.AttachPoint.transform);
            }
            tutorialCanvases[(int)TextCanvas.onGrip].transform.localEulerAngles = new Vector3(20f, 0f, 4f);
            tutorialCanvases[(int)TextCanvas.onGrip].transform.localPosition = new Vector3(-0.058f, -0.13f, -0.06f);
            tutorialCanvases[(int)TextCanvas.onTouchPad].transform.localPosition = new Vector3(0.07f, -0.085f, -0.056f);
            tutorialCanvases[(int)TextCanvas.onRotator].transform.localPosition = new Vector3(0f, -0.035f, -0.11f);
            tutorialCanvases[(int)TextCanvas.onTrigger].transform.localEulerAngles = new Vector3(20f, 0f, 4f);
            tutorialCanvases[(int)TextCanvas.onTrigger].transform.localPosition = new Vector3(-0.05f, -0.1f, -0.03f);
            touchpadImageGroup.transform.localPosition = touchpadSpinnerImageGroup.transform.localPosition = new Vector3(0f, -0.085f, -0.059f);
            touchpadImageGroup.transform.localEulerAngles = touchpadSpinnerImageGroup.transform.localEulerAngles = new Vector3(154f, 180f, -0.053f);
            touchpadImageGroup.transform.localScale = touchpadSpinnerImageGroup.transform.localScale = new Vector3(0.0004f, 0.0004f, 0.0004f);

            ViveSR_Experience.instance.CheckHandStatus(() =>
            {
                ToggleTutorial(IsTutorialOn);
                SetMainMessage();
                ViveSR_Experience_ControllerDelegate.touchpadDelegate_Late += HandleTouchpad_Tutorial;
                ViveSR_Experience_ControllerDelegate.triggerDelegate_Late += HandleTrigger_Tutorial;
            });
        }

        void HandleTrigger_Tutorial(ButtonStage buttonStage, Vector2 axis)
        {
            ViveSR_Experience_IButton CurrentButton = ViveSR_Experience_Demo.instance.Rotator.CurrentButton;
            if (IsTutorialOn && CurrentButton.isOn)
            {
                switch (buttonStage)
                {
                    case ButtonStage.PressDown:
                        InputHandlers[CurrentButton.ButtonType].TriggerDown();
                        break;
                    case ButtonStage.PressUp:
                        InputHandlers[CurrentButton.ButtonType].TriggerUp();
                        break;
                }
            }
        }

        public void RunSpriteAnimation()
        {                                  
            if (currentInput != previousSprite)
            {
                //Previous...
                //Disables animation.
                if (currentTouchPadCoroutine != null)
                {
                    StopCoroutine(currentTouchPadCoroutine);
                    currentTouchPadCoroutine = null;
                }
                if (previousSprite != ControllerInputIndex.none)
                {
                    Color color = touchpadImages_isFocused[(int)previousSprite] ? ViveSR_Experience_Demo.instance.AttentionColor : ViveSR_Experience_Demo.instance.OriginalEmissionColor;
                    touchpadScripts[(int)previousSprite].isAnimating = false;
                    if(touchpadImages[(int)previousSprite].color != ViveSR_Experience_Demo.instance.DisableColor) touchpadImages[(int)previousSprite].color = color;
                }

                //Current...
                //Enables animation.
                if (currentInput == ControllerInputIndex.none)
                {
                    SetCanvasText(TextCanvas.onTouchPad, "");
                    SetCanvas(TextCanvas.onTouchPad, false);
                }
                else
                {                                  
                    SetCanvas(TextCanvas.onTouchPad, true);
                              
                    //Start animating the hovered sprite.
                    touchpadScripts[(int)currentInput].isAnimating = true;
                    currentTouchPadCoroutine = touchpadScripts[(int)currentInput].Animate();
                    StartCoroutine(currentTouchPadCoroutine);

                    //Set touched sprite color to highlight.
                    touchpadImages[(int)currentInput].color = ViveSR_Experience_Demo.instance.BrightColor;//the hovered sprite
                }
                previousSprite = currentInput;
            }
        }

        public ControllerInputIndex GetCurrentSprite(Vector2 axis)
        {               
            TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, true);
          
            switch (touchpadDirection)
            {
                case TouchpadDirection.Mid:
                    currentInput = ControllerInputIndex.mid;
                    break;
                case TouchpadDirection.Right:
                    currentInput = ControllerInputIndex.right;
                    break;
                case TouchpadDirection.Left:
                    currentInput = ControllerInputIndex.left;
                    break;
                case TouchpadDirection.Up:
                    currentInput = ControllerInputIndex.up;
                    break;
                case TouchpadDirection.Down:
                    currentInput = ControllerInputIndex.down;
                    break;
            }

            if (currentInput != ControllerInputIndex.none)
            {   
                if (touchpadScripts[(int)currentInput].isDisabled)
                    currentInput = ControllerInputIndex.none;
            }
            return currentInput;
        }   

        void HandleTouchpad_Tutorial(ButtonStage buttonStage, Vector2 axis)
        {
            if (IsTutorialOn)
            {
                currentInput = GetCurrentSprite(axis);
                ViveSR_Experience_IButton CurrentButton = ViveSR_Experience_Demo.instance.Rotator.CurrentButton;
                switch (buttonStage)
                {
                    case ButtonStage.Press:
                        InputHandlers[CurrentButton.ButtonType].Pressed();
                        break;
                    case ButtonStage.PressUp:
                        InputHandlers[CurrentButton.ButtonType].PressedUp();
                        break;
                    case ButtonStage.PressDown:
                        InputHandlers[CurrentButton.ButtonType].PressedDown();
                        break;
                    case ButtonStage.TouchUp:
                        InputHandlers[CurrentButton.ButtonType].TouchedUp();
                        break;
                    case ButtonStage.Touch:
                        InputHandlers[CurrentButton.ButtonType].Touched(axis);
                        break;
                }
            }
        }

        public void SetTouchpadSprite(bool isAvailable, params ControllerInputIndex[] indexes)
        {        
            foreach (ControllerInputIndex index in indexes)
            {
                Color AvailableColor = touchpadImages_isFocused[(int)index] ? ViveSR_Experience_Demo.instance.AttentionColor : ViveSR_Experience_Demo.instance.OriginalEmissionColor;
                touchpadImages[(int)index].color = isAvailable ? AvailableColor : ViveSR_Experience_Demo.instance.DisableColor;
                touchpadImages[(int)index].color = isAvailable ? AvailableColor : ViveSR_Experience_Demo.instance.DisableColor;
                touchpadScripts[(int)index].isDisabled = !isAvailable;
                touchpadScripts[(int)index].isDisabled = !isAvailable;
            }
        }

        public void SetTouchpadSprite(bool isAvailable, bool isFocused, params ControllerInputIndex[] indexes)
        {
            SetTouchpadSpriteFocused(isFocused, indexes);
            SetTouchpadSprite(isAvailable, indexes);
        }

        void SetTouchpadSpriteFocused(bool isFocused, params ControllerInputIndex[] indexes)
        {
            foreach (ControllerInputIndex index in indexes)
            {
                touchpadImages_isFocused[(int)index] = isFocused;
            }
        }

        public void SetMainMessage()
        {
            string msgType;

            ViveSR_Experience_IButton CurrentButton = ViveSR_Experience_Demo.instance.Rotator.CurrentButton;

            if (CurrentButton.disabled)
                msgType = "Disabled";
            else if (CurrentButton.isOn)
                msgType = "On";
            else msgType = "Available";

            ViveSR_Experience_Tutorial_Line TextLineFound = MainLineManagers[CurrentButton.ButtonType].mainLines.FirstOrDefault(x => x.messageType == msgType);

            if (TextLineFound != null) SetCanvasText(TextCanvas.onRotator, MainLineManagers[CurrentButton.ButtonType].mainLines.First(x => x.messageType == msgType).text);
            else SetCanvasText(TextCanvas.onRotator, "Message Not Found.");
        }

        public void ToggleTutorial(bool isOn)
        {
            IsTutorialOn = isOn;                                   
            if(!isOn) SetCanvas(TextCanvas.onTouchPad, false);
            SetCanvas(TextCanvas.onRotator, isOn);
            SetTouchpadImage(isOn);
            SetTouchpadSprite(isOn, ControllerInputIndex.left, ControllerInputIndex.right, ControllerInputIndex.mid);

            if(!isOn)
            {
                currentInput = ControllerInputIndex.none;
                RunSpriteAnimation(); //stop previous coroutine;
            }
        }
                                                           
        public void SetCanvas(TextCanvas textCanvas, bool on)
        {
            tutorialCanvases[(int)textCanvas].SetActive(on);
        }
        public bool IsCanvasActive(TextCanvas textCanvas)
        {
            return tutorialCanvases[(int)textCanvas].activeSelf;
        }

        public void SetCanvasText(TextCanvas textCanvas, string text)
        {
            tutorialTexts[(int)textCanvas].text = text;
        }
        public void SetCanvasText(TextCanvas textCanvas, string text, Color color)
        {
            tutorialTexts[(int)textCanvas].text = text;
            tutorialTexts[(int)textCanvas].color = color;
        }
        public void SetTouchpadImage(bool on)
        {
            touchpadImageGroup.SetActive(on);
        }
    }
}