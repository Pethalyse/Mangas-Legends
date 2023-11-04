using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatItem
{
    [SerializeField] private Stats stat;
    [SerializeField] private int valeur;

    public int GetValue() { return valeur;}

    public Stats GetStat() { return stat;}

    public override string ToString()
    {
        return stat.ToString() + ":<color=#FFD700>" + valeur + "</color>";
    }
}
