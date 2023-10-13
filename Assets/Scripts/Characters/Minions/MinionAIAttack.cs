using Photon.Pun.UtilityScripts;
using System.Collections;
using UnityEngine;

public class MinionAIAttack : StatsManager
{
    bool canAttck = true;

    [Header("team")]
    public PhotonTeam team;

    new private void Start()
    {
        base.Start();

        if(team.Code == 0)
        {
            team.Name = "Blue";
        }
        else if(team.Code == 1)
        {
            team.Name = "Red";
        }
    }

    new private void Update()
    {
        base.Update();
    }

    public void TryAttack(Vector3 stoppingPosition, Transform currentTarget, float stopDistance)
    {
        if (!canAttck) { return; }
        if (gameObject.transform.position == stoppingPosition)
        {
            StatsManager sm = currentTarget.gameObject?.GetComponent<StatsManager>();
            if (sm)
            {
                Attack(sm);
            }   
        }
    }

    private void Attack(StatsManager statsManager)
    {
        statsManager.TakeDamage(ad, 0, gameObject);
        StartCoroutine(StartCD());
    }

    private IEnumerator StartCD()
    {
        canAttck = false;

        yield return new WaitForSeconds(attackSpeed);

        canAttck = true;
    }
}
