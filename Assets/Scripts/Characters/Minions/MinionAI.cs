using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.AI;

public class MinionAI : MonoBehaviourPun
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
        if (photonView.IsMine)
        {
            FindAndSetTarget();
        }
        
    }

    void Update()
    {
        if(!photonView.IsMine) { return; }

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
