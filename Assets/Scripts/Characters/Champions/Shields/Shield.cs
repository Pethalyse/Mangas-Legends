using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shield : MonoBehaviour
{
    [Header("Shield")]
    [SerializeField] private int value;
    [SerializeField] private float time;
    private bool antiCC;
    private bool already = false;

    private ChampionControleur championControleur;

    private void Awake()
    {
        championControleur = GetComponent<ChampionControleur>();
    }

    private void Update()
    {

        if(value != 0 && time != 0)
        {
            if(!already)
            {
                StartCoroutine(delayDestruct());
            }

            if (antiCC)
            {
                championControleur.setCantBeCC(true);
            }
        }
    }

    private IEnumerator delayDestruct()
    {
        already = true;
        yield return new WaitForSeconds(time);
        destruction();
    }

    private void destruction()
    {
        championControleur.setCantBeCC(false);
        Destroy(this);
    }

    public int getShield() { return value; }
    public void setShield(int value)
    { 
        this.value = value;
        if (value <= 0)
        {
            destruction();
        }
    }

    public void setTime(float time) { this.time = time; }
    public void setAntiCC() { antiCC = true; }
}
