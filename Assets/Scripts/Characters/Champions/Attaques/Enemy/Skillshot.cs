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
        if (send != GameManager.GetLocalPlayer()) { return; }
        if (!alreadyTouch.Contains(other))
        {
            if (!send.inSameTeam(other.gameObject))
            {
                StatsManager sm = GameManager.GetFromAll(other.name);
                ChampionControleur cc = GameManager.GetPlayer(send.name);

                if (sm && cc)
                {
                    sm.CmdTakeDamage(getValue(), ratioDamage, 0);

                    if (sm)
                    {
                        cc.ActiveOnHitPassifsItem(sm);

                        if (sm)
                        {
                            if (canSlow)
                            {
                                sm.CmdSetSlow(slowPourcent, slowTime);
                            }
                        }
                    }

                    alreadyTouch.Add(other);
                }
            }
        }
        
    }
}
