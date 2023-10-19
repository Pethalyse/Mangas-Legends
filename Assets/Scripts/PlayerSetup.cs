using Mirror;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{

    [SerializeField]
    Behaviour[] componentsToDisable;

    void Start()
    {
        if(!isLocalPlayer)
        {
            DisableCompenents();
        }

        AssignTagLayer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        string netId = GetComponent<NetworkIdentity>().netId.ToString();
        ChampionControleur cc = GetComponent<ChampionControleur>();

        GameManager.RegisterPlayer(netId, cc);
    }

    private void AssignTagLayer()
    {
        gameObject.tag = "Player";
        gameObject.layer = LayerMask.NameToLayer("Characters");
    }

    private void DisableCompenents()
    {
        foreach (Behaviour component in componentsToDisable)
        {
            component.enabled = false;
        }
    }

    private void OnDisable()
    {
        GameManager.UnregisterPlayer(transform.name);
    }
}
