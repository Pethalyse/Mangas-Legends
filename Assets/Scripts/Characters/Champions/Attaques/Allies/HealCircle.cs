using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class HealCircle : BuffAllies
{

    public float duration;
    public GameObject reference;
    public Vector3 position;

    private float time;

    private void Start()
    {
        StartCoroutine(timeLimite());
    }

    private void Update()
    {
        if(reference != null)
        {
            gameObject.transform.position = new Vector3(reference.transform.position.x, 0.1f, reference.transform.position.z);
        }
        else if(position != null)
        {
            gameObject.transform.position=position;
        }

        if(time - Time.time <= 0 )
        {
            time = Time.time + 1;
            alreadyTouch.Clear();
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (player == null || !player.IsLocal) { return; }
        if (!alreadyTouch.Contains(other))
        {
            if (TeamManager.the2InSameTeam(send, other.gameObject))
            {
                other.gameObject.GetComponent<StatsManager>()?.RPC_TakeHeal(getValue());
                alreadyTouch.Add(other);
            }
        }
        
    }

    private IEnumerator timeLimite()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
 
}
