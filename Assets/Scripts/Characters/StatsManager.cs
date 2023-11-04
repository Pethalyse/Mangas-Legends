using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

abstract public class StatsManager : TeamManager
{

    [Header("Icon")]
    [SerializeField] private Sprite icon;
    public Sprite Icon { get => icon;}

    //stats
    [Header("Stats")]
    [Header("Level")]
    [SyncVar][SerializeField] protected int level = 0; //les stats selon le level à partir du level 2

    [SerializeField] private StatsCharacterList stats;
    public StatsCharacterList Stats { get => stats;}

    //vie
    [SyncVar][SerializeField] protected float vie;
    public float Vie { get => vie;}
    protected float nextRegenPvTime;

    //mana
    [SyncVar][SerializeField] protected float mana;
    public float Mana { get => mana;}
    protected float nextRegenManaTime;

    //GOLDS
    [Header("Golds")]
    [SerializeField] private int goldsOnDeath;

    [Header("Stats changeante")]
    [SerializeField] protected int slow;
    [SerializeField] protected bool cantBeCC;

    private NavMeshAgent agent;

    public void setCantBeCC(bool v) { cantBeCC = v; }
    public bool getCantBeCC() { return cantBeCC; }

    protected void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected void Update()
    {
    }

    protected void Start()
    {
        stats = Instantiate(stats);

        vie = stats.vieMax.GetValue();
        mana = stats.manaMax.GetValue();

        agent.speed = Stats.moveSpeed.GetValue();
    }

    //Fonction Stats
    //Level
    [Command]
    public void CmdLeveling()
    {
        if (level < 18)
        {
            level++;
            stats.LevelUp();

            vie += stats.vieMax.GetValueLeveling();
            mana += Stats.manaMax.GetValueLeveling();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdTakeDamage(float damage, RatioDamage ratioDamage)
    {
        RpcTakeDamage(damage, ratioDamage);
    }

    //takeDamage
    [ClientRpc]
    private void RpcTakeDamage(float damage, RatioDamage ratioDamage)
    {
        float dmg = 0;
        switch (ratioDamage)
        {
            case RatioDamage.AD:
                {
                    dmg = Mathf.Round(damage * (100 / (100 + stats.ar.GetValue())));//calcule des degats selon l'armor
                    foreach (ShieldAD sad in GetComponents<ShieldAD>())
                    {
                        if (sad.getShield() != 0)
                        {
                            if (sad.getShield() >= dmg)
                            {
                                sad.setShield(sad.getShield() - Mathf.RoundToInt(dmg));
                                dmg = 0;
                            }
                            else
                            {
                                sad.setShield(0);
                                dmg -= sad.getShield();
                            }
                        }

                        if (dmg <= 0)
                        {
                            break;
                        }
                    }


                    break;
                }
            case RatioDamage.AP:
                {
                    dmg = Mathf.Round(damage * (100 / (100 + stats.mr.GetValue()))); //calcule des degats selon la magic resist
                    foreach (ShieldAP sap in GetComponents<ShieldAP>())
                    {
                        if (sap.getShield() != 0)
                        {
                            if (sap.getShield() >= dmg)
                            {
                                sap.setShield(sap.getShield() - Mathf.RoundToInt(dmg));
                                dmg = 0;
                            }
                            else
                            {
                                sap.setShield(0);
                                dmg -= sap.getShield();
                            }
                        }

                        if (dmg <= 0)
                        {
                            break;
                        }
                    }

                    break;
                }
            default:
                {
                    Debug.Log("probleme, manque de Degats");
                    break;
                }
        }

        foreach (ShieldAll sa in GetComponents<ShieldAll>())
        {
            if (sa.getShield() != 0)
            {
                if (sa.getShield() >= dmg)
                {
                    sa.setShield(sa.getShield() - Mathf.RoundToInt(dmg));
                    dmg = 0;
                }
                else
                {
                    sa.setShield(0);
                    dmg -= sa.getShield();
                }
            }

            if (dmg <= 0)
            {
                break;
            }
        }

        vie -= dmg;
        vie = Mathf.Clamp(vie, 0, stats.vieMax.GetValue());
        Death();
        //Debug.Log(gameObject.name + ", à pris des dégats : " + vie);
    }

    private void Death()
    {
        if (vie < 1)
        {
            //GameObject lastWhoHit = PhotonView.Find(id).gameObject;
            //ChampionControleur cc = lastWhoHit.GetComponent<ChampionControleur>();
            //if (cc)
            //{
            //    cc.RPC_targetToNull();
            //    cc.RPC_GiveGolds(goldsOnDeath);
            //    if (this is MinionAIAttack)
            //    {
            //        cc.NbMinions += 1;
            //    }
            //    else if (this is ChampionControleur)
            //    {
            //        cc.NbKills += 1;
            //    }
            //}

            Debug.Log("mort");
        }
    }

    [Command]
    public void CmdTakeHeal(float heal)
    {
        RpcTakeHeal(heal);
    }

    //heal
    [ClientRpc]
    private void RpcTakeHeal(float heal)
    {
        var healing = Mathf.Round(heal); //arrondi du heal
        vie += healing;
        vie = Mathf.Clamp(vie, 0, stats.vieMax.GetValue());
        Debug.Log(gameObject.name + ", à été heal : " + vie);
    }

    [Command]
    public void CmdSetSlow(int pourcentage, float time)
    {
        RpcSetSlow(pourcentage, time);
    }

    //Slow
    [ClientRpc]
    private void RpcSetSlow(int pourcentage, float time)
    {
        StartCoroutine(slowTime(pourcentage, time));
    }

    //COROUTINE
    private IEnumerator slowTime(int pourcentage, float time)
    {
        stats.moveSpeed.AddAlteration(pourcentage);
        agent.speed = stats.moveSpeed.GetValue();

        yield return new WaitForSeconds(time);

        stats.moveSpeed.RemoveAlteration(pourcentage);
        agent.speed = stats.moveSpeed.GetValue();
    }
}
