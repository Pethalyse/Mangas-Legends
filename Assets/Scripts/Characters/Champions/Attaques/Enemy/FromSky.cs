using static UnityEngine.UI.Image;
using UnityEngine;

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

        if (!alreadyTouch.Contains(other))
        {
            if (!inSameTeam(other.gameObject))
            {
                var sm = other.gameObject.GetComponent<StatsManager>();
                if (sm)
                {
                    sm.TakeDamage(getValue(), ratioDamage, send);

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