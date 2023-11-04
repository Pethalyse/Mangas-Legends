using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "[StatsCharacter]", menuName = "ML - New Character Stats", order = 1)]
[Serializable]
public class StatsCharacterList : ScriptableObject
{
    [SerializeField]
    [SyncVar]
    public StatCharacter vieMax, vieRegen, manaMax, manaRegen, moveSpeed, ad, ap, ar, mr, critChance, critDamage, range, attackSpeed, lethality, peneArmor, peneMagic, lifeSteal, bonusHeal, cdr;

    public StatCharacter GetStatFromItem(Stats stat)
    {
        switch(stat)
        {
            case Stats.Vie: return vieMax;
            case Stats.Mana: return manaMax;
            case Stats.MS: return moveSpeed;
            case Stats.AD: return ad;
            case Stats.AP: return ap;
            case Stats.Armor: return ar;
            case Stats.MagicRes: return mr;
            case Stats.CritChance: return critChance;
            case Stats.CritDamage: return critDamage;
            case Stats.Range: return range;
            case Stats.ASpd: return attackSpeed;
            case Stats.Lethality: return lethality;
            case Stats.PeneArmor: return peneArmor;
            case Stats.PeneMagic: return peneMagic;
            case Stats.LifeSteal: return lifeSteal;
            case Stats.BonusHeal: return bonusHeal;
            case Stats.Cdr: return cdr;

            default: return null;
        }
    }

    public void LevelUp()
    {
        vieMax.LevelUp();
        vieRegen.LevelUp();
        manaMax.LevelUp();
        manaRegen.LevelUp();
        moveSpeed.LevelUp();
        ad.LevelUp();
        ap.LevelUp();
        ar.LevelUp();
        mr.LevelUp();
        critChance.LevelUp();
        critDamage.LevelUp();
        range.LevelUp();
        attackSpeed.LevelUp();
        lethality.LevelUp();
        peneArmor.LevelUp();
        peneMagic.LevelUp();
        lifeSteal.LevelUp();
        bonusHeal.LevelUp();
        cdr.LevelUp();
    }
}
