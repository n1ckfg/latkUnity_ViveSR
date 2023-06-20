//========= Copyright 2017, HTC Corporation. All rights reserved. ===========

using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR.Experience.Chaperone
{
    public class ViveSR_Experience_Chaperone_ImageRenderer : MonoBehaviour
    {
        private static bool UpdateChaperoneMaterial = false;

        public List<Material> UndistortedLeft;
        public List<Material> ChaperoneLeft;
        public List<Material> ChaperoneMask;
        public List<Material> ChaperoneDepth;

        private ViveSR_Timer ChaperoneTimer = new ViveSR_Timer();
        public static float RealChaperoneFPS;
        private int LastChaperoneTextureUpdateTime = 0;

        public ViveSR_Experience_Overlay sr_overlay;
        private bool OverlayPtrInitial;
        public int index = 1;
        public Material matOverlayEdge;
        public Material matOverlayEdgeDepth;
        public Material matOverlayColor;
        public Material matOverlayThermal;

        Texture2D textureChaperoneMask;
        Texture2D textureChaperoneLeft;
        Texture2D textureChaperoneDepth;
        //private Matrix4x4[] PoseUndistorted = new Matrix4x4[2];
        //private Texture2D[] TextureUndistorted = new Texture2D[2];

        private void Start()
        {
            UpdateChaperoneMaterial = true;

            sr_overlay._Width = ViveSR_Experience_Chaperone_ImageCapture.HumanDectionImageWidth / (float)ViveSR_DualCameraImageCapture.FocalLength_L * sr_overlay.transform.localPosition.z;
            sr_overlay.UpdateOverlay(ViveSR_Experience_Overlay.Features.Width);
        }

        private void Update()
        {
            if (ViveSR_Experience_Chaperone_DualCameraRig.DualCameraStatus == DualCameraStatus.WORKING)
            {
                ViveSR_Experience_Chaperone_ImageCapture.UpdateHumanDectionImage();

                if (Input.GetKeyDown(KeyCode.F1))
                {
                    index = 0;
                }
                else if (Input.GetKeyDown(KeyCode.F2))
                {
                    index = 1;
                }
                else if (Input.GetKeyDown(KeyCode.F3))
                {
                    index = 2;
                }
                else if (Input.GetKeyDown(KeyCode.F4))
                {
                    index = 3;
                }
                else if (Input.GetKeyDown(KeyCode.F5))
                {
                    bool hasPlayAreaMask = ViveSR_Experience_Chaperone_ImageCapture.HasPlayAreaMask;
                    ViveSR_Experience_Chaperone_ImageCapture.EnablePlayAreaMask(!hasPlayAreaMask);
                    Debug.Log("EnablePlayAreaMask " + ViveSR_Experience_Chaperone_ImageCapture.HasPlayAreaMask);
                }

                #region Chaperone Image
                if (UpdateChaperoneMaterial)
                {
                    int currentChaperoneTimeIndex = ViveSR_Experience_Chaperone_ImageCapture.HumanDectionTimeIndex;
                    if (currentChaperoneTimeIndex != LastChaperoneTextureUpdateTime)
                    {
                        ChaperoneTimer.Add(currentChaperoneTimeIndex - LastChaperoneTextureUpdateTime);
                        RealChaperoneFPS = 1000 / ChaperoneTimer.AverageLeast(100);
                        int frameIndex, timeIndex;

                        ViveSR_Experience_Chaperone_ImageCapture.GetHumanDectionTexture(out textureChaperoneLeft, out textureChaperoneMask, out textureChaperoneDepth, out frameIndex, out timeIndex);
                        for (int i = 0; i < ChaperoneMask.Count; i++)
                        {
                            if (ChaperoneMask[i] != null) ChaperoneMask[i].mainTexture = textureChaperoneMask;
                        }
                        for (int i = 0; i < ChaperoneLeft.Count; i++)
                        {
                            if (ChaperoneLeft[i] != null) ChaperoneLeft[i].mainTexture = textureChaperoneLeft;
                        }
                        for (int i = 0; i < ChaperoneDepth.Count; i++)
                        {
                            if (ChaperoneDepth[i] != null) ChaperoneDepth[i].mainTexture = textureChaperoneDepth;
                        }
                        LastChaperoneTextureUpdateTime = currentChaperoneTimeIndex;

                        if (sr_overlay != null)
                        {
                            if (matOverlayEdge == null)
                                sr_overlay._Texture = textureChaperoneLeft;
                            else
                            {
                                if (index == 0)
                                {
                                    if (ViveSR_Experience_Chaperone_ImageCapture.HasDepthMask)
                                        ViveSR_Experience_Chaperone_ImageCapture.EnableDepthMask(false);
                                    matOverlayEdge.SetTexture("_AlphaTex", textureChaperoneMask);
                                    sr_overlay._Texture = ForceShaderPass(textureChaperoneLeft, matOverlayEdge);
                                }
                                else if(index == 1)
                                {
                                    if (!ViveSR_Experience_Chaperone_ImageCapture.HasDepthMask)
                                        ViveSR_Experience_Chaperone_ImageCapture.EnableDepthMask(true);
                                    matOverlayEdgeDepth.SetTexture("_AlphaTex", textureChaperoneMask);
                                    matOverlayEdgeDepth.SetTexture("_DepthTex", textureChaperoneDepth);
                                    sr_overlay._Texture = ForceShaderPass(textureChaperoneLeft, matOverlayEdgeDepth);
                                }
                                else if(index == 2)
                                {
                                    if (ViveSR_Experience_Chaperone_ImageCapture.HasDepthMask)
                                        ViveSR_Experience_Chaperone_ImageCapture.EnableDepthMask(false);
                                    matOverlayColor.SetTexture("_AlphaTex", textureChaperoneMask);
                                    sr_overlay._Texture = ForceShaderPass(textureChaperoneLeft, matOverlayColor);
                                }
                                else if (index == 3)
                                {
                                    if (ViveSR_Experience_Chaperone_ImageCapture.HasDepthMask)
                                        ViveSR_Experience_Chaperone_ImageCapture.EnableDepthMask(false);
                                    matOverlayThermal.SetTexture("_AlphaTex", textureChaperoneMask);
                                    sr_overlay._Texture = ForceShaderPass(textureChaperoneLeft, matOverlayThermal);
                                }
                            }
                            if (!OverlayPtrInitial)
                            {
                                sr_overlay.UpdateOverlay(ViveSR_Experience_Overlay.Features.TexturePtr);
                                OverlayPtrInitial = true;
                            }
                            sr_overlay.UpdateOverlay(ViveSR_Experience_Overlay.Features.Texture);
                            sr_overlay.UpdateOverlay(ViveSR_Experience_Overlay.Features.Transform);
                        }
                        if (ViveSR_Experience_Chaperone_DualCameraRig.Instance.TrackedCameraLeft != null)
                        {
                            ViveSR_Experience_Chaperone_DualCameraRig.Instance.TrackedCameraLeft.transform.localPosition = ViveSR_Experience_Chaperone_ImageCapture.GetHumanDectionLocalPosition();
                            ViveSR_Experience_Chaperone_DualCameraRig.Instance.TrackedCameraLeft.transform.localRotation = ViveSR_Experience_Chaperone_ImageCapture.GetHumanDectionLocalRotation();
                        }
                    }
                }
                #endregion
            }
        }

        private void OnDisable()
        {
            OverlayPtrInitial = false;
        }

        private RenderTexture renderTexture;
        private RenderTexture ForceShaderPass(Texture src, Material mat)
        {
            if (renderTexture == null || renderTexture.width != src.width || renderTexture.height != src.height)
            {
                Destroy(renderTexture);
                renderTexture = new RenderTexture(src.width, src.height, 0, RenderTextureFormat.ARGB32);
            }
            renderTexture.DiscardContents();
            Graphics.Blit(src, renderTexture, mat);
            return renderTexture;
        }
    }
}