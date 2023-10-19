using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class AbilityControleur: MonoBehaviour
{
    //Values
    private float value;
    public float getValue() { return value; }
    public void setValue(float value) { this.value = value; }

    //Ratio
    public int ratioDamage;

    //Tags
    protected List<Collider> alreadyTouch = new List<Collider>();

    //Slow
    public bool canSlow;
    public int slowPourcent;
    public float slowTime;

    public ChampionControleur send;

}