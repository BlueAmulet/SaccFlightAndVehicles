
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class SaccPassengerSeat : UdonSharpBehaviour
{
    public SaccEntity EntityControl;
    public GameObject SeatAdjuster;
    [Tooltip("Object that is enabled only for passenger that uses this seat. Not required.")]
    public GameObject PassengerOnly;
    private int ThisStationID;
    private bool SeatInitialized = false;
    private VRCPlayerApi localPlayer;
    private void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }
    private void Interact()
    {
        if (!SeatInitialized) { InitializeSeat(); }
        localPlayer.UseAttachedStation();

        EntityControl.MySeat = ThisStationID;
        if (PassengerOnly != null) { PassengerOnly.SetActive(true); }
        if (SeatAdjuster != null) { SeatAdjuster.SetActive(true); }

        EntityControl.PassengerEnterVehicleLocal();
    }
    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (!SeatInitialized) { InitializeSeat(); }//can't do this in start because hudcontrol might not have initialized
        //voice range change to allow talking inside cockpit (after VRC patch 1008)
        if (player != null)
        {
            EntityControl.SeatedPlayers[ThisStationID] = player.playerId;
            if (player.isLocal)
            {
                foreach (int crew in EntityControl.SeatedPlayers)
                {
                    VRCPlayerApi guy = VRCPlayerApi.GetPlayerById(crew);
                    if (guy != null)
                    {
                        SetVoiceInside(guy);
                    }
                }
            }
            else if (EntityControl.InVehicle)
            {
                SetVoiceInside(player);
            }
        }
        EntityControl.PassengerEnterVehicleGlobal();
    }
    public override void OnStationExited(VRCPlayerApi player)
    {
        if (!SeatInitialized) { InitializeSeat(); }
        PlayerExitPlane(player);
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (!SeatInitialized) { InitializeSeat(); }
        if (player.playerId == EntityControl.SeatedPlayers[ThisStationID])
        {
            PlayerExitPlane(player);
        }
    }
    public void PlayerExitPlane(VRCPlayerApi player)
    {
        if (!SeatInitialized) { InitializeSeat(); }
        EntityControl.PassengerExitVehicleGlobal();

        EntityControl.SeatedPlayers[ThisStationID] = -1;
        if (player != null)
        {
            SetVoiceOutside(player);
            if (player.isLocal)
            {
                EntityControl.MySeat = -1;
                EntityControl.PassengerExitVehicleLocal();
                //undo voice distances of all players inside the vehicle
                foreach (int crew in EntityControl.SeatedPlayers)
                {
                    VRCPlayerApi guy = VRCPlayerApi.GetPlayerById(crew);
                    if (guy != null)
                    {
                        SetVoiceOutside(guy);
                    }
                }
                if (PassengerOnly != null) { PassengerOnly.SetActive(false); }
                if (SeatAdjuster != null) { SeatAdjuster.SetActive(false); }
            }
        }
    }

    private void SetVoiceInside(VRCPlayerApi Player)
    {
        Player.SetVoiceDistanceNear(999999);
        Player.SetVoiceDistanceFar(1000000);
        Player.SetVoiceGain(.6f);
    }
    private void SetVoiceOutside(VRCPlayerApi Player)
    {
        Player.SetVoiceDistanceNear(0);
        Player.SetVoiceDistanceFar(25);
        Player.SetVoiceGain(15);
    }
    private void InitializeSeat()
    {
        int x = 0;
        foreach (VRCStation station in EntityControl.VehicleStations)
        {
            if (station.gameObject == gameObject)
            {
                ThisStationID = x;
            }
            x++;
        }
        SeatInitialized = true;
    }
}