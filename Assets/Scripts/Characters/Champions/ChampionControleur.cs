
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class ChampionControleur : StatsManager
{

    [Header("Stats Champions")]
    [Header("Penetration")]
    [SerializeField] private int lethality;
    [SerializeField] private int peneArmor;
    [SerializeField] private int peneMagic;

    [Header("LideSteal")]
    [SerializeField] private int lifeSteal;

    [Header("Support")]
    [SerializeField] private int bonusHeal;

    [Header("CDR")]
    [SerializeField] private int cdr;

    //system money
    [Header("Items")]
    [SerializeField] private int goldsOnStock;
    private int nbMaxItems = 6;
    [SerializeField] private List<Item> items = new List<Item>();

    //auto attaque
    [Header("Auto Attaque")]
    [SerializeField] protected GameObject aaVisuel;

    ////utilisation stats
    //[Header("Utilisation des stats")]
    ////protected float attackInterval;
    ////protected float nextAttackTime = 0;
    [Header("Auto Attaque action")]
    [SerializeField] protected bool isAttack = false;
    protected bool canAuto = true;
    protected Transform aaHit;

    //target
    private Transform target;

    //Components
    [Header("Components")]
    [SerializeField] protected GameObject spawnPoint;
    [SerializeField] private GUIControleur gui;
    [SerializeField] private GameObject shop;
    protected Mouvements _mouvements;
    protected Animations animations;
    [SerializeField] protected GameObject IndicatorRange;
    private GameObject IR;

    //GUI
    [Header("GUI")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider vieSlider;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private int nbKills = 0;
    [SerializeField] private int nbDeath = 0;
    [SerializeField] private int nbAssist = 0;
    [SerializeField] private int nbMinions = 0;
    public int NbKills { get => nbKills; set => nbKills = value; }
    public int NbDeath { get => nbDeath; set => nbDeath = value; }
    public int NbAssist { get => nbAssist; set => nbAssist = value; }
    public int NbMinions { get => nbMinions; set => nbMinions = value; }

    //inBattle
    protected bool inBattle = false;

    //Setters Getters
    //MOUVEMETNS
    public float getAutoRange() { return range; }
    public Transform getTarget() { return target; }
    public void setTarget(Transform target) { this.target = target; }
    public float getMoveSpeed() { return moveSpeed; }

    //ATTACKS
    public void setIsAttack(bool v) { isAttack = v; inBattle = v; }
    public bool getIsAttack() { return isAttack; }
    public void setCanAuto(bool v) { canAuto = v; }
    public bool getCanAuto() { return canAuto; }

    //ITEMS
    public int getGolds() { return goldsOnStock; }
    public List<Item> getItems() { return items; }

    public void RPC_targetToNull() { photonView.RPC("targetToNull", RpcTarget.All); }

    [PunRPC]
    public void targetToNull() 
    { 
        if (!photonView.IsMine) { return; } 
        target = null; 
    }

    new protected void Awake()
    {
        base.Awake();
        _mouvements = gameObject.GetComponent<Mouvements>();
        animations = GetComponent<Animations>();
        if(photonView.IsMine)
        {
            gui = GameObject.Find("2D GUI").GetComponent<GUIControleur>();
            shop = GameObject.Find("Shop");
            shop.SetActive(false);
        }

        //gameObject.tag = "Player";
    }

    new void Start()
    {
        base.Start();

        level = 0;
        goldsOnStock = 0;

        gameObject.tag = "Player";
        gameObject.layer = LayerMask.NameToLayer("Characters");

        if (photonView.IsMine)
        {
            Leveling();
        }
    }

    // Update is called once per frame
    new protected void Update()
    { 
        if (photonView.IsMine)
        {
            base.Update();
            Inputs();
            animations.changeAttackSpeed(attackSpeed);

            regenerationVie();
            regenerationMana();
        }
    }

    private void LateUpdate()
    {
        updateGUI();
        if (photonView.IsMine)
        {
            gui.miseAJour(this);
        }
    }

    private void updateGUI()
    {
        levelText.text = level.ToString();
        vieSlider.maxValue = vieMax;
        vieSlider.value = vie;
        manaSlider.maxValue = manaMax;
        manaSlider.value = mana;
    }

    //FONCTIONS USUELLES
    private void Inputs()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Characters")) && !inSameTeam(hit.collider.gameObject))
            {
                target = hit.collider.transform;
                _mouvements.lookAt(hit.collider.transform.position);

                if (Vector3.Distance(transform.position, hit.collider.transform.position) <= range && canAuto) // && Time.time > nextAttackTime
                {
                    aaHit = hit.collider.transform;
                    photonView.RPC("autoAttaque", RpcTarget.All);
                }
            }

        }
        else if (target != null && Vector3.Distance(transform.position, target.position) <= range && canAuto && !inSameTeam(target.gameObject)) // && Time.time > nextAttackTime
        {
            _mouvements.lookAt(target.position);
            aaHit = target.transform;
            photonView.RPC("autoAttaque", RpcTarget.All);

        }

        if (Input.GetMouseButtonDown(2))
        {
            Leveling();
            TakeDamage(400, 0, photonView.ViewID);
            GiveGolds(3100);
        }


        if (Input.GetKeyDown(KeyCode.P))
        { 
            UI_Shop.showShop(!UI_Shop.self.activeSelf);
        }

        if (shop.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            UI_Shop.showShop(false);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            AfficherRangeQClick();
            if (Input.GetMouseButtonDown(0)) { QClickAttack(); }
        }

        if (Input.GetKeyUp(KeyCode.A) && IR) { DesafficherRangeQClick(); }

    }

    //FONCTIONS STATISTIQUES
    virtual protected void regenerationVie()
    {
        if (Time.time >= nextRegenPvTime)
        {
            nextRegenPvTime = Time.time + 1f/100;

            if (vie < vieMax)
            {
                if (!inBattle)
                {
                    vieRegen = vieRegenBase + vieRegenLeveling * level;
                    
                }
                else
                {
                    vieRegen = (vieRegenBase + vieRegenLeveling * level) / 4;
                }

                vie += vieRegen/100f;

                if (vie > vieMax)
                {
                    vie = vieMax;
                }
            }
        }
    }

    virtual protected void regenerationMana()
    {
        if (Time.time >= nextRegenManaTime)
        {
            nextRegenManaTime = Time.time + 1f / 100;

            if (mana < manaMax)
            {
                if (!inBattle)
                {
                    manaRegen = manaRegenBase + manaRegenLeveling * level;
                }
                else
                {
                    manaRegen = (manaRegenBase + manaRegenLeveling * level) / 4;
                }

                mana += manaRegen / 100f;

                if (mana > manaMax)
                {
                    mana = manaMax;
                }
            }
        }
    }

    //ITEMS
    public void RPC_GiveGolds(int goldsGive)
    {
        photonView.RPC("GiveGolds", RpcTarget.All, goldsGive);
    }

    [PunRPC]
    public void GiveGolds(int goldsGive)
    {
        if (!photonView.IsMine) { return; }
        goldsOnStock += goldsGive;
    }

    public bool AddItem(Item add, int prix)
    {
        if(prix <= goldsOnStock && items.Count < nbMaxItems)
        {
            goldsOnStock -= prix;

            foreach(Item item in add.composents)
            {
                if (items.Contains(item))
                {
                    items.Remove(item);
                    ActualiserStatsSelonItemsNegatif(item);
                }
                else
                {
                    foreach (Item i in item.composents)
                    {
                        if (items.Contains(i))
                        {
                            items.Remove(i);
                            ActualiserStatsSelonItemsNegatif(i);
                        }
                    }
                }

            }

            items.Add(add);
            ActualiserStatsSelonItemsPositif(add);
            return true;
        }

        return false;
    }

    public bool SellItem(Item remove)
    {
        if (items.Contains(remove))
        {
            goldsOnStock += Mathf.RoundToInt(remove.prix * 0.7f);
            items.Remove(remove);
            ActualiserStatsSelonItemsNegatif(remove);
            return true;
        }

        return false;
    }

    private void ActualiserStatsSelonItemsPositif(Item item)
    {
        //flats
        vieMax += item.vie;
        vie += item.vie;
        manaMax += item.mana;
        mana += item.mana;
        ad += item.ad;
        ap += item.ap;
        ar += item.armor;
        mr += item.magicRes;
        critChance += item.critChance;
        critDamage += item.critDamage;
        bonusHeal += item.bonusHeal;
        lethality += item.lethality;
        peneArmor += item.peneArmor;
        peneMagic += item.peneMagic;
        cdr += item.cdr;

        //pourcentages
        attackSpeed += attackSpeedBase * item.aSpd / 100;
        manaRegenBase += manaRegenBase * item.regenMana / 100;
        moveSpeed += moveSpeedBase * item.ms / 100;
    }    
    
    private void ActualiserStatsSelonItemsNegatif(Item item)
    {
        //flats
        vieMax -= item.vie;
        vie -= item.vie;
        manaMax -= item.mana;
        manaMax -= item.mana;
        ad -= item.ad;
        ap -= item.ap;
        ar -= item.armor;
        mr -= item.magicRes;
        critChance -= item.critChance;
        critDamage -= item.critDamage;
        bonusHeal -= item.bonusHeal;
        lethality -= item.lethality;
        peneArmor -= item.peneArmor;
        peneMagic -= item.peneMagic;
        cdr += item.cdr;

        //pourcentages
        attackSpeed -= attackSpeedBase * item.aSpd / 100;
        manaRegenBase -= manaRegenBase * item.regenMana / 100;
        moveSpeed -= moveSpeedBase * item.ms / 100;
    }

    //PASSIFS
    public void ActiveOnHitPassifsItem(StatsManager target)
    {
        foreach(Item item in items)
        {
            foreach(var passif in item.passifs)
            {
                if(passif is OnHitPassif)
                {
                    var pass = passif as OnHitPassif;
                    pass.itemInteraction(this, target);
                }
            }
        }
    }

    //ATTAQUES
    [PunRPC]
    virtual protected void autoAttaque() 
    {
        isAttack = true;
        inBattle = true;
    }

    private void AfficherRangeQClick()
    {
        if (!IR)
        {
            foreach (Transform transform in IndicatorRange.transform)
            {
                transform.localScale = new Vector3(range * 2, range * 2, range * 2);
            }
            IR = Instantiate(IndicatorRange, gameObject.transform);
        }
    }

    private void DesafficherRangeQClick()
    {
        Destroy(IR);
        IR = null;
    }

    private Transform QClickAttack()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestObject = null;
        Vector3 currentPosition = transform.position;

        //foreach (GameObject obj in objects)
        //{
        //    float distance = Vector3.Distance(currentPosition, obj.transform.position);

        //    if (distance < closestDistance && distance <= radius)
        //    {
        //        closestDistance = distance;
        //        closestObject = obj.transform;
        //    }
        //}
        
        DesafficherRangeQClick();
        return closestObject;

    }

    //Coroutines
    protected void AutoAttaque()
    {
        if (target == null && aaHit == null)
        {
            animations.stopAaAnimation();
            canAuto = true;
        }
        else
        {
            canAuto = false;
            animations.startAaAnimation();
        }
    }

    //FULL VIRTUAL
    virtual public void passif() { }

    virtual public void ability1() { }
    virtual public void ability2() { }
    virtual public void ability3() { }
    virtual public void ability4() { }

    //SYNCR SERVEUR
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting)
        {
            stream.SendNext(nbKills);
            stream.SendNext(nbDeath);
            stream.SendNext(nbAssist);
            stream.SendNext(nbMinions);
        }
        else
        {
            nbKills = (int)stream.ReceiveNext();
            nbDeath = (int)stream.ReceiveNext();
            nbAssist = (int)stream.ReceiveNext();
            nbMinions = (int)stream.ReceiveNext();
        }
    }

}
