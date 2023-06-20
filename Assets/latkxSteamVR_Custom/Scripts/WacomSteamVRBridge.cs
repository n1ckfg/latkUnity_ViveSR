using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WacomSteamVRBridge : MonoBehaviour {

    public WacomOffset wacom;
    public SteamVR_NewController mainCtl;

	private void Update() {
        if (wacom.isActive) {
            mainCtl.triggerDown = wacom.triggerDown;
            mainCtl.triggerPressed = wacom.triggerPressed;
            mainCtl.triggerUp = wacom.triggerUp;

            mainCtl.menuDown = wacom.menuDown;
            mainCtl.menuPressed = wacom.menuPressed;
            mainCtl.menuUp = wacom.menuUp;
        }
    }

}
