using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "[Item]", menuName = "MangaLegends/Item")]
public class Item : ScriptableObject
{
    [Header("Texts")]
    [SerializeField]
    private string nom;
    public string Nom { get => nom;}

    [Header("Sprite")]
    [SerializeField]
    private Sprite icone;
    public Sprite Icone { get => icone;}

    [Header("OnHit")]
    [SerializeField]
    private bool onHitEffect = false;
    public bool OnHitEffect { get => onHitEffect;}
    
    [Header("Stat")]
    [SerializeField]
    private List<StatItem> stats = new List<StatItem>();
    public List<StatItem> Stats { get => stats;}
    
    [Header("Prix")]
    [SerializeField]
    private int prix;
    public int Prix { get => prix;}  

    [Header("Composents")]
    [SerializeField]
    private bool unique;
    public bool Unique { get => unique; }

    [SerializeField]
    private List<Item> composents;
    public List<Item> Composents { get => composents; }

    [Header("Passif/Actifs")]
    [SerializeField]
    private List<Passif> passifs;
    public List<Passif> Passifs { get => passifs; }

    [SerializeField]
    private List<Actif> actifs;
    public List<Actif> Actifs { get => actifs; }

    public int GetPrix()
    {
        int p = Prix;

        foreach(Item item in Composents)
        {
            p += item.GetPrix();
        }

        return p;
    }

    public StatItem GetStat(Stats stat)
    {
        foreach(StatItem s in Stats)
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

    public override bool Equals(object obj)
    {
        return obj is Item item &&
               base.Equals(obj) &&
               name == item.name &&
               hideFlags == item.hideFlags &&
               nom == item.nom &&
               OnHitEffect == item.OnHitEffect &&
               EqualityComparer<List<StatItem>>.Default.Equals(Stats, item.Stats) &&
               Prix == item.Prix &&
               Unique == item.Unique &&
               EqualityComparer<List<Item>>.Default.Equals(Composents, item.Composents) &&
               EqualityComparer<List<Passif>>.Default.Equals(Passifs, item.Passifs) &&
               EqualityComparer<List<Actif>>.Default.Equals(Actifs, item.Actifs);
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(name);
        hash.Add(hideFlags);
        hash.Add(nom);
        hash.Add(OnHitEffect);
        hash.Add(Stats);
        hash.Add(Prix);
        hash.Add(Unique);
        hash.Add(Composents);
        hash.Add(Passifs);
        hash.Add(Actifs);
        return hash.ToHashCode();
    }
}
