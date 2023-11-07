
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class ChampionControleur : StatsManager
{
    //system money
    [Header("Items")]
    [SyncVar][SerializeField] private int goldsOnStock = 0;
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
    [SerializeField] private GameObject scoreboard;
    protected Mouvements mouvements;
    protected Animations animations;
    [SerializeField] protected GameObject IndicatorRange;
    private GameObject IR;

    //GUI
    [Header("GUI")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider vieSlider;
    [SerializeField] private Slider manaSlider;
    [SyncVar][SerializeField] private int nbKills = 0;
    [SyncVar][SerializeField] private int nbDeath = 0;
    [SyncVar][SerializeField] private int nbAssist = 0;
    [SyncVar][SerializeField] private int nbMinions = 0;
    public int NbKills { get => nbKills; set => nbKills = value; }
    public int NbDeath { get => nbDeath; set => nbDeath = value; }
    public int NbAssist { get => nbAssist; set => nbAssist = value; }
    public int NbMinions { get => nbMinions; set => nbMinions = value; }

    //inBattle
    protected bool inBattle = false;

    //Setters Getters
    //MOUVEMETNS
    public Transform getTarget() { return target; }
    public void setTarget(Transform target) { this.target = target; }

    //ATTACKS
    public void setIsAttack(bool v) { isAttack = v; inBattle = v; }
    public bool getIsAttack() { return isAttack; }
    public void setCanAuto(bool v) { canAuto = v; }
    public bool getCanAuto() { return canAuto; }

    //ITEMS
    public int getGolds() { return goldsOnStock; }
    public List<Item> getItems() { return items; }

    public void RpcTargetToNull() { target = null; }

    new protected void Awake()
    {
        base.Awake();
        mouvements = GetComponent<Mouvements>();
        animations = GetComponent<Animations>();

        //gameObject.tag = "Player";
    }

    new void Start()
    {
        base.Start();
        CmdLeveling();

        if (isLocalPlayer)
        {
            gui = GUIControleur.instance;
            shop = Shop.instance.gameObject;
            shop.SetActive(false);

            scoreboard = ScoreboardControleur.instance.gameObject;
            scoreboard.SetActive(false);
        }
    }

    // Update is called once per frame
    new protected void Update()
    {
        if (isLocalPlayer)
        {
            base.Update();
            Inputs();
            animations.changeAttackSpeed(Stats.attackSpeed.GetValue());

            CmdRegenerationVie();
            CmdRegenerationMana();
        }
    }

    private void LateUpdate()
    {
        updateGUI();
        if (isLocalPlayer)
        {
            gui.MiseAJour(this);
        }
    }

    [Client]
    private void updateGUI()
    {
        levelText.text = level.ToString();
        vieSlider.maxValue = Stats.vieMax.GetValue();
        vieSlider.value = vie;
        manaSlider.maxValue = Stats.manaMax.GetValue();
        manaSlider.value = mana;
    }

    [Client]
    //FONCTIONS USUELLES
    private void Inputs()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Characters")) && !inSameTeam(hit.collider.gameObject))
            {
                target = hit.collider.transform;
                mouvements.lookAt(hit.collider.transform.position);

                if (Vector3.Distance(transform.position, hit.collider.transform.position) <= Stats.range.GetValue() && canAuto) // && Time.time > nextAttackTime
                {
                    aaHit = hit.collider.transform;
                    CmdAutoAttaque();
                }
            }

        }
        else if (target != null && Vector3.Distance(transform.position, target.position) <= Stats.range.GetValue() && canAuto && !inSameTeam(target.gameObject)) // && Time.time > nextAttackTime
        {
            mouvements.lookAt(target.position);
            aaHit = target.transform;
            CmdAutoAttaque();

        }

        if (Input.GetMouseButtonDown(2))
        {
            CmdLeveling();
            CmdTakeDamage(100, 0);
            CmdGiveGolds(3100);
            NbKills++;
        }

        if (Input.GetButtonDown("Shop"))
        {
            UI_Shop.showShop(!UI_Shop.self.activeSelf);
        }

        if (shop.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            UI_Shop.showShop(false);
        }

        if (Input.GetButtonDown("QClick"))
        {
            AfficherRangeQClick();
            if (Input.GetMouseButtonDown(0)) { QClickAttack(); }
        }

        if (Input.GetKeyUp(KeyCode.A) && IR) { DesafficherRangeQClick(); }

        if(Input.GetKey(KeyCode.Tab))
        {
            scoreboard.SetActive(true);
        }
        else if(scoreboard.activeSelf)
        {
            scoreboard.SetActive(false);
        }
    }


    //FONCTIONS STATISTIQUES
    [Command]
    virtual protected void CmdRegenerationVie()
    {
        if (Time.time >= nextRegenPvTime)
        {
            nextRegenPvTime = Time.time + 1f/100;

            if (vie < Stats.vieMax.GetValue())
            {
                if (!inBattle)
                {
                    vie += Stats.vieRegen.GetValue()/100;
                }
                else
                {
                    vie += Stats.vieRegen.GetValue()/100 / 4;
                }

                if (vie > Stats.vieMax.GetValue())
                {
                    vie = Stats.vieMax.GetValue();
                }
            }
        }
    }

    [Command]
    virtual protected void CmdRegenerationMana()
    {
        if (Time.time >= nextRegenManaTime)
        {
            nextRegenManaTime = Time.time + 1f / 100;

            if (mana < Stats.manaMax.GetValue())
            {
                if (!inBattle)
                {
                    mana += Stats.manaRegen.GetValue() / 100;
                }
                else
                {
                    mana += Stats.manaRegen.GetValue() / 100 / 4;
                }

                if (mana > Stats.manaMax.GetValue())
                {
                    mana = Stats.manaMax.GetValue();
                }
            }
        }
    }

    //ITEMS
    [Command]
    public void CmdGiveGolds(int goldsGive)
    {
        goldsOnStock += goldsGive;
    }

    [Command]
    public void CmdAddItem(string key, int prix)
    {
        Item add = ItemsManager.instance.GetItem(key);
        if(add == null) { return; }

        if (prix <= goldsOnStock)
        {
            bool t = true;
            if (items.Count >= nbMaxItems)
            {
                t = false;
                foreach (Item item in add.Composents)
                {
                    if (items.Contains(item))
                    {
                        t = true;
                        break;
                    }
                    else
                    {
                        foreach (Item item2 in item.Composents)
                        {
                            if (items.Contains(item))
                            {
                                t = true;
                                break;
                            }
                        }

                        if (t) { break; }
                    }
                }
            }

            if (t)
            {
                goldsOnStock -= prix;

                foreach (Item item in add.Composents)
                {
                    if (items.Contains(item))
                    {
                        RpcRemoveAddItem(item.name, false);
                        RpcActualiserStatsSelonItemsNegatif(item.name);
                    }
                    else
                    {
                        foreach (Item i in item.Composents)
                        {
                            if (items.Contains(i))
                            {
                                RpcRemoveAddItem(i.name, false);
                                RpcActualiserStatsSelonItemsNegatif(i.name);
                            }
                        }
                    }
                }

                RpcRemoveAddItem(key, true);
                RpcActualiserStatsSelonItemsPositif(key);
            }
        }

    }

    [Command]
    private void CmdSellItem(string name)
    {
        Item remove = ItemsManager.instance.GetItem(name);
        if (items.Contains(remove))
        {
            goldsOnStock += Mathf.RoundToInt(remove.Prix * 0.7f);
            RpcRemoveAddItem(name, false);
            RpcActualiserStatsSelonItemsNegatif(name);
        }
    }

    [ClientRpc]
    private void RpcRemoveAddItem(string name, bool b)
    {
        Item i = ItemsManager.instance.GetItem(name);
        if (b)
        {
            items.Add(i);
        }
        else
        {
            items.Remove(i);
        }
    }

    [ClientRpc]
    private void RpcActualiserStatsSelonItemsPositif(string name)
    {
        Item item = ItemsManager.instance.GetItem(name);
        item.Stats.ForEach(x => Stats.GetStatFromItem(x.GetStat()).AddAlteration(x.GetValue()));
    }

    [ClientRpc]
    private void RpcActualiserStatsSelonItemsNegatif(string name)
    {
        Item item = ItemsManager.instance.GetItem(name);
        item.Stats.ForEach(x => Stats.GetStatFromItem(x.GetStat()).RemoveAlteration(x.GetValue()));
    }

    [Client]
    //PASSIFS
    public void ActiveOnHitPassifsItem(StatsManager target)
    {
        foreach(Item item in items)
        {
            foreach(var passif in item.Passifs)
            {
                if(passif is OnHitPassif)
                {
                    var pass = passif as OnHitPassif;
                    pass.itemInteraction(this, target);
                }
            }
        }
    }

    [Command]
    private void CmdAutoAttaque()
    {
        RpcAutoAttaque();
    }

    //ATTAQUES
    [ClientRpc]
    virtual protected void RpcAutoAttaque() 
    {
        isAttack = true;
        inBattle = true;
        AutoAttaque();
    }

    private void AfficherRangeQClick()
    {
        if (!IR)
        {
            foreach (Transform transform in IndicatorRange.transform)
            {
                transform.localScale = new Vector3(Stats.range.GetValue() * 2, Stats.range.GetValue() * 2, Stats.range.GetValue() * 2);
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

}
