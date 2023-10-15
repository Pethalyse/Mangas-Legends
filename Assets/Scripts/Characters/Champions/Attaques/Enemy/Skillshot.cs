using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Skillshot : AbilityControleur
{
    public float speed;
    public bool firstHit;
    public Vector3 origin;
    public float maxDistance;

    private void Start()
    {

    }

    private void Update()
    {
        gameObject.transform.TransformDirection(Vector3.forward);
        gameObject.transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));

        if(Vector3.Distance(gameObject.transform.position, origin) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (player == null || !player.IsLocal) { return; }
        if (!alreadyTouch.Contains(other))
        {
            if (!TeamManager.the2InSameTeam(send, other.gameObject))
            {
                var sm = other.gameObject.GetComponent<StatsManager>();
                if (sm)
                {
                    sm.RPC_TakeDamage(getValue(), ratioDamage, send.GetPhotonView().ViewID);

                    if (sm)
                    {
                        send.GetComponent<ChampionControleur>()?.ActiveOnHitPassifsItem(sm);

                        if (sm)
                        {
                            if (canSlow)
                            {
                                sm.RPC_SetSlow(slowPourcent, slowTime);
                            }
                        }
                    }

                    alreadyTouch.Add(other);
                }
            }
        }
        
    }
}
