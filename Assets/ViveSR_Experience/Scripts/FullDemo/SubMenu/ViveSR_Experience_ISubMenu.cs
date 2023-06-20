using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_ISubMenu : MonoBehaviour
    {
        public bool isSubMenuOn;
        public int HoverredButton { get; private set; }
        public int SelectedButton { get; private set; }

        [SerializeField] Texture originalTexture;
        [SerializeField] Texture selectingModeTexture;

        public List<GameObject> subBtns;
        public List<ViveSR_Experience_ISubBtn> subBtnScripts;

        float subBtnEnlargingSpeed = 2f;
        Vector3 Vector_Enlarged, Vector_RegularSize;
        float EnlargedSize = 1.5f, RegularSize = 1f;

        bool isSwitching;

        private void Awake()
        {
            Vector_Enlarged = Vector3.one * EnlargedSize;
            Vector_RegularSize = Vector3.one * RegularSize;
            AwakeToDo();
        }

        protected virtual void AwakeToDo(){}

        private void Start()
        {
            StartToDo();
        }
        protected virtual void StartToDo() { }

        public virtual void ToggleSubMenu(bool isOn)
        {
            isSubMenuOn = isOn;
            RenderSubBtns(isOn);

            ViveSR_Experience_IButton CurrentButton = ViveSR_Experience_Demo.instance.Rotator.CurrentButton;

            CurrentButton.SetButtonTexture(isOn ? selectingModeTexture : originalTexture);

            if (isSubMenuOn)
            {
                subBtnScripts[HoverredButton].isShrinking = false;
                Enlarge(subBtnScripts[HoverredButton]);
                ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpadInput_SubMenu;
            }
            else
            {
                subBtnScripts[HoverredButton].isEnlarging = false;
                Shirnk(subBtnScripts[HoverredButton]);
                ViveSR_Experience_ControllerDelegate.touchpadDelegate -= HandleTouchpadInput_SubMenu;
            }

            if (!isOn)
            {
                for (int i = 0; i < subBtnScripts.Count; i++)
                {
                    if (subBtnScripts[i].isOn == true) subBtnScripts[i].Execute();
                }
            }
         }

        void Enlarge(ViveSR_Experience_ISubBtn button)
        {
            ViveSR_Experience.instance.SoundManager.PlayAtAttachPoint(AudioClipIndex.Click);
            StartCoroutine(_Enlarge(true, button));
        }

        void Shirnk(ViveSR_Experience_ISubBtn button)
        {
            StartCoroutine(_Enlarge(false, button));
        }

        public IEnumerator _Enlarge(bool on, ViveSR_Experience_ISubBtn button)
        {
            if (on)
            {
                button.isEnlarging = true;
                isSwitching = true;
            }
            else button.isShrinking = true;

            bool a = true;
            bool b = true;

            //on ? enlarge : shrink
            while (a && b)
            {
                button.transform.localScale += (on? 1 : -1) * new Vector3(subBtnEnlargingSpeed * Time.deltaTime, subBtnEnlargingSpeed * Time.deltaTime, subBtnEnlargingSpeed * Time.deltaTime);

                a = on ? button.transform.localScale.x < EnlargedSize : button.transform.localScale.x > RegularSize;
                b = on ? button.isEnlarging : button.isShrinking;

                yield return new WaitForEndOfFrame();
            }

            button.transform.localScale = on ? Vector_Enlarged : Vector_RegularSize;
            if (on)
            {
                button.isEnlarging = false;
                isSwitching = false;
            }
            else button.isShrinking = false;  
        }

        bool AllowHover()
        {
            return !isSwitching 
                   && !ViveSR_RigidReconstruction.IsExportingMesh
                   && !ViveSR_Experience_Demo.instance.StaticMeshScript.ModelIsLoading;
        }

        void HandleTouchpadInput_SubMenu(ButtonStage buttonStage, Vector2 axis)
        {
            if (isSubMenuOn)
            {
                TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, true);

                switch (buttonStage)
                {
                    case ButtonStage.PressDown:
                        switch (touchpadDirection)
                        {
                            case TouchpadDirection.Mid: Execute(); break;
                        }
                        break;
                    case ButtonStage.Press:
                        if (AllowHover())
                        {
                            switch (touchpadDirection)
                            {
                                case TouchpadDirection.Up: HoverBtn(false); break;
                                case TouchpadDirection.Down: HoverBtn(true); break;
                            }
                        }
                        break;
                }
            }
        }

        void HoverBtn(bool isDown)
        {                                                    
            if ((!isDown && HoverredButton - 1 < 0) || (isDown && HoverredButton + 1 > subBtnScripts.Count - 1)) return;

            subBtnScripts[HoverredButton].isEnlarging = false;
            Shirnk(subBtnScripts[HoverredButton]);

            HoverredButton += isDown ? 1 : -1;

            //Enlarges the currently hovered subBtn
            subBtnScripts[HoverredButton].isShrinking = false;
            Enlarge(subBtnScripts[HoverredButton]);
        }
        
        //When mid is pressed...
        protected virtual void Execute()
        {
            ViveSR_Experience_IButton CurrentButton = ViveSR_Experience_Demo.instance.Rotator.CurrentButton;
            if (!CurrentButton.disabled && !subBtnScripts[HoverredButton].disabled)
            {
                SelectedButton = HoverredButton;
                subBtnScripts[SelectedButton].Execute();
                ViveSR_Experience.instance.SoundManager.PlayAtAttachPoint(subBtnScripts[SelectedButton].isOn ? AudioClipIndex.Select_On : AudioClipIndex.Select_Off);
            };
        }

        public void RenderSubBtns(bool on)
        {
            foreach (ViveSR_Experience_ISubBtn subBtn in subBtnScripts)
                subBtn.renderer.enabled = on;
        }
    }
}