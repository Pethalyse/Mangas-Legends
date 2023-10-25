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
        if (send != GameManager.GetLocalPlayer()) { return; }
        if (target == null) return;

        if (other.transform == target)
        {
            StatsManager sm = GameManager.GetFromAll(other.name);
            ChampionControleur cc = GameManager.GetPlayer(send.name);
            if (!sm || !cc) return;
            sm.CmdTakeDamage(getValue(), ratioDamage);
            if (target)
            {
                cc.ActiveOnHitPassifsItem(sm);
            }
                
            Destroy(gameObject);
        }
    }

}