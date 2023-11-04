using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "[Item]", menuName = "MangaLegends/Item")]
public class Item : ScriptableObject
{
    [Header("Texts")]
    public string nom;

    [Header("Visual")]
    public Sprite icone;

    [Header("OnHit")]
    public bool onHitEffect = false;

    [Header("Stat")]
    public List<StatItem> stats = new List<StatItem>();

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

    public StatItem GetStat(Stats stat)
    {
        foreach(StatItem s in stats)
        {
            if(s.GetStat().Equals(stat)) return s;
        }

        return null;
    }

    public bool CompareStat(Stats stat, int value, int comparator)
    {
        if (GetStat(stat) is null)
        {
            return false;
        }
        else
        {
            switch (comparator)
            {
                case 0: return GetStat(stat).GetValue() == value;
                case 1: return GetStat(stat).GetValue() > value;
                case 2: return GetStat(stat).GetValue() < value;
                default: return false;
            }
            
        }
    }
}
