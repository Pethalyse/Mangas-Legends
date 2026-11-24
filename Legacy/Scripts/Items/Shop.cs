using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public static Shop instance;
    public ChampionControleur player;
    public List<Item> items = new List<Item>();
    public List<Item> itemsMages = new List<Item>();
    public List<Item> itemsAssassins = new List<Item>();
    public List<Item> itemsBruisers = new List<Item>();
    public List<Item> itemsTireurs = new List<Item>();
    public List<Item> itemsSupports = new List<Item>();
    public List<Item> itemsTanks = new List<Item>();

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private void Update()
    {
        if (!player)
        {
            player = GameManager.GetLocalPlayer();
        }
    }

    public void AcheterObjet(Item objet)
    {
        int prix = objet.GetPrix();
        foreach (Item i in objet.Composents)
        {

            if (player.getItems().Contains(i))
            {
                prix -= i.GetPrix();
            }
            else
            {
                foreach (Item i2 in i.Composents)
                {
                    if (player.getItems().Contains(i2))
                    {
                        prix -= i2.GetPrix();
                    }
                }
            }
        }
        
        player.CmdAddItem(objet.name, prix);
    }

    public static void TrierParPrixCroissant(   List<Item> items)
    {
        items.Sort((x, y) => x.GetPrix().CompareTo(y.GetPrix()));
    }

    public static void TrierParPrixDecroissant(List<Item> items)
    {
        items.Sort((x, y) => y.GetPrix().CompareTo(x.GetPrix()));
    }

    public void initItems()
    {
        // Charger les ScriptableObjects d'objets uniques
        Item[] unique = Resources.LoadAll<Item>("Items/Uniques");
        items.AddRange(unique);

        // Charger les ScriptableObjects d'objets secondaires
        Item[] secondary = Resources.LoadAll<Item>("Items/Secondaires");
        items.AddRange(secondary);

        // Charger les ScriptableObjects d'objets tertiaires
        Item[] terciary = Resources.LoadAll<Item>("Items/Terciaires");
        items.AddRange(terciary);

        foreach (Item item in items)
        {
            if (item.CompareStat(Stats.AP, 0, 1))
            {
                itemsMages.Add(item);
            }

            if (item.CompareStat(Stats.AD, 0, 1) && item.CompareStat(Stats.Lethality, 0, 0) && item.CompareStat(Stats.CritChance, 0, 0) && item.CompareStat(Stats.CritDamage, 0, 0))
            {
                itemsBruisers.Add(item);
            }

            if (item.CompareStat(Stats.ASpd, 0, 1) || item.CompareStat(Stats.CritChance, 0, 1) || item.CompareStat(Stats.CritDamage, 0, 1))
            {
                itemsTireurs.Add(item);
            }

            if (item.CompareStat(Stats.Lethality, 0, 1) || item.CompareStat(Stats.PeneArmor, 0, 1))
            {
                itemsAssassins.Add(item);
            }

            if (item.CompareStat(Stats.BonusHeal, 0, 1) || item.CompareStat(Stats.RegenMana, 0, 1))
            {
                itemsSupports.Add(item);
            }

            if (item.CompareStat(Stats.Armor,0,1) || item.CompareStat(Stats.MagicRes,0,1) || (item.CompareStat(Stats.Vie,0,1) && item.CompareStat(Stats.AD,0,0) && item.CompareStat(Stats.AP,0,0)))
            {
                itemsTanks.Add(item);
            }
        }
    }
}
