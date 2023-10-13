using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "MangaLegends/Item")]
public class Item : ScriptableObject
{
    [Header("Texts")]
    public string nom;

    [Header("Visual")]
    public Sprite icone;

    [Header("OnHit")]
    public bool onHitEffect = false;

    [Header("Vie")]
    public int vie;

    [Header("AD")]
    public int ad;
    public int lethality;
    public int peneArmor;

    [Header("AP")]
    public int ap;
    public int peneMagic;

    [Header("Crit")]
    public int critChance;
    public int critDamage;

    [Header("Armor")]
    public int armor;

    [Header("MagicRes")]
    public int magicRes;

    [Header("AS")]
    public int aSpd;

    [Header("Mana")]
    public int mana;
    public int regenMana;

    [Header("Support")]
    public int bonusHeal;

    [Header("CDR")]
    public int cdr;

    [Header("MS")]
    public int ms;

    [Header("Life Steal")]
    public int lifeSteal;

    [Header("Prix")]
    public int prix;

    [Header("Composents")]
    public bool unique;
    public List<Item> composents;

    [Header("Passif/Actifs")]
    public List<Passif> passifs;
    public List<Actif> actifs;

    public int getPrix()
    {
        int p = prix;

        foreach(Item item in composents)
        {
            p += item.getPrix();
        }

        return p;
    }
}
