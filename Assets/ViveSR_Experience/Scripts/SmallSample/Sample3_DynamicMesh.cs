using UnityEngine;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(ViveSR_Experience))]
    public class Sample3_DynamicMesh : MonoBehaviour
    {       
        [SerializeField] protected Text LeftText, RightText, ThrowableText, DisplayMesh;
        [SerializeField] ViveSR_Experience_IDartGenerator dartGenerator;

        ViveSR_Experience_DynamicMesh DynamicMeshScript;

        private void Awake()
        {
            DynamicMeshScript = GetComponent<ViveSR_Experience_DynamicMesh>();
            ViveSR_Experience.instance.CheckHandStatus(() =>
            {
                DynamicMeshScript.SetDynamicMesh(true);
                ViveSR_Experience_ControllerDelegate.triggerDelegate += HandleTrigger;
                ViveSR_Experience_ControllerDelegate.touchpadDelegate += HandleTouchpad;
            }); 
        }

        void HandleTrigger(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {
                case ButtonStage.PressDown:
                    LeftText.enabled = true;
                    RightText.enabled = true;
                    ThrowableText.text = "Click on '<' or '>' to Change Toy";
                    break;
                case ButtonStage.PressUp:
                    LeftText.enabled = false;
                    RightText.enabled = false;
                    ThrowableText.text = "Hold Trigger to Throw Item";
                    break;
            }
        }
        void HandleTouchpad(ButtonStage buttonStage, Vector2 axis)
        {
            switch (buttonStage)
            {
                case ButtonStage.PressDown:

                    TouchpadDirection touchpadDirection = ViveSR_Experience_ControllerDelegate.GetTouchpadDirection(axis, false);

                    if (touchpadDirection == TouchpadDirection.Up)
                    {
                        DynamicMeshScript.SetMeshDisplay(!DynamicMeshScript.ShowDynamicCollision);
                        DisplayMesh.text = DisplayMesh.text == "[Show]" ? "[Hide]" : "[Show]";
                    }
                    else if (touchpadDirection == TouchpadDirection.Down)
                        dartGenerator.DestroyObjs();
                    break;
            }
        }
    }
}