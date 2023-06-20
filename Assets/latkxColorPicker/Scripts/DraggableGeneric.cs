// https://stackoverflow.com/questions/39437044/how-to-pick-color-from-raycast-hit-point-in-unity
// https://www.youtube.com/watch?v=wysIsMEQ3_Y

using UnityEngine;

public class DraggableGeneric : MonoBehaviour {

    public enum PickMode { CAMERA, MOUSE };
    public PickMode pickMode = PickMode.CAMERA;
	public ShowHideGeneric showHideGeneric;
	public Transform minBound; 
	public bool fixX = false;
	public bool fixY = false;
	public bool fixZ = false;
	public Transform thumb;	

	private bool dragging;
    private Ray ray;
    private Vector3 rayPos, rayDir;

    void FixedUpdate() {
        if (pickMode == PickMode.CAMERA) {
            rayPos = Camera.main.transform.position;
            rayDir = Camera.main.transform.forward;
        }

		if (showHideGeneric.isTracking) {//(sixCtl.menuDown) {
			dragging = false;
            if (pickMode == PickMode.CAMERA) {
                ray = new Ray(rayPos, rayDir);
            } else if (pickMode == PickMode.MOUSE) {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }

			RaycastHit hit;

			if (GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity)) {
				dragging = true;
			}
		}

		if (!showHideGeneric.isTracking) dragging = false;

		if (dragging && showHideGeneric.isTracking) {
            if (pickMode == PickMode.CAMERA) {
                ray = new Ray(rayPos, rayDir);
            } else {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }

            RaycastHit hit;

			if (GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity)) {
				var point = hit.point;
				SetThumbPosition(point); 
				Vector3 message = Vector3.one - (thumb.localPosition - minBound.localPosition) / GetComponent<BoxCollider>().size.x;

				SendMessage("OnDrag", message);
			}
		}
	}

	void SetThumbPosition(Vector3 point) {
		Vector3 temp = thumb.localPosition;
		thumb.position = point;
		thumb.localPosition = new Vector3(fixX ? temp.x : thumb.localPosition.x, fixY ? temp.y : thumb.localPosition.y, thumb.localPosition.z-1);
	}

}
