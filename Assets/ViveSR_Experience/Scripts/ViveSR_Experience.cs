using UnityEngine;
using System.Collections.Generic;
using Valve.VR.InteractionSystem;
using System.Collections;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience : MonoBehaviour
    {
        private static ViveSR_Experience _instance;
        public static ViveSR_Experience instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ViveSR_Experience>();
                }
                return _instance;
            }
        }

        public GameObject targetHand { get; private set; }
        public Hand targetHandScript { get; private set; }

        bool showControllerModel = true;

        public GameObject PlayerHeadCollision;

        public List<Renderer> controllerRenderers = new List<Renderer>();
        GameObject ControllerObjGroup;

        public GameObject AttachPoint;

        public ViveSR_Experience_SoundManager SoundManager { get; private set; }

        private void Awake()
        {
            Player.instance.allowToggleTo2D = false;
            CheckFrameStatus(()=>StartCoroutine(DetectHand()));
            SoundManager = FindObjectOfType<ViveSR_Experience_SoundManager>();

        }

        //show controller model------
        IEnumerator CollectControllerParts()
        {
            while (controllerRenderers.Count == 0)
            {
                for (int i = 0; i < ControllerObjGroup.transform.childCount; i++)
                {
                    Renderer targetRenderer = ControllerObjGroup.transform.GetChild(i).GetComponent<Renderer>();
                    if (targetRenderer != null) controllerRenderers.Add(targetRenderer);
                }
                yield return new WaitForEndOfFrame();
            }
            SetControllerRenderer(false);
        }

        public bool ShowControllerModel()
        {
            return showControllerModel;
        }

        public void SetControllerRenderer(bool On)
        {
            foreach (Renderer rndr in controllerRenderers)
            {
                rndr.enabled = On;
            }
        }
        //show controller model end---

        public void CheckFrameStatus(System.Action done)
        {
            StartCoroutine(_CheckFrameStatus(done));
        }
        IEnumerator _CheckFrameStatus(System.Action done)
        {
            while (ViveSR_DualCameraRig.DualCameraStatus != DualCameraStatus.WORKING)
            {
                yield return new WaitForEndOfFrame();
            }

            done();
        }

        public void CheckHandStatus(System.Action done)
        {
            StartCoroutine(_CheckHandStatus(done));
        }
        IEnumerator _CheckHandStatus(System.Action done)
        {
            while (targetHand == null)
            {
                yield return new WaitForEndOfFrame();
            }

            done();
        }

        IEnumerator DetectHand()
        {
            while (targetHand == null)
            {    
                if (Player.instance.GetHand(0).AttachedObjects.Count > 0) targetHand = Player.instance.GetHand(0).gameObject;
                else if (Player.instance.GetHand(1).AttachedObjects.Count > 0) targetHand = Player.instance.GetHand(1).gameObject;

                if (targetHand != null)
                {
                    targetHandScript = targetHand.GetComponent<Hand>();

                    //prevent controller disappearing when holding gameobjs
                    HideOnHandFocusLost[] handHiders;
                    handHiders = FindObjectsOfType<HideOnHandFocusLost>();
                    foreach (HideOnHandFocusLost handHider in handHiders) Destroy(handHider);

                    //Move playerHeadCollision to follow the headset.
                    GameObject PlayerHead = GameObject.Find("Camera (eye)").gameObject;
                    PlayerHeadCollision.transform.parent = PlayerHead.transform;
                    PlayerHeadCollision.transform.localPosition = Vector3.zero;
                    PlayerHeadCollision.transform.localEulerAngles = Vector3.zero;

                    //Attachpoint controlls the positioning of the UI.
                    AttachPoint.transform.parent = targetHand.transform.Find("Attach_ControllerTip").transform;
                    AttachPoint.transform.localPosition = new Vector3(0f, 0.015f, 0.02f);
                    AttachPoint.transform.localEulerAngles = new Vector3(60f, 0f, 0f);

                    Destroy(targetHand.transform.Find("ControllerHoverHighlight").gameObject);    //don't allow highlight from steamVR
                    Destroy(GameObject.Find("HeadCollider").gameObject);

                    //Get controller parts to hide the virtual model
                    if (!showControllerModel)
                    {
                        ControllerObjGroup = GameObject.Find("SteamVR_RenderModel").gameObject;
                        StartCoroutine(CollectControllerParts());
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }       

        public void WaitForOneFrame(System.Action done)
        {
            StartCoroutine(_WaitForOneFrame(done));
        }

        IEnumerator _WaitForOneFrame(System.Action done)
        {
            yield return new WaitForEndOfFrame();
            done();
        }

        public void WaitForSeconds(float secs, System.Action done)
        {
            StartCoroutine(_WaitForSeconds(secs, done));
        }

        IEnumerator _WaitForSeconds(float secs, System.Action done)
        {
            yield return new WaitForSeconds(secs);
            done();
        }
    }
}