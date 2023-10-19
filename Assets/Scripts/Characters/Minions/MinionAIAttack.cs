using Mirror;
using System.Collections;
using UnityEngine;

public class MinionAIAttack : StatsManager
{
    bool canAttck = true;

    new private void Start()
    {
        base.Start();

        GameManager.RegisterObjet(GetComponent<NetworkIdentity>().netId.ToString(), this);
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
            StatsManager sm = GameManager.GetFromAll(currentTarget.name);
            if (sm)
            {
                Attack(sm);
            }   
        }
    }

    private void Attack(StatsManager statsManager)
    {
        statsManager.CmdTakeDamage(ad, 0, 0);
        StartCoroutine(StartCD());
    }

    private IEnumerator StartCD()
    {
        canAttck = false;

        yield return new WaitForSeconds(attackSpeed);

        canAttck = true;
    }
}
