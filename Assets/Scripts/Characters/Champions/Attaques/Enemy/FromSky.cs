using static UnityEngine.UI.Image;
using UnityEngine;
using Photon.Pun;

public class FromSky : AbilityControleur
{
    public float speed;

    private void Update()
    {
        gameObject.transform.TransformDirection(Vector3.down);
        gameObject.transform.Translate(new Vector3(0, -(speed * Time.deltaTime), 0));

        if (transform.position.y <= 0)
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
                    }

                    alreadyTouch.Add(other);
                }
            }
        }
        
    }

}