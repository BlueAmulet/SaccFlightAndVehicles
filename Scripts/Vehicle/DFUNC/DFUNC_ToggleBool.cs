﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DFUNC_ToggleBool : UdonSharpBehaviour
{
    [SerializeField] private Animator BoolAnimator;
    [SerializeField] private string AnimBoolName = "AnimBool";
    [SerializeField] private bool OnDefault = false;
    [SerializeField] private bool PilotExitTurnOff = true;
    [SerializeField] private float ToggleMinDelay;
    [SerializeField] private GameObject Dial_Funcon;
    [SerializeField] private bool OpensDoor = false;
    [Header("Door Only:")]
    [SerializeField] private SoundController SoundControl;
    [SerializeField] private float DoorCloseTime = 2;
    private bool Dial_FunconNULL = true;
    private bool AnimOn = false;
    private float ToggleTime;
    private bool UseLeftTrigger = false;
    private bool TriggerLastFrame;
    private int AnimBool_STRING;
    private bool sound_DoorOpen;
    public void DFUNC_LeftDial() { UseLeftTrigger = true; }
    public void DFUNC_RightDial() { UseLeftTrigger = false; }
    public void SFEXT_L_ECStart()
    {
        if (OpensDoor && (ToggleMinDelay < DoorCloseTime)) { ToggleMinDelay = DoorCloseTime; }
        AnimBool_STRING = Animator.StringToHash(AnimBoolName);
        Dial_FunconNULL = Dial_Funcon == null;
        if (OnDefault)
        {
            SetBoolOn();
        }
    }
    public void SFEXT_O_PlayerJoined()
    {
        if (OnDefault && !AnimOn)
        { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetBoolOff"); }
        else if (!OnDefault && AnimOn)
        { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetBoolOn"); }
    }
    public void DFUNC_Selected()
    {
        gameObject.SetActive(true);
    }
    public void DFUNC_Deselected()
    {
        gameObject.SetActive(false);
    }
    public void SFEXT_O_PilotExit()
    {
        if (PilotExitTurnOff)
        {
            if (!OnDefault && AnimOn)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetBoolOff"); }
            else if (OnDefault && !AnimOn)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetBoolOn"); }
        }
        gameObject.SetActive(false);
    }
    public void SFEXT_G_Explode()
    {
        if (OnDefault && !AnimOn)
        { SetBoolOn(); }
        else if (!OnDefault && AnimOn)
        { SetBoolOff(); }
    }
    public void KeyboardInput()
    {
        if (Time.time - ToggleTime > ToggleMinDelay)
        {
            if (AnimOn)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetBoolOff"); }
            else
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetBoolOn"); }
        }
    }
    private void Update()
    {
        float Trigger;
        if (UseLeftTrigger)
        { Trigger = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger"); }
        else
        { Trigger = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger"); }
        if (Trigger > 0.75)
        {
            if (!TriggerLastFrame)
            {
                if (Time.time - ToggleTime > ToggleMinDelay)
                {
                    if (AnimOn)
                    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetBoolOff"); }
                    else
                    { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetBoolOn"); }
                }
            }
            TriggerLastFrame = true;
        }
        else { TriggerLastFrame = false; }
    }

    public void SetBoolOn()
    {
        if (AnimOn) { return; }
        ToggleTime = Time.time;
        AnimOn = true;
        BoolAnimator.SetBool(AnimBool_STRING, AnimOn);
        if (!Dial_FunconNULL) { Dial_Funcon.SetActive(true); }
        if (OpensDoor)
        { SoundControl.DoorOpen(); }
    }
    public void SetBoolOff()
    {
        if (!AnimOn) { return; }
        ToggleTime = Time.time;
        AnimOn = false;
        BoolAnimator.SetBool(AnimBool_STRING, AnimOn);
        if (!Dial_FunconNULL) { Dial_Funcon.SetActive(false); }
        if (OpensDoor)
        { SoundControl.SendCustomEventDelayedSeconds("DoorClose", DoorCloseTime); }
    }
    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        if (PilotExitTurnOff && player.isLocal)
        {
            if (!OnDefault && AnimOn)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetBoolOff"); }
            else if (OnDefault && !AnimOn)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetBoolOn"); }
        }
    }
}