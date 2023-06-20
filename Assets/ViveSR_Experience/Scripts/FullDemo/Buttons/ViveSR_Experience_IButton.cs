using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_IButton: MonoBehaviour
    {
        public MenuButton ButtonType { get; protected set; }
        public bool isOn = false;
        public bool disableWhenRotatedAway;
        public bool disabled;
        public bool allowToggle;
        public new Renderer renderer;
         
        public ViveSR_Experience_ISubMenu SubMenu;

        public bool isShrinking, isEnlarging;

        //-1 means not in rotator included list
        public int rotatorIdx = -1;

        private void Awake()
        {           
            AwakeToDo();
        }
        protected virtual void AwakeToDo() { }

        private void Start()
        {
            StartToDo();
        }

        protected virtual void StartToDo() { }

        public void Action(bool isOn)
        {
            if (!disabled)
            {
                this.isOn = isOn;
                if(SubMenu != null)
                {
                    SubMenu.enabled = isOn;
                    SubMenu.ToggleSubMenu(isOn);
                }
                else
                {
                    ViveSR_Experience.instance.SoundManager.PlayAtAttachPoint(isOn ? AudioClipIndex.Select_On : AudioClipIndex.Select_Off);
                }
                SetButtonEmissionColor(isOn ? ColorType.Bright : ColorType.Original);
                ActionToDo();
            }
        }
        public virtual void ActionToDo() { }

        public virtual void ActOnRotator(bool isOn) 
        {
            Action(isOn);
        }

        protected void Update()
        {
            UpdateToDo();
        }
        protected virtual void UpdateToDo() { }

        public virtual void ForceExcuteButton(bool on)
        {
            Action(on);
        }

        public void EnableButton(bool on)
        {
            disabled = !on;

            SetButtonEmissionColor(on ? ColorType.Original : ColorType.Disable);
        }

        public void SetButtonEmissionColor(ColorType colorType)
        {
            Color color = Color.clear;

            if (colorType == ColorType.Bright) color = ViveSR_Experience_Demo.instance.BrightColor;
            else if (colorType == ColorType.Original) color = ViveSR_Experience_Demo.instance.OriginalEmissionColor;
            else if (colorType == ColorType.Disable) color = ViveSR_Experience_Demo.instance.DisableColor;
            else if (colorType == ColorType.Attention) color = ViveSR_Experience_Demo.instance.AttentionColor;

            renderer.material.SetColor("_EmissionColor", color);
        }

        public void SetButtonColor(Color color)
        {
            renderer.material.SetColor("_Color", color);
        }

        public void SetButtonTexture(Texture texture)
        {
            renderer.material.mainTexture = texture;
            renderer.material.SetTexture("_EmissionMap", texture);
        }
    }
}






