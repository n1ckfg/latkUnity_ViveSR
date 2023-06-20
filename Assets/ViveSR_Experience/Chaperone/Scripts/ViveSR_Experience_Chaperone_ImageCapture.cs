//========= Copyright 2017, HTC Corporation. All rights reserved. ===========

using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Vive.Plugin.SR.Experience.Chaperone
{
    public class ViveSR_Experience_Chaperone_ImageCapture
    {
        private static IntPtr[] PtrHumanDection;
        private static DataInfo[] DataInfoHumanDection = new DataInfo[6];
        private static bool InitialHumanDectionPtrSize = false;

        private static int[] RawHumanDectionFrameIndex = new int[1];
        private static int[] RawHumanDectionTimeIndex = new int[1];
        private static float[] RawHumanDectionPose = new float[16];
        public static Matrix4x4 HumanDectionPose;
        private static IntPtr PtrHumanDectionLeft;
        private static IntPtr PtrHumanDectionMask;
        private static IntPtr PtrHumanDectionDepth;
        private static Texture2D TextureHumanDectionLeft;
        private static Texture2D TextureHumanDectionMask;
        private static Texture2D TextureHumanDectionDepth;
        public static int HumanDectionImageWidth = 0, HumanDectionImageHeight = 0, HumanDectionImageChannel = 0, HumanDectionMaskDataSize = 1;
        private static int LastHumanDectionFrameIndex = -1;

        public static int HumanDectionFrameIndex { get { return RawHumanDectionFrameIndex[0]; } }
        public static int HumanDectionTimeIndex { get { return RawHumanDectionTimeIndex[0]; } }
        public static bool HumanDectionProcessing { get; private set; }

        /// <summary>
        /// Initialize the image capturing tool.
        /// </summary>
        /// <returns></returns>
        public static int Initial()
        {
            GetParameters();
            TextureHumanDectionLeft = new Texture2D(HumanDectionImageWidth, HumanDectionImageHeight, TextureFormat.RGBA32, false);
            TextureHumanDectionMask = new Texture2D(HumanDectionImageWidth, HumanDectionImageHeight, TextureFormat.Alpha8, false);
            TextureHumanDectionDepth = new Texture2D(HumanDectionImageWidth, HumanDectionImageHeight, TextureFormat.RFloat, false);

            PtrHumanDection = new IntPtr[] {
                Marshal.AllocCoTaskMem(HumanDectionImageWidth * HumanDectionImageHeight * 4 ),
                Marshal.AllocCoTaskMem(HumanDectionImageWidth * HumanDectionImageHeight * 1 ),
                Marshal.AllocCoTaskMem(HumanDectionImageWidth * HumanDectionImageHeight * 1* sizeof(float) ),
                Marshal.AllocCoTaskMem(sizeof(int)),
                Marshal.AllocCoTaskMem(sizeof(int)),
                Marshal.AllocCoTaskMem(sizeof(float) * 16),};

            DataInfoHumanDection[0].mask = (int)HumanDetectionDataMask.LEFT_FRAME;
            DataInfoHumanDection[1].mask = (int)HumanDetectionDataMask.MASK_MAP;
            DataInfoHumanDection[2].mask = (int)HumanDetectionDataMask.DEPTH_MAP;
            DataInfoHumanDection[3].mask = (int)HumanDetectionDataMask.FRAME_SEQ;
            DataInfoHumanDection[4].mask = (int)HumanDetectionDataMask.TIME_STP;
            DataInfoHumanDection[5].mask = (int)HumanDetectionDataMask.POSE;
            for (int i = 0; i < DataInfoHumanDection.Length; i++) DataInfoHumanDection[i].ptr = PtrHumanDection[i];

            return (int)Error.WORK;
        }


        private static void GetParameters()
        {
            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)HumanDetectionParam.OUTPUT_WIDTH, ref HumanDectionImageWidth);
            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)HumanDetectionParam.OUTPUT_HEIGHT, ref HumanDectionImageHeight);
            ViveSR_Framework.GetParameterInt(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)HumanDetectionParam.OUTPUT_CHANNEL_1, ref HumanDectionImageChannel);
        }

        #region GetTexture2D
        /// <summary>
        /// Get the HumanDection texture, frame index, time index from current buffer.
        /// </summary>
        /// <param name="imageLeft"></param>
        /// <param name="imageMask"></param>
        /// <param name="frameIndex"></param>
        /// <param name="timeIndex"></param>
        public static void GetHumanDectionTexture(out Texture2D imageLeft, out Texture2D imageMask, out Texture2D imageDepth, out int frameIndex, out int timeIndex)
        {
            if (PtrHumanDection[(int)HumanDetectionDataMask.LEFT_FRAME] != IntPtr.Zero)
            {
                TextureHumanDectionLeft.LoadRawTextureData(PtrHumanDection[(int)HumanDetectionDataMask.LEFT_FRAME], HumanDectionImageWidth * HumanDectionImageHeight * 4);
                TextureHumanDectionLeft.Apply();
            }
            if (PtrHumanDection[(int)HumanDetectionDataMask.MASK_MAP] != IntPtr.Zero)
            {
                TextureHumanDectionMask.LoadRawTextureData(PtrHumanDection[(int)HumanDetectionDataMask.MASK_MAP], HumanDectionImageWidth * HumanDectionImageHeight * 1);
                TextureHumanDectionMask.Apply();
            }
            if (PtrHumanDection[(int)HumanDetectionDataMask.DEPTH_MAP] != IntPtr.Zero)
            {
                TextureHumanDectionDepth.LoadRawTextureData(PtrHumanDection[(int)HumanDetectionDataMask.DEPTH_MAP], HumanDectionImageWidth * HumanDectionImageHeight * sizeof(float));
                TextureHumanDectionDepth.Apply();
            }
            imageLeft = TextureHumanDectionLeft;
            imageMask = TextureHumanDectionMask;
            imageDepth = TextureHumanDectionDepth;
            frameIndex = HumanDectionFrameIndex;
            timeIndex = HumanDectionTimeIndex;
        }
        #endregion

        #region GetPosture
        public static Vector3 GetHumanDectionLocalPosition()
        {
            return new Vector3(HumanDectionPose.m03, HumanDectionPose.m13, HumanDectionPose.m23);
        }
        public static Quaternion GetHumanDectionLocalRotation()
        {
            return Quaternion.LookRotation(HumanDectionPose.GetColumn(2), HumanDectionPose.GetColumn(1));
        }
        #endregion

        #region Active
        public static void UpdateHumanDectionImage()
        {
            int result = (int)Error.FAILED;
            if (!InitialHumanDectionPtrSize)
            {
                result = ViveSR_Framework.GetMultiDataSize(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, DataInfoHumanDection, DataInfoHumanDection.Length);
                InitialHumanDectionPtrSize = (result == (int)Error.WORK);
            }

            DataInfo[] dataInfoFrame = new DataInfo[] { DataInfoHumanDection[(int)HumanDetectionDataMask.FRAME_SEQ] };
            result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, dataInfoFrame, dataInfoFrame.Length);
            if (result != (int)Error.WORK) return;

            Marshal.Copy(DataInfoHumanDection[(int)HumanDetectionDataMask.FRAME_SEQ].ptr, RawHumanDectionFrameIndex, 0, RawHumanDectionFrameIndex.Length);
            if (LastHumanDectionFrameIndex == HumanDectionFrameIndex) return;
            else LastHumanDectionFrameIndex = HumanDectionFrameIndex;

            result = ViveSR_Framework.GetMultiData(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, DataInfoHumanDection, DataInfoHumanDection.Length);
            if (result == (int)Error.WORK) ParseHumanDectionPtrData();
        }
        #endregion

        private static void ParseHumanDectionPtrData()
        {
            Marshal.Copy(PtrHumanDection[(int)HumanDetectionDataMask.FRAME_SEQ], RawHumanDectionFrameIndex, 0, RawHumanDectionFrameIndex.Length);
            Marshal.Copy(PtrHumanDection[(int)HumanDetectionDataMask.TIME_STP], RawHumanDectionTimeIndex, 0, RawHumanDectionTimeIndex.Length);
            Marshal.Copy(PtrHumanDection[(int)HumanDetectionDataMask.POSE], RawHumanDectionPose, 0, RawHumanDectionPose.Length);

            for (int i = 0; i < 4; i++)
            {
                HumanDectionPose.SetColumn(i, new Vector4(RawHumanDectionPose[i * 4 + 0], RawHumanDectionPose[i * 4 + 1],
                                                          RawHumanDectionPose[i * 4 + 2], RawHumanDectionPose[i * 4 + 3]));
            }
        }

        public static bool HasDepthMask { get; private set; }
        public static int EnableDepthMask(bool active)
        {
            int result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)HumanDetectionParam.ENABLE_DEPTH_MASK, active);
            if (result == (int)Error.WORK) HasDepthMask = active;
            return result;
        }

        public static bool HasPlayAreaMask { get; private set; }
        public static int EnablePlayAreaMask(bool active)
        {
            int result = ViveSR_Framework.SetParameterBool(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)HumanDetectionParam.ENABLE_PLAY_AREA_MASK, active);
            if (result == (int)Error.WORK)
            {
                HasPlayAreaMask = active;
                if (active)
                {
                    float[] playArea = new float[12];
                    ViveSR_Framework.GetParameterFloatArray(ViveSR_Framework.MODULE_ID_SEETHROUGH, (int)SeeThroughParam.PLAY_AREA_RECT, ref playArea);
                    ViveSR_Framework.SetParameterFloatArray(ViveSR_Framework.MODULE_ID_HUMAN_DETECTION, (int)HumanDetectionParam.PLAY_AREA_RECT, playArea);
                }
            }
            return result;
        }
    }
}