using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class StatCharacter
{
    [SerializeField] private float baseValue;
    [SerializeField] private float valueLeveling;
    [SerializeField] private bool isPercent;

    [SerializeField] private List<float> alterations = new List<float>();

    public float GetValue()
    {
        float value = baseValue;

        if(isPercent)
        {
            alterations.ForEach(x => value += baseValue * x / 100);
        }
        else
        {
            alterations.ForEach(x => value += x);
        }
        
        return value;

    }

    public float GetBaseValue() { return  baseValue; }
    public float GetValueLeveling() {  return valueLeveling; }

    public void AddAlteration(float value)
    {
        if (value == 0)
            return;

        alterations.Add(value);
    }

    public void RemoveAlteration(float value)
    {
        if (value == 0)
            return;

        alterations.Remove(value);
    }

    public void LevelUp()
    {
        AddAlteration(valueLeveling);
    }
}
