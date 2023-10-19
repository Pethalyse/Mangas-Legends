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
        int prix = objet.getPrix();
        foreach (Item i in objet.composents)
        {

            if (player.getItems().Contains(i))
            {
                prix -= i.getPrix();
            }
            else
            {
                foreach (Item i2 in i.composents)
                {
                    if (player.getItems().Contains(i2))
                    {
                        prix -= i2.getPrix();
                    }
                }
            }
        }
        
        player.CmdAddItem(objet, prix);
    }

    public static void TrierParPrixCroissant(List<Item> items)
    {
        items.Sort((x, y) => x.getPrix().CompareTo(y.getPrix()));
    }

    public static void TrierParPrixDecroissant(List<Item> items)
    {
        items.Sort((x, y) => y.getPrix().CompareTo(x.getPrix()));
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
            if (item.ap > 0)
            {
                itemsMages.Add(item);
            }

            if (item.ad > 0 && item.lethality == 0 && item.critChance == 0 && item.critDamage == 0)
            {
                itemsBruisers.Add(item);
            }

            if (item.aSpd > 0 || item.critChance > 0 || item.critDamage > 0)
            {
                itemsTireurs.Add(item);
            }

            if (item.lethality > 0 || item.peneArmor > 0)
            {
                itemsAssassins.Add(item);
            }

            if (item.bonusHeal > 0 || item.regenMana > 0)
            {
                itemsSupports.Add(item);
            }

            if (item.armor > 0 || item.magicRes > 0 || (item.vie > 0 && item.ad == 0 && item.ap == 0))
            {
                itemsTanks.Add(item);
            }
        }
    }
}
