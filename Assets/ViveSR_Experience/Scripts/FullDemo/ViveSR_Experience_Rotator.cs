using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Rotator : MonoBehaviour
    {
        public bool isRotateOn { get; private set; }

        float distToCenter = -0.034f;
        float rotateSpeed = 200;
        float buttonEnlargingSpeed = 1.7f;

        bool isTouchPositive = false;
        float localY, targetY;
        float rotateAngle;

        public ViveSR_Experience_IButton CurrentButton { get; private set; }

        Vector3 Vector_Enlarged, Vector_RegularSize;
        float EnlargedSize = 1.2f, RegularSize = 0.8f;

        List<ViveSR_Experience_IButton> offsetButtons = new List<ViveSR_Experience_IButton>();
        List<int> oldOffsetButtonDegrees = new List<int>();

        public bool isRotateDown { get; private set; }

        [SerializeField] List<ViveSR_Experience_IButton> IncludedBtns;

        public void SetRotator(bool isOn)
        {
            isRotateOn = isOn;
        }

        private void Awake()
        {
            Vector_Enlarged = Vector3.one * EnlargedSize;
            Vector_RegularSize = Vector3.one * RegularSize;

            ViveSR_Experience.instance.CheckHandStatus(() =>
            {
                InitRotator();
                ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpad_RotatorControl;
            });
        }

        void InitRotator()
        {                 
            Color _oriColor = ViveSR_Experience_Demo.instance.OriginalEmissionColor;
            Color fadedColor = new Color(_oriColor.r, _oriColor.g, _oriColor.b, 0.2f);

            for (int i = 1; i < IncludedBtns.Count; i++)         // set default trainsparency. not changing depth image button.
                IncludedBtns[i].SetButtonColor(fadedColor);

            //Rotate the Buttons and then expands them to form a circle.
            for (int i = 0; i < IncludedBtns.Count; i++)
            {
                //Add 90 degrees to match the controller's orientation.
                if (i == 0) rotateAngle = 90;

                //Rotate the Buttons.
                IncludedBtns[i].gameObject.transform.localEulerAngles += new Vector3(0, rotateAngle, 0);

                //Extend the Button from the geo center of all Buttons.
                IncludedBtns[i].gameObject.transform.GetChild(0).transform.localPosition += new Vector3(distToCenter, 0, 0);

                //Accumulate the degree number for the next button.
                rotateAngle += 360 / IncludedBtns.Count;

                IncludedBtns[i].rotatorIdx = i;
            }

            //Move the UI to Attachpoint.
            transform.localPosition = new Vector3(0, -0.085f, 0);
            if (distToCenter >= 0) transform.localScale = new Vector3(-3, 3, 3);
            else transform.localScale = new Vector3(3, 3, -3);

            //Assign & Enlarge the current Button.
            CurrentButton = IncludedBtns[0];
            CurrentButton.gameObject.transform.GetChild(0).transform.localScale *= 1.3f;
            CurrentButton.gameObject.transform.localScale = Vector_Enlarged;

            //Push & scale down non-current buttons
            AdjustUITransform();

            //Display of included buttons
            RenderButtons(true);
        }

        public void HandleTouchpad_RotatorControl(ButtonStage buttonStage, Vector2 axis)
        {
            TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, true);

            if (isRotateOn)
            {
               switch (buttonStage)
                {
                    case ButtonStage.PressDown:

                        switch (touchpadDirection)
                        {
                            case TouchpadDirection.Mid: //Mid: Excute the choosen Buttn.
                                ActOnButton();
                                break;
                            case TouchpadDirection.Right:  isRotateDown = true;  break;
                            case TouchpadDirection.Left: isRotateDown = true; break;
                        }

                        break;

                    case ButtonStage.Press:
                        if (isRotateDown)
                        {
                            switch (touchpadDirection)
                            {
                                case TouchpadDirection.Right:
                                    StartCoroutine(Rotate(true));
                                    break;
                                case TouchpadDirection.Left:
                                    StartCoroutine(Rotate(false));
                                    break;
                            }
                        }
                        break;
                }

            }
            switch (buttonStage)
            {
                case ButtonStage.PressUp:
                    {
                        isRotateDown = false;
                    }
                    break;
            }
        }
               
        void Enlarge(ViveSR_Experience_IButton button, System.Action done = null)
        {
            ViveSR_Experience.instance.SoundManager.PlayAtAttachPoint(AudioClipIndex.Slide);
            ColorFade(CurrentButton, false);

            StartCoroutine(_Enlarge(true, button, done));
        }   
        void Shirnk(ViveSR_Experience_IButton button, System.Action done = null)
        {
            ColorFade(CurrentButton, true);

            StartCoroutine(_Enlarge(false, button, done));
        }
        IEnumerator _Enlarge(bool on, ViveSR_Experience_IButton button, System.Action done)
        {
            if (on) button.isEnlarging = true;
            else button.isShrinking = true;

            bool a = true;
            bool b = true;

            //on ? enlarge : shrink
            while (a && b)
            {
                button.transform.localScale += (on ? 1 : -1) * new Vector3(buttonEnlargingSpeed * Time.deltaTime, buttonEnlargingSpeed * Time.deltaTime, buttonEnlargingSpeed * Time.deltaTime);

                a = on ? button.transform.localScale.x < EnlargedSize : button.transform.localScale.x > RegularSize;
                b = on ? button.isEnlarging : button.isShrinking;

                yield return new WaitForEndOfFrame();
            }

            button.transform.localScale = on ? Vector_Enlarged : Vector_RegularSize;
            if (on) button.isEnlarging = false;
            else button.isShrinking = false;

            if (done != null) done();
        }

        void ColorFade(ViveSR_Experience_IButton button, bool isFading)
        {
            StartCoroutine(_ColorFade(button, isFading));
        }
        IEnumerator _ColorFade(ViveSR_Experience_IButton button, bool isFading)
        {
            Color newC;

            bool a = true, b = true;

            while (a && b)
            {    
                newC = new Color(button.renderer.material.color.r, button.renderer.material.color.g, button.renderer.material.color.b,
                   button.renderer.material.color.a + 3f * Time.deltaTime * (isFading ? -1 : 1));

                button.SetButtonColor(newC);
                         
                a = isFading ? (button.renderer.material.color.a > 0.3f) : (button.renderer.material.color.a < 0.95f);
                b = isFading ? button.isShrinking : button.isEnlarging;

                yield return new WaitForEndOfFrame();
            }

            newC = new Color(button.renderer.material.color.r, button.renderer.material.color.g, button.renderer.material.color.b,
                  isFading? 0.3f : 1f);

            button.SetButtonColor(newC);
        }

        ViveSR_Experience_IButton GetButtons(bool isTouchPositive, int num)
        {
            int tempNum = (int)CurrentButton.ButtonType;
            tempNum += isTouchPositive ? num : -num;
            if (tempNum < 0) tempNum = IncludedBtns.Count + tempNum;
            else if (tempNum > IncludedBtns.Count - 1) tempNum = tempNum % IncludedBtns.Count;
            return ViveSR_Experience_Demo.instance.ButtonScripts[(MenuButton)tempNum];
        }     

        void AdjustUITransform()
        {
            for (int i = 0; i < offsetButtons.Count; i++)
            {
                Vector3 vec = offsetButtons[i].gameObject.transform.localEulerAngles;
                offsetButtons[i].gameObject.transform.localEulerAngles = new Vector3(vec.x, oldOffsetButtonDegrees[i], vec.z);
            }

            offsetButtons.Clear();
            oldOffsetButtonDegrees.Clear();

            List<ViveSR_Experience_IButton> Btns_Before = new List<ViveSR_Experience_IButton>();
            List<ViveSR_Experience_IButton> Btns_After = new List<ViveSR_Experience_IButton>();

            //get 3 buttons before & after current
            for (int i = 1; i < IncludedBtns.Count/2; i++) Btns_Before.Add(GetButtons(true, i));
            for (int i = 1; i < IncludedBtns.Count/2; i++) Btns_After.Add(GetButtons(false, i));

            OffsetButton(!isTouchPositive, Btns_Before.ToArray());
            OffsetButton(isTouchPositive, Btns_After.ToArray());
        }  

        void OffsetButton(bool isPositive, params ViveSR_Experience_IButton[] buttons)
        {
            float accelerator = 1f;
            int count = 0;
            foreach (ViveSR_Experience_IButton button in buttons)
            {
                offsetButtons.Add(button);

                Vector3 vec = button.gameObject.transform.localEulerAngles;

                int degree = 16;

                oldOffsetButtonDegrees.Add((int)vec.y);
                if (count == 0) accelerator = 1;
                else if (count == 1) accelerator = 0.63f;
                else if (count == 2) accelerator = 0.3f;
                float targetY = vec.y + degree * (isPositive ? 1 : -1) * accelerator;
                button.gameObject.transform.localEulerAngles = new Vector3(vec.x, targetY, vec.z);

                count += 1;
            }
        }

        IEnumerator Rotate(bool isTouchPositive)
        {                    
            SetRotator(false);

            /*---The Old Button---*/                
            CurrentButton.isEnlarging = false;
            Shirnk(CurrentButton);  

            if (CurrentButton.isOn && CurrentButton.disableWhenRotatedAway)
                CurrentButton.ActOnRotator(false);

            ViveSR_Experience_IButton oldBtn = CurrentButton;
            /*--------------------*/

            /*---The New Button---*/
            CurrentButton = GetButtons(isTouchPositive, 1);

            //Enlarge
            CurrentButton.isShrinking = false;
            Enlarge(CurrentButton);

            //Set the target degree.
            targetY = localY + (360 / (int)MenuButton.MaxNum) * (isTouchPositive ? 1 : -1);

            CurrentButton.gameObject.transform.GetChild(0).transform.localScale *= 1.2f;

            while (isTouchPositive ? localY < targetY : localY > targetY)
            {
                localY += (isTouchPositive ? 1 : -1) * rotateSpeed * Time.deltaTime;
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, localY, transform.localEulerAngles.z);

                yield return new WaitForEndOfFrame();
            }

            targetY = localY = (360 / IncludedBtns.Count) * CurrentButton.rotatorIdx;
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, targetY, transform.localEulerAngles.z);

            oldBtn.gameObject.transform.GetChild(0).transform.localScale = Vector3.one * 0.03f;

            AdjustUITransform();
            SetRotator(true);

            /*--------------------*/
        }

        void ActOnButton()
        {
            //Toggle on and off. Some Buttons do not allow toggling off.
            if ((CurrentButton.isOn && CurrentButton.allowToggle) || !CurrentButton.isOn)
            {
                if (!CurrentButton.disabled)
                {
                    CurrentButton.isOn = !CurrentButton.isOn;
                    CurrentButton.ActOnRotator(CurrentButton.isOn);
                }
            }
        }

        public void RenderButtons(bool isOn)
        {
            for (int i = 0; i < IncludedBtns.Count; i++)
                IncludedBtns[i].renderer.enabled = isOn;
                   
            CurrentButton.disabled = !isOn;
            SetRotator(isOn);
        }
    }
}