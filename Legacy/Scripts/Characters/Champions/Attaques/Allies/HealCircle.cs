
using System.Collections;
using UnityEngine;

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
        if(send != GameManager.GetLocalPlayer()) { return; }
        if (!alreadyTouch.Contains(other))
        {
            if (send.inSameTeam(other.gameObject))
            {
                ChampionControleur sm = GameManager.GetPlayer(other.name);
                if (!sm) { return; }
                sm.CmdTakeHeal(getValue());
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
