using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideGeneric : MonoBehaviour {

	public Transform root;
	public Transform pointer;
	public GameObject[] target;

	[HideInInspector] public bool isTracking = false;

	private float zPos = 0f;
	private bool armRotCorrect = true;

	void Start() {
		zPos = root.position.z;

		for (int i = 0; i < target.Length; i++) {
			target[i].SetActive (false);
		}
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.C)) {
			showColor();
		} else if (Input.GetKeyUp(KeyCode.C)) {
			hideColor();
		}
	}

	public void showColor() {
		isTracking = true;

		if (armRotCorrect) {
			Ray ray = new Ray(pointer.position, -pointer.forward);
			root.position = ray.GetPoint(zPos);
			root.LookAt(pointer);
			armRotCorrect = false;
		}

		for (int i = 0; i < target.Length; i++) {
			target[i].SetActive(true);
		}
	}

	public void hideColor() {
		isTracking = false;

		for (int i = 0; i < target.Length; i++) {
			target[i].SetActive (false);
		}

		armRotCorrect = true;
	}

}
