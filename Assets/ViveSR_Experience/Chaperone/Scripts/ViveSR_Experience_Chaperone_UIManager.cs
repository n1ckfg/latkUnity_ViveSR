using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience.Chaperone
{
    public class ViveSR_Experience_Chaperone_UIManager : MonoBehaviour
    {
        public Text Title;
        public Text Description;
        public Text Message;
        public ViveSR_Experience_Chaperone_IconGroup ViveIcon;
        public ViveSR_Experience_Chaperone_IconGroup ChaperoneIcon;

        [HideInInspector] public UnityAction btnViveIdleOnClick;
        [HideInInspector] public UnityAction btnViveWorkOnClick;
        [HideInInspector] public UnityAction btnViveErrorOnClick;
        [HideInInspector] public UnityAction btnChaperoneIdleOnClick;
        [HideInInspector] public UnityAction btnChaperoneWorkOnClick;
        [HideInInspector] public UnityAction btnChaperoneErrorOnClick;

        private int factor = 1;

        #region singleton
        private ViveSR_Experience_Chaperone_UIManager() { }
        private static ViveSR_Experience_Chaperone_UIManager Mgr = null;
        public static ViveSR_Experience_Chaperone_UIManager Instance
        {
            get
            {
                if (Mgr == null)
                {
                    Mgr = FindObjectOfType<ViveSR_Experience_Chaperone_UIManager>();
                }
                if (Mgr == null)
                {
                    Debug.LogError("UIManager does not be attached on GameObject");
                }
                return Mgr;
            }
        }
        #endregion

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            Message.text = ViveSR_Experience_Chaperone_GlobalManager.Instance.LastErrMsg;

            ViveIcon.EnableIcon((int)ViveSR_Experience_Chaperone_GlobalManager.EnvironmentStatus);
            ChaperoneIcon.EnableIcon((int)ViveSR_Experience_Chaperone_GlobalManager.ChaperoneStatus);

            /*switch (GlobalManager.EnvironmentStatus)
            {
                case Status.Idle:
                    break;
                case Status.Start:
                    break;
                case Status.Work:
                    break;
                case Status.Error:
                    break;
            }*/
            switch (ViveSR_Experience_Chaperone_GlobalManager.ChaperoneStatus)
            {
                case Status.Idle:
                    break;
                case Status.Start:
                    Color color = ChaperoneIcon.Imgs[(int)Status.Start].color;
                    color.a += Time.time / 15 * factor;
                    factor = color.a >= 0.8f ? -1 : color.a <= 0.2f ? 1 : factor;
                    ChaperoneIcon.Imgs[(int)Status.Start].color = color;
                    break;
                case Status.Work:
                    break;
                case Status.Error:
                    break;
            }
        }
    }
}