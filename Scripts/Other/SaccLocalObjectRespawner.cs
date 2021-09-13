
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class SaccLocalObjectRespawner : UdonSharpBehaviour
{
    public GameObject ObjectToRespawn;
    public Transform RespawnPoint;
    private VRCPlayerApi localPlayer;
    private void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }
    void Interact()
    {
        Networking.SetOwner(localPlayer, ObjectToRespawn);
        ObjectToRespawn.transform.position = RespawnPoint.position;
        ObjectToRespawn.transform.rotation = RespawnPoint.rotation;
    }
}
