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
        if (send != GameManager.GetLocalPlayer()) { return; }

        if (!alreadyTouch.Contains(other))
        {
            if (!send.inSameTeam(other.gameObject))
            {
                StatsManager sm = GameManager.GetFromAll(other.name);
                ChampionControleur cc = GameManager.GetPlayer(send.name);
                if (sm && cc)
                {
                    sm.CmdTakeDamage(getValue(), ratioDamage);
                    if (!sm) { return; }
                    cc.ActiveOnHitPassifsItem(sm);
                    alreadyTouch.Add(other);
                }
            }
        }
        
    }

}