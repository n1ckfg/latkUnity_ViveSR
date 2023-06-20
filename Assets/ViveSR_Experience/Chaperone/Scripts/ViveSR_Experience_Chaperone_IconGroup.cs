using UnityEngine;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience.Chaperone
{
    public class ViveSR_Experience_Chaperone_IconGroup : MonoBehaviour
    {
        private const uint StatusLength = (uint)Status.Error + 1;
        public GameObject[] Objs = new GameObject[StatusLength];
        [HideInInspector] public RawImage[] Imgs = new RawImage[StatusLength];
        [HideInInspector] public UnityEngine.UI.Button[] Btns = new UnityEngine.UI.Button[StatusLength];

        private void Awake()
        {
            for (int i = 0; i < Objs.Length; i++)
            {
                Imgs[i] = Objs[i].GetComponent<RawImage>();
                Btns[i] = Objs[i].GetComponent<UnityEngine.UI.Button>();
                if (Imgs[i] == null || Btns[i] == null)
                {
                    Debug.LogError("Please add reference");
                    return;
                }
            }
        }

        public void EnableIcon(int index)
        {
            for (int i = 0; i < Objs.Length; i++) Objs[i].SetActive(false);
            Objs[index].SetActive(true);
        }
    }
}