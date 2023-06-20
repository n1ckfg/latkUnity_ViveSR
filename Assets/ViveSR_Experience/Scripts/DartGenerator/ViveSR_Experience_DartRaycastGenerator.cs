using UnityEngine;
namespace Vive.Plugin.SR.Experience
{
    [RequireComponent(typeof(LineRenderer))]
    public class ViveSR_Experience_DartRaycastGenerator : ViveSR_Experience_IDartGenerator
    {
        RaycastHit hitInfo;
        LineRenderer lineRenderer;

        protected override void AwakeToDo()
        {
            lineRenderer = GetComponent<LineRenderer>();                                    
        }

        protected override void OnDisableToDo()
        {
            lineRenderer.enabled = false;
        }

        public override void TriggerPress()
        {
            base.TriggerPress();
            lineRenderer.enabled = true;
            GenerateDart();
            InstantiatedDarts.Add(currentGameObj);

            isHolding = true;
        }

        protected override void TriggerHold()
        {               
            Vector3 fwd = transform.forward;
            Physics.Raycast(transform.position, fwd, out hitInfo);
            lineRenderer.SetPosition(0, transform.position);
            if (hitInfo.rigidbody != null)
            {
                lineRenderer.endColor = Color.green;
                currentGameObj.transform.position = hitInfo.point;
                currentGameObj.transform.up = hitInfo.normal;
                lineRenderer.SetPosition(1, hitInfo.point);
            }
            else
            {   
                lineRenderer.endColor = Color.red;
                lineRenderer.SetPosition(1, fwd * 0.5f + transform.position);
            }
        }

        public override void TriggerRelease()
        {
            base.TriggerRelease();
            lineRenderer.endColor = Color.white;
            lineRenderer.enabled = false;
            if (hitInfo.rigidbody == null)  Destroy(currentGameObj);
            
            ViveSR_Experience.instance.targetHandScript.DetachObject(currentGameObj);
            currentGameObj.transform.parent = null;
            isHolding = false;
        }

        protected override void GenerateDart()
        {
            currentGameObj = Instantiate(dart_prefabs[currentDartPrefeb]);
            currentGameObj.transform.eulerAngles = Vector3.zero;
            currentGameObj.GetComponent<ViveSR_Experience_Dart>().dartGeneratorMgr = dartGeneratorMgr;

            if (currentGameObj.name.Contains("viveDeer"))
            {
                currentGameObj.GetComponent<Renderer>().material = deerMgr.deerMaterials[Random.Range(0, deerMgr.deerMaterials.Count - 1)];
                int scale = Random.Range(0, deerMgr.deerScale.Count);
                currentGameObj.transform.localScale = new Vector3(deerMgr.deerScale[scale], deerMgr.deerScale[scale], deerMgr.deerScale[scale]);
            }
        }
    }
}