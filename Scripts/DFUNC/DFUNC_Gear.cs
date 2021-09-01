
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DFUNC_Gear : UdonSharpBehaviour
{
    [SerializeField] private SaccAirVehicle SAVControl;
    [Tooltip("Object enabled when function is active (used on MFD)")]
    [SerializeField] private GameObject Dial_Funcon;
    [SerializeField] private Animator GearAnimator;
    [Tooltip("Multiply drag by this amount while gear is down")]
    [SerializeField] private float LandingGearDragMulti = 1.3f;
    private SaccEntity EntityControl;
    private bool UseLeftTrigger = false;
    private bool Dial_FunconNULL = true;
    private bool TriggerLastFrame;
    [System.NonSerializedAttribute] public bool GearUp = false;
    private bool DragApplied = false;
    private bool DisableGroundDetector = false;
    [System.NonSerializedAttribute] public int DisableGearToggle = 0;
    private int GEARUP_STRING = Animator.StringToHash("gearup");
    private int INSTANTGEARDOWN_STRING = Animator.StringToHash("instantgeardown");
    public void DFUNC_LeftDial() { UseLeftTrigger = true; }
    public void DFUNC_RightDial() { UseLeftTrigger = false; }
    public void SFEXT_L_EntityStart()
    {
        EntityControl = SAVControl.EntityControl;
        LandingGearDragMulti -= 1;//to match how the old values worked
        SetGearDown();
        Dial_FunconNULL = Dial_Funcon == null;
        if (!Dial_FunconNULL) Dial_Funcon.SetActive(!GearUp);
    }
    public void DFUNC_Selected()
    {
        gameObject.SetActive(true);
    }
    public void DFUNC_Deselected()
    {
        gameObject.SetActive(false);
        TriggerLastFrame = false;
    }
    public void SFEXT_O_PilotEnter()
    {
        if (!Dial_FunconNULL) Dial_Funcon.SetActive(!GearUp);
    }
    public void SFEXT_O_PilotExit()
    {
        DFUNC_Deselected();
    }
    public void SFEXT_O_PassengerEnter()
    {
        if (!Dial_FunconNULL) Dial_Funcon.SetActive(!GearUp);
    }
    public void SFEXT_G_Explode()
    {
        SetGearDown();
    }
    public void SFEXT_G_RespawnButton()
    {
        SetGearDown();
        GearAnimator.SetTrigger(INSTANTGEARDOWN_STRING);
    }
    public void KeyboardInput()
    {
        if (DisableGearToggle == 0) { ToggleGear(); }
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
            if (!TriggerLastFrame && DisableGearToggle == 0) { ToggleGear(); }
            TriggerLastFrame = true;
        }
        else { TriggerLastFrame = false; }
    }
    public void SetGearUp()
    {
        //Debug.Log("SetGearUp");
        if (!DisableGroundDetector) { SAVControl.DisableGroundDetection += 1; DisableGroundDetector = true; }
        if (!Dial_FunconNULL) { Dial_Funcon.SetActive(false); }
        GearUp = true;
        GearAnimator.SetBool(GEARUP_STRING, true);
        if (DragApplied) { SAVControl.ExtraDrag -= LandingGearDragMulti; DragApplied = false; }

        if (SAVControl.IsOwner)
        {
            EntityControl.SendEventToExtensions("SFEXT_O_GearUp");
        }
    }
    public void SetGearDown()
    {
        //Debug.Log("SetGearDown");
        if (DisableGroundDetector) { SAVControl.DisableGroundDetection -= 1; DisableGroundDetector = false; }
        if (!Dial_FunconNULL) { Dial_Funcon.SetActive(true); }
        GearUp = false;
        GearAnimator.SetBool(GEARUP_STRING, false);
        if (!DragApplied) { SAVControl.ExtraDrag += LandingGearDragMulti; DragApplied = true; }

        if (SAVControl.IsOwner)
        {
            EntityControl.SendEventToExtensions("SFEXT_O_GearDown");
        }
    }
    public void ToggleGear()
    {
        if (!GearUp)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetGearUp");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetGearDown");
        }
    }
    public void SFEXT_O_PlayerJoined()
    {
        if (GearUp)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetGearUp");
        }
    }

}
