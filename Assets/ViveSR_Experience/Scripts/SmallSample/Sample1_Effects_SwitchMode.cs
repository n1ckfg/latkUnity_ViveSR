using UnityEngine;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(ViveSR_Experience))]
    public class Sample1_Effects_SwitchMode : MonoBehaviour
    {
        ViveSR_Experience_Effects EffectsScript;
        ViveSR_Experience_SwitchMode SwitchModeScript;

        [SerializeField] GameObject canvas;
        [SerializeField] Text EffectText;

        bool isTriggerDown;

        private void Awake()
        {
            EffectsScript = GetComponent<ViveSR_Experience_Effects>();
            SwitchModeScript = GetComponent<ViveSR_Experience_SwitchMode>();
            ViveSR_Experience.instance.CheckHandStatus(() =>
            {
                ViveSR_Experience_ControllerDelegate.triggerDelegate += HandleTrigger;
                ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpad;
            });
        }
        void HandleTrigger(ButtonStage buttonStage, Vector2 axis)
        {
            if (SwitchModeScript.currentMode == DualCameraDisplayMode.MIX)
            {
                switch (buttonStage)
                {
                    case ButtonStage.PressDown:
                        EffectsScript.GenerateEffectBall();
                        canvas.SetActive(false);
                        isTriggerDown = true;
                        break;
                    case ButtonStage.PressUp:
                        EffectsScript.ReleaseDart();
                        canvas.SetActive(true);
                        isTriggerDown = false;
                        break;
                }
            }
        }

        void HandleTouchpad(ButtonStage buttonStage, Vector2 axis)
        {
            if (!isTriggerDown)
            {
                switch (buttonStage)
                {
                    case ButtonStage.PressDown:
                        SwitchModeScript.SwithMode(SwitchModeScript.currentMode == DualCameraDisplayMode.MIX ? DualCameraDisplayMode.VIRTUAL : DualCameraDisplayMode.MIX);
                        EffectsScript.ChangeShader(-1);

                        EffectText.text = SwitchModeScript.currentMode == DualCameraDisplayMode.MIX ? "Effect Candy->" : "";
                        break;
                }
            }
        }    
    }
}