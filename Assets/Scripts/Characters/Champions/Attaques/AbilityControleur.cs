using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AbilityControleur: MonoBehaviourPun
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

    //Player
    public Player player;
    public GameObject send;

}