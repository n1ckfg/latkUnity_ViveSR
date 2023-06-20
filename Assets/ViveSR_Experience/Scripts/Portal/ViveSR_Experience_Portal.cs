using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Valve.VR.InteractionSystem;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Portal : MonoBehaviour
    {
        [SerializeField] GameObject PortalPrefab;
        ViveSR_Portal Portal;
        [SerializeField] GameObject HeadCollision;
        public ViveSR_PortalMgr PortalManager;
        Vector3 OldPosition;
        public GameObject dartGenerator;

        List<ViveSR_PortalTraveller> HandTravellers = new List<ViveSR_PortalTraveller>();

        [SerializeField] protected GameObject VR_BG, VR_BG_Cutout;
        [SerializeField] List<Material> BGObjsMats = new List<Material>();
        WorldMode oldMode = WorldMode.RealWorld;

        bool isPortalOn;

        private void Awake()
        {
            foreach (Hand hand in Player.instance.hands)
            {
                if (hand.name != "FallbackHand")
                    HandTravellers.Add(hand.GetComponent<ViveSR_PortalTraveller>());
            }
            //Get objects ready to be used as VRBackground
            if(VR_BG != null) SetVRBGMaterials(VR_BG, false);
            if(VR_BG_Cutout != null) SetVRBGMaterials(VR_BG_Cutout, true);
        }

        public void SetPortal(bool isOn)
        {
            isPortalOn = isOn;
            if (isOn)
            {
                InitPortal();
                PortalManager.TurnOnCamera();

            }
            else
            {
                PortalManager.viewerInWorld = WorldMode.RealWorld;
                PortalManager.UpdateViewerWorld();
                MatchControllerWorld();

                Destroy(Portal.gameObject);
                PortalManager.ClearAllPortals();
                PortalManager.TurnOffCamera();
            }
            dartGenerator.SetActive(isOn);
            PortalManager.gameObject.SetActive(isOn);
        } 

        void SetVRBGMaterials(GameObject Obj, bool Cutout)
        {
            for (int i = 0; i < Obj.transform.childCount; i++)
            {
                Renderer renderer = Obj.transform.GetChild(i).gameObject.GetComponent<Renderer>();
                if (renderer)
                {
                    BGObjsMats.Add(renderer.material);
                    renderer.material.shader = Shader.Find(Cutout ? "ViveSR/Standard, AlphaTest, Stencil" : "ViveSR/Standard, Stencil");
                    if (Cutout) renderer.material.SetFloat("_CullMode", (float)UnityEngine.Rendering.CullMode.Off);
                    renderer.material.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
                    renderer.material.SetFloat("_ZTestComp", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                }

                if (Obj.transform.GetChild(i).transform.childCount > 0)
                {
                    SetVRBGMaterials(Obj.transform.GetChild(i).gameObject, Cutout);
                }
            }
        }                    

        public void UpdateBGStencil()
        {
            foreach (Material mat in BGObjsMats)
            {
                mat.SetFloat("_StencilComp", PortalManager.viewerInWorld == WorldMode.RealWorld ?
                     (float)UnityEngine.Rendering.CompareFunction.Equal :
                     (float)UnityEngine.Rendering.CompareFunction.Always);
            }
        }

        void InitPortal()
        {
            GameObject go = Instantiate(PortalPrefab);
            go.transform.position = ViveSR_Experience.instance.PlayerHeadCollision.transform.position + ViveSR_Experience.instance.PlayerHeadCollision.transform.forward * 0.5f;
            go.transform.forward = -ViveSR_Experience.instance.PlayerHeadCollision.transform.forward;
            PortalManager.AddPortal(go);
            Portal = go.GetComponent<ViveSR_Portal>();

            OldPosition = Portal.transform.position;

            StartCoroutine(CheckViewerInWorld());
            StartCoroutine(CheckPortalPosition());
        }

        public void ResetPortalPosition()
        {
            Portal.transform.position = HeadCollision.transform.position + HeadCollision.transform.forward * 0.5f;
            Portal.transform.forward = (PortalManager.viewerInWorld == WorldMode.RealWorld ? -1 : 1) * HeadCollision.transform.forward;
        }

        IEnumerator CheckPortalPosition()
        {
            while (isPortalOn)
            {
                if (Portal.transform.position != OldPosition)
                {
                    Portal.UpdatePlaneNormal();
                    OldPosition = Portal.transform.position;
                }
                yield return new WaitForEndOfFrame();
            }
        }
                             
        IEnumerator CheckViewerInWorld()
        {
            while (isPortalOn)
            {
                if (PortalManager.viewerInWorld != oldMode)
                {
                    MatchControllerWorld();
                    UpdateBGStencil();
                    oldMode = PortalManager.viewerInWorld;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public void MatchControllerWorld()
        {
            foreach (ViveSR_PortalTraveller handTraveller in HandTravellers)
            {
                handTraveller.IsTransitioning = false;
                PortalManager.controllerInWorld = handTraveller.CurrentWorld = PortalManager.viewerInWorld;
                if (handTraveller.Renderers == null) handTraveller.Renderers = handTraveller.gameObject.GetComponentsInChildren<MeshRenderer>(true);
                handTraveller.SwitchMaterials(handTraveller.Renderers, handTraveller.CurrentWorld);
                handTraveller.SetClippingPlaneEnable(handTraveller.Renderers, false, handTraveller.CurrentWorld);
            }
        }

        private void OnDestroy()
        {
            foreach (Material mat in BGObjsMats)
            {
                mat.shader = Shader.Find("Standard");
            }
        }
    }
}