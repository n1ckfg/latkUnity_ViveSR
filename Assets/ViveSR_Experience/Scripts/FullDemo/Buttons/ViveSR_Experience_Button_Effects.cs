using UnityEngine;
using System.Collections.Generic;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Button_Effects : ViveSR_Experience_IButton
    {
        public ViveSR_Experience_Effects EffectsScript;

        [SerializeField] Collider playerHeadCollider;
        [SerializeField] List<MeshRenderer> EffectBallRenderers;

        protected override void AwakeToDo()
        {
            ButtonType = MenuButton.Effects;
        }

        public override void ActionToDo()
        {
            playerHeadCollider.enabled = isOn;

            if (isOn) ViveSR_Experience_ControllerDelegate.triggerDelegate += HandleTrigger_Effects;
            else 
			{
				ViveSR_Experience_ControllerDelegate.triggerDelegate -= HandleTrigger_Effects;
				EffectsScript.ToggleEffects(false);
			}
        }

        void HandleTrigger_Effects(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {
                case ButtonStage.PressDown:
                    ViveSR_Experience.instance.SoundManager.PlayAtAttachPoint(AudioClipIndex.EffectBall);
                    ViveSR_Experience_Demo.instance.Rotator.RenderButtons(false);
                    EffectsScript.GenerateEffectBall();
                    for (int i = 0; i < EffectBallRenderers.Count; i++)
                    {
                        EffectBallRenderers[i].enabled = true;

                        Transform dupObjTransform = EffectBallRenderers[i].gameObject.transform.Find(i == 0 ? "A_dup" : "Base_dup");
                        if(dupObjTransform) dupObjTransform.gameObject.SetActive(false);
                    }

                    break;
                case ButtonStage.PressUp:
                    ViveSR_Experience_Demo.instance.Rotator.RenderButtons(true);
                    EffectsScript.ReleaseDart();
                    for (int i = 0; i < EffectBallRenderers.Count; i++)
                    {
                        EffectBallRenderers[i].enabled = true;
                    }

                    break;
            }
        }
    }
}