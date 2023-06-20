using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(ViveSR_Experience))]
    public class Sample7_Portal : MonoBehaviour
    {    
        [SerializeField] GameObject TriggerHint, LeftHint, RightHint;

        ViveSR_Experience_IDartGenerator dartGenerator;
        ViveSR_Experience_Portal PortalScript;
        ViveSR_Experience_Effects EffectsScript;

        private void Awake()
        {
            EffectsScript = GetComponent<ViveSR_Experience_Effects>();
            PortalScript = GetComponent<ViveSR_Experience_Portal>();
            dartGenerator = PortalScript.dartGenerator.GetComponent<ViveSR_Experience_IDartGenerator>();
        }

        private void OnEnable()
        {
            ViveSR_Experience.instance.CheckHandStatus(()=>
            {
                PortalScript.SetPortal(true);
                ViveSR_Experience_ControllerDelegate.triggerDelegate += HandleTrigger_ThrowableItemUI;
                ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpad_ControlPortal;
                ViveSR_Experience_ControllerDelegate.gripDelegate += HandleGrip_SwitchEffects;
            });
        }

        void HandleTrigger_ThrowableItemUI(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {
                case ButtonStage.PressDown:
                    TriggerHint.SetActive(false);
                    RightHint.SetActive(true);
                    LeftHint.SetActive(true);
                    break;

                case ButtonStage.PressUp:
                    TriggerHint.SetActive(true);
                    RightHint.SetActive(false);
                    LeftHint.SetActive(false);
                    break;
            }
        }

        void HandleGrip_SwitchEffects(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {
                case ButtonStage.PressDown:
                    EffectsScript.CurrentEffectNumber += 1;
                    if (EffectsScript.CurrentEffectNumber == (int)ImageEffectType.TOTAL_NUM) EffectsScript.CurrentEffectNumber = -1;
                   
                    EffectsScript.ChangeShader(EffectsScript.CurrentEffectNumber);
                break;
            }
        }

        void HandleTouchpad_ControlPortal(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {    
                case ButtonStage.PressDown:

                    TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, false);

                    switch (touchpadDirection)
                    {
                        case TouchpadDirection.Up:
                            PortalScript.ResetPortalPosition();
                            break;
                        case TouchpadDirection.Down:
                            dartGenerator.DestroyObjs();
                            break;
                    }
                 
                    break;
            } 
        }
    }
}
