using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Projectile: AbilityControleur
{
    private Transform target;
    private NavMeshAgent agent;

    private float speed;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if(target == null) return;

        agent.SetDestination(target.position);
    }

    public void setTarget(Transform target) { this.target = target; }

    private void OnTriggerEnter(Collider other)
    {
        if (player == null || !player.IsLocal) { return; }
        if (target == null) return;

        if (other.transform == target)
        {
            target.gameObject.GetComponent<StatsManager>()?.TakeDamage(getValue(), ratioDamage, send);
            if (target)
            {
                send.GetComponent<ChampionControleur>()?.ActiveOnHitPassifsItem(target.gameObject.GetComponent<StatsManager>());
            }
                
            PhotonNetwork.Destroy(gameObject);
        }
    }

}