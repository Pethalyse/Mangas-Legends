using Mirror;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(StatsManager))]
public class PvControleur: NetworkBehaviour
{
    [SyncVar][SerializeField] protected float vie;
    public float Vie { get => vie; }

    [SerializeField] bool canRegen;
    protected float nextRegenPvTime;

    StatsCharacterList stats;
    private bool inBattle;

    public StatsCharacterList Stats { set => stats = value; }

    private void Update()
    {
        if (!stats) { return; }

        if(canRegen)
        {
            CmdRegenerationVie();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdTakeDamage(float damage, RatioDamage ratioDamage)
    {
        if (!stats) { Debug.LogWarning("Stats du controleur PV n'existe pas !"); }

        //FAIRE QUE LES SHIELD ONT UN NETWORKBEHAVIOUR ET CHANGER LEURS VALEUR SUR LE SERVEUR
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

    [Command]
    virtual protected void CmdRegenerationVie()
    {
        if (Time.time >= nextRegenPvTime)
        {
            nextRegenPvTime = Time.time + 1f / 100;

            if (vie < stats.vieMax.GetValue())
            {
                if (!inBattle)
                {
                    vie += stats.vieRegen.GetValue() / 100;
                }
                else
                {
                    vie += stats.vieRegen.GetValue() / 100 / 4;
                }

                if (vie > stats.vieMax.GetValue())
                {
                    vie = stats.vieMax.GetValue();
                }
            }
        }
    }

    [Command]
    public void CmdTakeHeal(float heal)
    {
        vie += heal;
        vie = Mathf.Clamp(vie, 0, stats.vieMax.GetValue());
        Debug.Log(gameObject.name + ", à été heal : " + vie);
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
}