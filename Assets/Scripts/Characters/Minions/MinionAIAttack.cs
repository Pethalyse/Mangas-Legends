using Mirror;
using System.Collections;
using UnityEngine;

public class MinionAIAttack : StatsManager
{
    bool canAttack = true;

    [Server]
    new private void Start()
    {
        base.Start();

        GameManager.RegisterObjet(GetComponent<NetworkIdentity>().netId.ToString(), this);
    }

    [Server]
    new private void Update()
    {
        base.Update();
    }

    [Server]
    public void TryAttack(Vector3 stoppingPosition, Transform currentTarget, float stopDistance)
    {
        if (!canAttack) { return; }
        if (gameObject.transform.position == stoppingPosition)
        {
            StatsManager sm = GameManager.GetFromAll(currentTarget.name);
            if (sm)
            {
                Attack(sm);
            }   
        }
    }

    [Server]
    private void Attack(StatsManager statsManager)
    {
        statsManager.CmdTakeDamage(Stats.ad.GetValue(), RatioDamage.AD);
        StartCoroutine(StartCD());
    }

    [Server]
    private IEnumerator StartCD()
    {
        canAttack = false;

        yield return new WaitForSeconds(Stats.attackSpeed.GetValue());

        canAttack = true;
    }
}
