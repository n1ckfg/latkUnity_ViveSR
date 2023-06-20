using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Effects : MonoBehaviour
    {
        [Header("EffectBalls")]
        public int CurrentEffectNumber = -1;

        [SerializeField] GameObject EffectBall;
        [SerializeField] Renderer EffectballRenderer;
        [SerializeField] List<Texture> EffectImages;

        [Header("ImageEffect")]
        [SerializeField] ViveSR_Experience_PostEffects postEffectScript;

        public void GenerateEffectBall()
        {
            //Switch effect balls.            
            CurrentEffectNumber = (CurrentEffectNumber + 1) % (int)ImageEffectType.TOTAL_NUM;    // loop from 0 to 3
            EffectBall.SetActive(true);
            EffectballRenderer.material.mainTexture = EffectImages[CurrentEffectNumber];
        }
        public void ReleaseDart()
        {
            EffectBall.SetActive(false);
        }

        public void ChangeShader(int index)
        {
            if (index == -1)
            {
                postEffectScript.gameObject.SetActive(false);
            }
            else
            {
                postEffectScript.gameObject.SetActive(true);
                postEffectScript.SetEffectShader((ImageEffectType)index);
            }
        }

        public void ToggleEffects(bool isOn)
        {
            postEffectScript.gameObject.SetActive(isOn);
        }

    }
}