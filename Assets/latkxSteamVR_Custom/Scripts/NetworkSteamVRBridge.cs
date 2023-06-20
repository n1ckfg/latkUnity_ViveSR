using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSteamVRBridge : MonoBehaviour {

    public SteamVR_NewController mainCtl;
    public SteamVR_NewController altCtl;
    public ContourOscToLatk osc;

    private void Start() {
		
	}
	
	private void Update() {
        if (mainCtl.menuPressed && altCtl.padDown || mainCtl.menuDown && altCtl.padPressed) {
            osc.armReceiver = true;
        } else if (mainCtl.menuPressed && altCtl.padUp || mainCtl.menuUp && altCtl.padPressed) {
            osc.armReceiver = false;
        }

        if (altCtl.triggerDown || altCtl.padDown) osc.clearList();
    }

}
