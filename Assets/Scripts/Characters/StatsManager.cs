using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

abstract public class StatsManager : TeamManager, IPunObservable
{
    //stats
    [Header("Stats")]
    [Header("Level")]
    [SerializeField] protected int level; //les stats selon le level à partir du level 2

    //vie
    [Header("Vie")]
    [SerializeField] protected float vie;
    [SerializeField] protected float vieMax;
    [SerializeField] protected float vieLeveling;
    [Header("VieRegen")]
    [SerializeField] protected float vieRegenBase;
    [SerializeField] protected float vieRegenLeveling;
    protected float vieRegen;
    protected float nextRegenPvTime;

    //mana
    [Header("Mana")]
    [SerializeField] protected float mana;
    [SerializeField] protected float manaMax;
    [SerializeField] protected float manaLeveling;
    [Header("ManaRegen")]
    [SerializeField] protected float manaRegenBase;
    [SerializeField] protected float manaRegenLeveling;
    protected float manaRegen;
    protected float nextRegenManaTime;

    //ms
    [Header("MS")]
    [SerializeField] protected float moveSpeedBase;
    [SerializeField] protected float moveSpeed;

    //AD
    [Header("AD")]
    [SerializeField] protected float adBase;
    [SerializeField] protected float adLeveling;
    [SerializeField] protected float ad;

    //AP
    [Header("AP")]
    [SerializeField] protected float apBase;
    [SerializeField] protected float ap;

    //ARMOR
    [Header("ARMOR")]
    [SerializeField] protected float arBase;
    [SerializeField] protected float arLeveling;
    [SerializeField] protected float ar;

    //MAGIC RESIST
    [Header("Magic Resist")]
    [SerializeField] protected float mrBase;
    [SerializeField] protected float mrLeveling;
    [SerializeField] protected float mr;

    //CRIT
    [Header("Critique")]
    [SerializeField] protected int critChance = 0;
    [SerializeField] protected int critDamage = 150;

    //AA
    [Header("Range")]
    [SerializeField] protected float rangeBase;
    [SerializeField] protected float range;

    //AS
    [Header("AS")]
    [SerializeField] protected float attackSpeedBase;
    [SerializeField] protected float attackSpeedLevelingPourcent;
    [SerializeField] protected float attackSpeed;

    //GOLDS
    [Header("Golds")]
    [SerializeField] private int goldsOnDeath;

    [Header("Stats changeante")]
    [SerializeField] protected int slow;
    [SerializeField] protected bool cantBeCC;

    private NavMeshAgent agent;

    private int nbLoop = 0;

    //SETTER GETTER
    //GUI
    public float getVie() { return vie; }
    public float getVieMax() { return vieMax; }
    public float getMana() { return mana; }
    public float getManaMax() { return manaMax; }

    //STATS
    public float getAD() { return ad; }
    public float getAP() { return ap; }
    public int getCritChance() { return critChance; }
    public int getCritDamage() { return critDamage; }

    public void setCantBeCC(bool v) { cantBeCC = v; }
    public bool getCantBeCC() { return cantBeCC; }

    protected void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected void Update()
    {
        if(!photonView.IsMine) { return; }
    }

    protected void Start()
    {
        mana = manaMax;
        manaRegen = manaRegenBase;
        vie = vieMax;
        vieRegen = vieRegenBase;

        ad = adBase;
        ap = apBase;
        mr = mrBase;
        ar = arBase;

        moveSpeed = moveSpeedBase;
        range = rangeBase;
        attackSpeed = attackSpeedBase;

        if (photonView.IsMine)
        {
            InvokeRepeating("updateNbLoop", 0f, 1f);
        }
    }

    public void updateNbLoop() { nbLoop++; }

    //Fonction Stats
    //Level
    public void Leveling()
    {
        if (level < 18)
        {
            level++;

            manaMax += manaLeveling;
            mana += manaLeveling * level;
            if (mana > manaMax) { mana = manaMax; }
            vieMax += vieLeveling;
            vie += vieLeveling * level;
            if (vie > vieMax) { vie = vieMax; }

            ad = adBase + adLeveling * level;
            mr = mrBase + mrLeveling * level;
            ar = arBase + arLeveling * level;

            attackSpeed += attackSpeedBase * (attackSpeedLevelingPourcent / 100);
        }
    }

    public void RPC_TakeDamage(float damage, int ratioDamage, int id)
    {
        photonView.RPC("TakeDamage", RpcTarget.All, damage, ratioDamage, id);
    }

    //takeDamage
    [PunRPC]
    public void TakeDamage(float damage, int ratioDamage, int id)
    {
        if (!photonView.IsMine) { return; }

        float dmg = 0;
        switch (ratioDamage)
        {
            case 0:
                {
                    dmg = Mathf.Round(damage * (100 / (100 + ar)));//calcule des degats selon l'armor
                    foreach (ShieldAD sad in GetComponents<ShieldAD>())
                    {
                        if (sad.getShield() != 0)
                        {
                            if (sad.getShield() >= dmg)
                            {
                                sad.setShield(sad.getShield() - (int)dmg);
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
            case 1:
                {
                    dmg = Mathf.Round(damage * (100 / (100 + mr))); //calcule des degats selon la magic resist
                    foreach (ShieldAP sap in GetComponents<ShieldAP>())
                    {
                        if (sap.getShield() != 0)
                        {
                            if (sap.getShield() >= dmg)
                            {
                                sap.setShield(sap.getShield() - (int)dmg);
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
                    sa.setShield(sa.getShield() - (int)dmg);
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
        vie = Mathf.Clamp(vie, 0, vieMax);
        Death(id);
        //Debug.Log(gameObject.name + ", à pris des dégats : " + vie);
    }

    private void Death(int id)
    {
        if (vie < 1)
        {
            GameObject lastWhoHit = PhotonView.Find(id).gameObject;
            lastWhoHit.GetComponent<ChampionControleur>()?.RPC_targetToNull();
            lastWhoHit.GetComponent<ChampionControleur>()?.RPC_GiveGolds(goldsOnDeath);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void RPC_TakeHeal(float heal)
    {
        photonView.RPC("TakeHeal", RpcTarget.All, heal);
    }

    //heal
    [PunRPC]
    public void TakeHeal(float heal)
    {
        if (!photonView.IsMine) { return; }

        var healing = Mathf.Round(heal); //arrondi du heal
        vie += healing;
        vie = Mathf.Clamp(vie, 0, vieMax);
        Debug.Log(gameObject.name + ", à été heal : " + vie);
    }

    public void RPC_SetSlow(int pourcentage, float time)
    {
        photonView.RPC("SetSlow", RpcTarget.All, pourcentage, time);
    }

    //Slow
    [PunRPC]
    public void SetSlow(int pourcentage, float time)
    {
        if(!photonView.IsMine) { return ; }

        StartCoroutine(slowTime(pourcentage, time));
    }

    //COROUTINE
    private IEnumerator slowTime(int pourcentage, float time)
    {
        var variation = moveSpeedBase * pourcentage / 100;

        moveSpeed -= variation;

        yield return new WaitForSeconds(time);

        moveSpeed += variation;
    }

    //SYNCR SERVEUR
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(level);

            stream.SendNext(vie);
            stream.SendNext(vieMax);
            stream.SendNext(mana);
            stream.SendNext(manaMax);

            stream.SendNext(ad);
            stream.SendNext(ap);
            stream.SendNext(ar);
            stream.SendNext(mr);

            stream.SendNext(critChance);
            stream.SendNext(critDamage);

            stream.SendNext(attackSpeed);
            stream.SendNext(range);

            stream.SendNext(slow);
            stream.SendNext(moveSpeed);

            stream.SendNext(cantBeCC);

            stream.SendNext(nbLoop);

            //stream.SendNext(transform.position);
            //stream.SendNext(transform.rotation);
        }
        else
        {
            level = (int)stream.ReceiveNext();

            vie = (float)stream.ReceiveNext();
            vieMax = (float)stream.ReceiveNext();
            mana = (float)stream.ReceiveNext();
            manaMax = (float)stream.ReceiveNext();

            ad = (float)stream.ReceiveNext();
            ap = (float)stream.ReceiveNext();
            ar = (float)stream.ReceiveNext();
            mr = (float)stream.ReceiveNext();

            critChance = (int)stream.ReceiveNext();
            critDamage = (int)stream.ReceiveNext();

            attackSpeed = (float)stream.ReceiveNext();
            range = (float)stream.ReceiveNext();

            slow = (int)stream.ReceiveNext();
            moveSpeed = (float)stream.ReceiveNext();

            cantBeCC = (bool)stream.ReceiveNext();

            nbLoop = (int)stream.ReceiveNext();

            //transform.position = (Vector3)stream.ReceiveNext();
            //transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
