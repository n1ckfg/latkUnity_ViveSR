﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vive.Plugin.SR.Experience
{
    enum ValueToGet
    {
        Max = 0,
        Min,
        Value,
        Add,
        DefaultValue
    }

    public enum ControlMode
    {
        ConfidenceThreshold = 0,
        DenoiseGuidedFilter,
        DenoiseMedianFilter,
        Refinement,
        EdgeEnhance,
        DepthCase,
        MaxNum,
    }

    public class ViveSR_Experience_DepthControl : MonoBehaviour
    {     
        [SerializeField] Material depthImageMaterial;

        [Header("Slider UI")]
        [SerializeField] List<Slider> sliders;
        [Header("On/Off UI")]
        [SerializeField] List<Text> switches_status;

        List<float> Custom_Values = new List<float>();
        bool Custom_isEdgeEnhanceOn;
        DepthCase Custom_DepthCase;

        float ConfidenceThreshold_old;

        List<Button> Left_Btns = new List<Button>();
        List<Button> Right_Btns = new List<Button>();

        public void ResetPanelPos()
        {
            Transform targethandTrans = ViveSR_Experience.instance.targetHand.transform;
            transform.position = targethandTrans.position + targethandTrans.forward * 0.4f;
            transform.forward = targethandTrans.forward;
        }

        void Awake()
        {
            ViveSR_Experience.instance.CheckHandStatus(() =>
            {
                Reset();

                //Assign depthImageMaterial to ViveSR.
                if (ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.Depth.Count > 0)
                    ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.Depth[0] = depthImageMaterial;
                else
                    ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.Depth.Add(depthImageMaterial);

                SetListener(true);
            });
        }

        private void OnEnable()
        {
            Transform PlayerHeand = ViveSR_Experience.instance.PlayerHeadCollision.transform;
            transform.position = PlayerHeand.position + new Vector3(0, -0.25f, 0) + PlayerHeand.forward * 0.8f;
            transform.forward = PlayerHeand.forward;
            ViveSR_DualCameraImageCapture.EnableDepthProcess(true);
            ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = true;
        }
        private void OnDisable()
        {
            ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = false;
            ViveSR_DualCameraImageCapture.EnableDepthProcess(false);
        }

        void SetListener(bool isOn)
        {
            for (int modeNum = 0; modeNum < (int)ControlMode.MaxNum; modeNum++)
            {
                ControlMode controlmode = (ControlMode)modeNum; //prevents listener reference error

                if (modeNum < sliders.Count)
                {
                    Left_Btns.Add(sliders[modeNum].transform.Find("Left_Btn").GetComponent<Button>());
                    Right_Btns.Add(sliders[modeNum].transform.Find("Right_Btn").GetComponent<Button>());
                    sliders[modeNum].onValueChanged.AddListener(x =>
                    {
                        ViveSR_Experience.instance.SoundManager.PlayAtAttachPoint(AudioClipIndex.Drag);
                        SetValue(controlmode, (int)x);
                    });
                    Left_Btns[modeNum].onClick.AddListener(() =>
                    {
                        ViveSR_Experience.instance.SoundManager.PlayAtAttachPoint(AudioClipIndex.Click);
                        AdjustValue(controlmode, false);
                    });
                    Right_Btns[modeNum].onClick.AddListener(() =>
                    {
                        ViveSR_Experience.instance.SoundManager.PlayAtAttachPoint(AudioClipIndex.Click);
                        AdjustValue(controlmode, true);
                    });
                }
                else
                {
                    switches_status[modeNum - sliders.Count].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ViveSR_Experience.instance.SoundManager.PlayAtAttachPoint(AudioClipIndex.Drag);
                        AdjustValue(controlmode);                    
                    });
                }
            }
        }

        public void AdjustValue(ControlMode controlMode, bool isAdd = false)
        {
            switch (controlMode)
            {
                case ControlMode.Refinement: 
                    SetRefinement(!ViveSR_DualCameraImageCapture.DepthRefinement);
                    break;

                case ControlMode.EdgeEnhance:
                    SetEdgeEnhance(!ViveSR_DualCameraImageCapture.DepthEdgeEnhance);
                    break;

                case ControlMode.DepthCase:
                    SetDepthCase(ViveSR_DualCameraImageCapture.DepthCase == DepthCase.DEFAULT ? DepthCase.CLOSE_RANGE : DepthCase.DEFAULT);
                    break;

                default:
                    
                    float add = GetValue(controlMode, ValueToGet.Add);

                    sliders[(int)controlMode].value += isAdd ? add : -add;

                    break;
            }

        }

        void SetRefinement(bool isOn)
        {
            ViveSR_DualCameraImageCapture.EnableDepthRefinement(isOn);

            for (int i = 0; i < sliders.Count; i++)     //lock slider values when DepthRefinement is On  
                sliders[i].interactable = Right_Btns[i].interactable = Left_Btns[i].interactable = !isOn;

            //Best setting for Refinement
            if (isOn)
            {
                SaveCustomValue();

                sliders[(int)ControlMode.ConfidenceThreshold].value = 4;     //actually value to sdk: 0
                sliders[(int)ControlMode.DenoiseGuidedFilter].value = 7;     //actually value to sdk: 7
                sliders[(int)ControlMode.DenoiseMedianFilter].value = 2;     //actually value to sdk: 2
            }
            else LoadCustomValue();

            switches_status[(int)ControlMode.Refinement - sliders.Count].text = ViveSR_DualCameraImageCapture.DepthRefinement ? "On" : "Off";
        }

        void SetEdgeEnhance(bool isOn)
        {
            ViveSR_DualCameraImageCapture.EnableDepthEdgeEnhance(isOn);
            switches_status[(int)ControlMode.EdgeEnhance - sliders.Count].text = ViveSR_DualCameraImageCapture.DepthEdgeEnhance ? "On" : "Off";
        }

        void SetDepthCase(DepthCase DepthCase)
        {
            ViveSR_DualCameraImageCapture.ChangeDepthCase(DepthCase);
            switches_status[(int)ControlMode.DepthCase - sliders.Count].text = ViveSR_DualCameraImageCapture.DepthCase == DepthCase.DEFAULT ? "Default" : "Close Range";
        }

        float GetValue(ControlMode controlMode, ValueToGet ValueToGet)
        {
            switch (controlMode)
            {
                case ControlMode.ConfidenceThreshold:
                    switch (ValueToGet)
                    {
                        case ValueToGet.Max:
                            return 9;
                        case ValueToGet.Min:
                            return 0;
                        case ValueToGet.Value:
                            return sliders[(int)controlMode].value;
                        case ValueToGet.Add:
                            return 1f;
                        case ValueToGet.DefaultValue:
                            return 3;
                        default:
                            return -1;
                    }
                case ControlMode.DenoiseGuidedFilter:
                    switch (ValueToGet)
                    {
                        case ValueToGet.Max:
                            return 7;
                        case ValueToGet.Min:
                            return 0;
                        case ValueToGet.Value:
                            return sliders[(int)controlMode].value;
                        case ValueToGet.Add:
                            return 1;
                        case ValueToGet.DefaultValue:
                            return 3;
                        default:
                            return -1;
                    }
                case ControlMode.DenoiseMedianFilter:
                    switch (ValueToGet)
                    {
                        case ValueToGet.Max:
                            return 2;
                        case ValueToGet.Min:
                            return 0;
                        case ValueToGet.Value:
                            return sliders[(int)controlMode].value;
                        case ValueToGet.Add:
                            return 1;
                        case ValueToGet.DefaultValue:
                            return 2;
                        default:
                            return -1;
                    }
                default:
                    return -1;
            }
        }

        void SetValue(ControlMode controlMode, float SliderValue) 
        {
            switch (controlMode)
            {
                case ControlMode.ConfidenceThreshold:

                    int A = (int)sliders[(int)controlMode].minValue;
                    int B = (int)sliders[(int)controlMode].maxValue;
                    int C = -1;
                    int D = 1;

                    float value = (SliderValue - A) / (B - A) * (D - C) + C;//map from from 0~9 to -1~1 

                    ViveSR_DualCameraImageCapture.DepthConfidenceThreshold = Mathf.Pow(10f, value) / 2 - 0.05f; //smaller numbers have more details

                    break;
                case ControlMode.DenoiseGuidedFilter:

                    ViveSR_DualCameraImageCapture.DepthDenoiseGuidedFilter = (int)SliderValue; //0-7

                    break;
                case ControlMode.DenoiseMedianFilter:

                    ViveSR_DualCameraImageCapture.DepthDenoiseMedianFilter = (int)SliderValue + (int)SliderValue + 1; //1, 3, 5 
                    break;
            }
        }

        void LoadCustomValue()
        {
            if (Custom_Values.Count > 0)
            {
                for (int i = 0; i < sliders.Count; i++) sliders[i].value = Custom_Values[i];
                SetEdgeEnhance(Custom_isEdgeEnhanceOn);
                SetDepthCase(Custom_DepthCase);
            }
        }

        void SaveCustomValue()
        {     
            Custom_Values.Clear();
            for (int i = 0; i < sliders.Count; i++) Custom_Values.Add(GetValue((ControlMode)i, ValueToGet.Value));

            Custom_isEdgeEnhanceOn = ViveSR_DualCameraImageCapture.DepthEdgeEnhance;
            Custom_DepthCase = ViveSR_DualCameraImageCapture.DepthCase;
        }

        public void Reset()
        {
            for (int i = 0; i < sliders.Count; i++)
            {
                ControlMode mode = (ControlMode)i;
                sliders[i].value = GetValue(mode, ValueToGet.DefaultValue);
            }
          
            ViveSR_DualCameraImageCapture.EnableDepthRefinement(false);
            ViveSR_DualCameraImageCapture.EnableDepthEdgeEnhance(false);
            ViveSR_DualCameraImageCapture.SetDefaultDepthCase(DepthCase.DEFAULT);
        }
    }
}