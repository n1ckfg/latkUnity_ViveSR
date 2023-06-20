using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Wacki;
using System;
using Vive.Plugin.SR.Experience;

public class PlayerHandUILaserPointer : IUILaserPointer {

    public EVRButtonId button = EVRButtonId.k_EButton_SteamVR_Trigger;

    Hand hand;

    private bool _connected = false;

    MeshRenderer pointer_rnd, hitPoint_rnd;

    Color BrightColor, OriginalColor;

    public static PlayerHandUILaserPointer LaserPointer;

    float soundPlayedTime;

    bool isTriggerDown;

    public static void CreateLaserPointer()
    {
        LaserPointer = new GameObject("LaserPointer").AddComponent<PlayerHandUILaserPointer>();
        LaserPointer.transform.SetParent(ViveSR_Experience.instance.AttachPoint.transform);
        LaserPointer.transform.localPosition = Vector3.zero;
        LaserPointer.transform.localEulerAngles = new Vector3(-60f, 0f, 0f);
        LaserPointer.color = Color.white;
        LaserPointer.button = EVRButtonId.k_EButton_Axis1;
    }

    public static void EnableLaserPointer(bool isOn)
    {
        if(LaserPointer != null) LaserPointer.gameObject.SetActive(isOn);
    }

    protected override void Initialize()
    {
        base.Initialize();

        BrightColor = new Color(1f, 0.921f, 0.7f, 1f);
        OriginalColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        ViveSR_Experience.instance.CheckHandStatus(() =>
        {
            hand = ViveSR_Experience.instance.targetHandScript;
            _connected = true;
        });
    }

    public override bool ButtonDown()
    {     
        if(!_connected)
            return false;

        if (hand.controller != null && hand.controller.GetPressDown(button))
        {
            isTriggerDown = true;
            return true;
        }

        return false;
    }

    public override bool ButtonUp()
    {
        if(!_connected)
            return false;

        if (hand.controller != null && hand.controller.GetPressUp(button))
        {
            isTriggerDown = false;
            return hand.controller.GetPressUp(button);
        }

        return false;
    }
        
    public override void OnEnterControl(GameObject control)
    {
        if (!_connected)
            return;

        if (control.name.Contains("Panel")) return;

        if (pointer_rnd == null) pointer_rnd = pointer.GetComponent<MeshRenderer>();
        if (hitPoint_rnd == null) hitPoint_rnd = hitPoint.GetComponent<MeshRenderer>();
        pointer_rnd.material.SetColor("_Color", BrightColor);
        hitPoint_rnd.material.SetColor("_Color", BrightColor);

        if (Time.timeSinceLevelLoad - soundPlayedTime > 0.1f && !isTriggerDown)
        {
            soundPlayedTime = Time.timeSinceLevelLoad;
            ViveSR_Experience.instance.SoundManager.PlayAtAttachPoint(AudioClipIndex.Click);
        }
            // hand.controller.TriggerHapticPulse(1000);

    }

    public override void OnExitControl(GameObject control)
    {
        if (!_connected)
            return;

        if (control.name.Contains("Panel")) return;

        pointer_rnd.material.SetColor("_Color", OriginalColor);
        hitPoint_rnd.material.SetColor("_Color", OriginalColor);
        //  hand.controller.TriggerHapticPulse(600);
    }

    int controllerIndex
    {
        get {
            if (!_connected) return 0;
            return Array.IndexOf(Player.instance.hands, hand) + 2;
        }
    }
}