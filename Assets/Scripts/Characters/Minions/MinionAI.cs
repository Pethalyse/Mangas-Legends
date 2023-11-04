using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class MinionAI : NetworkBehaviour
{

    private NavMeshAgent agent;
    private MinionAIAttack attack;
    private Transform currentTarget;
    public string enemyMinionTag;
    public string turretTag;
    public float stopDistance = 2f;
    public float aggroRange = 5f;
    public float targetSwitchInterval = 2f;

    private float timeSinceLastTargetSwitch = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        attack = GetComponent<MinionAIAttack>();

        stopDistance = attack.Stats.range.GetValue();

        if (isServer) 
        {
            FindAndSetTarget();
        }
        
    }

    void Update()
    {
        if(!isServer) { return; }
        timeSinceLastTargetSwitch += Time.deltaTime;

        if(timeSinceLastTargetSwitch >= targetSwitchInterval)
        {
            CheckAndSwitchTargets();
            timeSinceLastTargetSwitch = 0f;
        }

        if(currentTarget != null)
        {
            Vector3 directionToTarget = currentTarget.position - transform.position;

            Vector3 stoppingPosition = currentTarget.position - directionToTarget.normalized * stopDistance;

            agent.SetDestination(stoppingPosition);

            attack.TryAttack(stoppingPosition, currentTarget, stopDistance);
        }
    }

    [Server]
    private void CheckAndSwitchTargets()
    {
        GameObject[] enemyMinions = GameObject.FindGameObjectsWithTag(enemyMinionTag);
        Transform closestEnemyMinion = GetClosestObjectInRadius(enemyMinions, aggroRange);

        if(closestEnemyMinion != null)
        {
            currentTarget = closestEnemyMinion;
        }
        else
        {
            GameObject[] turrets = GameObject.FindGameObjectsWithTag(turretTag);
            currentTarget = GetClosestObject(turrets);
        }
    }

    [Server]
    private void FindAndSetTarget()
    {
        GameObject[] enemyMinions = GameObject.FindGameObjectsWithTag(enemyMinionTag);
        Transform closestEnemyMinion = GetClosestObjectInRadius(enemyMinions, aggroRange);

        if(closestEnemyMinion != null)
        {
            currentTarget = closestEnemyMinion;
        }
        else
        {
            GameObject[] turrets = GameObject.FindGameObjectsWithTag(turretTag);
            currentTarget = GetClosestObject(turrets);
        }
    }

    [Server]
    private Transform GetClosestObject(GameObject[] objects)
    {
        float closestDistance = Mathf.Infinity;
        Transform closestObject = null;
        Vector3 currentPosition = transform.position;

        foreach(GameObject obj in objects)
        {
            float distance = Vector3.Distance(currentPosition, obj.transform.position);

            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = obj.transform;
            }
        }

        return closestObject;
    }

    [Server]
    private Transform GetClosestObjectInRadius(GameObject[] objects, float radius)
    {
        float closestDistance = Mathf.Infinity;
        Transform closestObject = null;
        Vector3 currentPosition = transform.position;

        foreach (GameObject obj in objects)
        {
            float distance = Vector3.Distance(currentPosition, obj.transform.position);

            if (distance < closestDistance && distance <= radius)
            {
                closestDistance = distance;
                closestObject = obj.transform;
            }
        }

        return closestObject;
    }
}
