using System;
using UnityEngine;
using Valve.VR;

namespace Vive.Plugin.SR
{
    public class ViveSR_Experience_Overlay : MonoBehaviour
    {
        public enum Features
        {
            TexturePtr               = 0X01 << 0,
            Texture                  = 0X01 << 1,
            TextureBounds            = 0X01 << 2,
            Color                    = 0X01 << 3,
            Alpha                    = 0X01 << 4,
            Width                    = 0X01 << 5,
            Transform                = 0X01 << 6,
            AutoCurveDistanceRange   = 0X01 << 7,
            Flag                     = 0X01 << 8,
            MouseScale               = 0X01 << 9,
            InputMethod              = 0X01 << 10,
        };
        public enum AnchorType
        {
            HMD,
            ORIGIN,
        }

        private string OverlayKey = "";
        private ulong OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        private GameObject OriginPoint;

        public Texture _Texture = null;
        private Texture_t _Texture_t;
        public Vector4 _TextureBounds = new Vector4(0, 0, 1, 1);  // Need rewrite
        public Color _Color = Color.white;
        public float _Alpha = 1;
        public float _Width = 1;   // In meters
        public AnchorType _Anchor;
        public Vector2 _CurveDistanceRange = new Vector2(1, 1);
        public Vector2 _MouseScale = Vector3.one;
        public VROverlayInputMethod _InputMethod = VROverlayInputMethod.None;

        private void OnEnable()
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null) goto Fail;
            EVROverlayError error = overlay.CreateOverlay(OverlayKey + gameObject.GetInstanceID(), gameObject.name, ref OverlayHandle);
            if (error != EVROverlayError.None) goto Fail;

            ETextureType textureType;
            switch (SystemInfo.graphicsDeviceType)
            {
#if (UNITY_5_4)
			case UnityEngine.Rendering.GraphicsDeviceType.OpenGL2:
#endif
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                    textureType = ETextureType.OpenGL;
                    break;
#if !(UNITY_5_4)
                case UnityEngine.Rendering.GraphicsDeviceType.Vulkan:
                    textureType = ETextureType.Vulkan;
                    break;
#endif
                default:
                    textureType = ETextureType.DirectX;
                    break;
            }

            _Texture_t = new Texture_t
            {
                handle = _Texture.GetNativeTexturePtr(),
                eType = textureType,
                eColorSpace = EColorSpace.Auto
            };
            OriginPoint = new GameObject("Origin Point") { hideFlags = HideFlags.HideInHierarchy };

            UpdateOverlay(Features.TexturePtr);
            UpdateOverlay((int)Features.Texture | (int)Features.TextureBounds | (int)Features.Width | (int)Features.Transform);
            return;

            Fail:
            enabled = false;
        }

        private void OnDisable()
        {
            if (OverlayHandle == OpenVR.k_ulOverlayHandleInvalid) return;
            var overlay = OpenVR.Overlay;
            if (overlay != null) overlay.DestroyOverlay(OverlayHandle);
            OverlayKey = "";
            Destroy(OriginPoint);
            OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        }

        private void Update()
        {
            if (AutoUpdate)
            {
                foreach (Features feature in Enum.GetValues(typeof(Features)))
                {
                    if (CheckOverlayChanged(feature))
                    {
                        UpdateOverlay(feature);
                    }
                }
            }
        }

        public void UpdateOverlay(Features feature)
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null) return;

            switch (feature)
            {
                case Features.TexturePtr:
                    _Texture_t.handle = _Texture.GetNativeTexturePtr();
                    break;
                case Features.Texture:
                    overlay.SetOverlayTexture(OverlayHandle, ref _Texture_t);
                    break;
                case Features.Flag:
                    break;
                case Features.Color:
                    overlay.SetOverlayColor(OverlayHandle, _Color.r, _Color.g, _Color.b);
                    break;
                case Features.Alpha:
                    overlay.SetOverlayAlpha(OverlayHandle, _Alpha);
                    break;
                case Features.Width:
                    overlay.SetOverlayWidthInMeters(OverlayHandle, _Width);
                    break;
                case Features.AutoCurveDistanceRange:
                    overlay.SetOverlayAutoCurveDistanceRangeInMeters(OverlayHandle, _CurveDistanceRange.x, _CurveDistanceRange.y);
                    break;
                case Features.TextureBounds:
                    var textureBounds = new VRTextureBounds_t
                    {
                        uMin = (0 + _TextureBounds.x) * _TextureBounds.z,
                        vMin = (1 + _TextureBounds.y) * _TextureBounds.w,
                        uMax = (1 + _TextureBounds.x) * _TextureBounds.z,
                        vMax = (0 + _TextureBounds.y) * _TextureBounds.w
                    };
                    overlay.SetOverlayTextureBounds(OverlayHandle, ref textureBounds);
                    break;
                case Features.Transform:
                    Transform transRelateTo = OriginPoint.transform;
                    var rt = new SteamVR_Utils.RigidTransform(transRelateTo, transform);
                    rt.pos.x /= transRelateTo.localScale.x;
                    rt.pos.y /= transRelateTo.localScale.y;
                    rt.pos.z /= transRelateTo.localScale.z;
                    var matrix = rt.ToHmdMatrix34();
                    switch (_Anchor)
                    {
                        case AnchorType.HMD:
                            overlay.SetOverlayTransformTrackedDeviceRelative(OverlayHandle, 0, ref matrix);
                            break;
                        case AnchorType.ORIGIN:
                            overlay.SetOverlayTransformAbsolute(OverlayHandle, SteamVR_Render.instance.trackingSpace, ref matrix);
                            break;
                    }
                    break;
                case Features.MouseScale:
                    var vec2MouseScale = new HmdVector2_t
                    {
                        v0 = _MouseScale.x,
                        v1 = _MouseScale.y
                    };
                    overlay.SetOverlayMouseScale(OverlayHandle, ref vec2MouseScale);
                    break;
                case Features.InputMethod:
                    overlay.SetOverlayInputMethod(OverlayHandle, _InputMethod);
                    break;
            }
        }

        public void UpdateOverlay(int mask)
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null) return;
            if (_Texture == null)
            {
                overlay.HideOverlay(OverlayHandle);
                return;
            }
            var error = overlay.ShowOverlay(OverlayHandle);
            if (error == EVROverlayError.InvalidHandle || error == EVROverlayError.UnknownOverlay)
            {
                if (overlay.FindOverlay(OverlayKey, ref OverlayHandle) != EVROverlayError.None) return;
            }

            foreach (Features feature in Enum.GetValues(typeof(Features)))
            {
                if ((mask & (int)feature) > 0)
                {
                    UpdateOverlay(feature);
                }
            }
        }

        #region Auto Update
        [Tooltip("It will hurt the performance if enable.")]
        public bool AutoUpdate = false;
        private Texture __Texture = null;
        private Vector4 __TextureBounds = new Vector4(0, 0, 1, 1);  // Need rewrite
        private Color __Color = Color.white;
        private float __Alpha = 1;
        private float __Width = 1;   // In meters
        private Vector3 __Position = Vector3.zero;
        private Quaternion __Rotation = Quaternion.identity;
        private AnchorType __Anchor;
        private Vector2 __CurveDistanceRange = new Vector2(1, 1);
        private Vector2 __MouseScale = Vector3.one;
        private VROverlayInputMethod __InputMethod = VROverlayInputMethod.None;

        private bool CheckOverlayChanged(Features feature)
        {
            switch (feature)
            {
                case Features.TexturePtr:
                    if (_Texture == null) return false;
                    else if (_Texture_t.handle == _Texture.GetNativeTexturePtr()) return false;
                    return true;
                case Features.Texture:
                    if (_Texture == __Texture) return false;
                    __Texture = _Texture;
                    return true;
                case Features.Flag:
                    return false;
                case Features.Color:
                    if (_Color == __Color) return false;
                    __Color = _Color;
                    return true;
                case Features.Alpha:
                    if (_Alpha == __Alpha) return false;
                    __Alpha = _Alpha;
                    return true;
                case Features.Width:
                    if (_Width == __Width) return false;
                    __Width = _Width;
                    return true;
                case Features.AutoCurveDistanceRange:
                    if (_CurveDistanceRange == __CurveDistanceRange) return false;
                    __CurveDistanceRange = _CurveDistanceRange;
                    return true;
                case Features.TextureBounds:
                    if (_TextureBounds == __TextureBounds) return false;
                    __TextureBounds = _TextureBounds;
                    return true;
                case Features.Transform:
                    if (_Anchor == __Anchor &&
                        transform.position == __Position && transform.rotation == __Rotation) return false;
                    __Anchor = _Anchor;
                    __Position = transform.position;
                    __Rotation = transform.rotation;
                    return true;
                case Features.MouseScale:
                    if (_MouseScale == __MouseScale) return false;
                    __MouseScale = _MouseScale;
                    return true;
                case Features.InputMethod:
                    if (_InputMethod == __InputMethod) return false;
                    __InputMethod = _InputMethod;
                    return true;
            }
            return false;
        }
        #endregion
    }
}