using Photon.Pun;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MantleOfChaos : Ability
{
    [Header("Stats")]
    [SerializeField] private float maxRange = 6.7f;
    [SerializeField] private float speed = 10f;

    [Header("Slow")]
    [SerializeField] private int slow;
    [SerializeField] private int slowTime;

    new void Awake()
    {
        base.Awake();

        if (championControleur.photonView.IsMine)
        {

            abilityImageIcon = GameObject.Find("Ability 1 Icon").GetComponent<Image>();
            abilityImageIconCD = GameObject.Find("Ability 1 Icon CD").GetComponent<Image>();
            abilityText = GameObject.Find("Ability 1 CD").GetComponent<Text>();
            canvas = GameObject.Find("Ability1IndicatorCanvas").GetComponent<Canvas>();
        }

    }

    new void Update()
    {
        base.Update();
        abilityCanvas();
    }

    new private void abilityCanvas()
    {
        if (canvas.enabled)
        {
            Cursor.visible = false;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Sol")))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }

            Quaternion ab1Canvas = Quaternion.LookRotation(position - transform.position);
            ab1Canvas.eulerAngles = new Vector3(0, ab1Canvas.eulerAngles.y, ab1Canvas.eulerAngles.z);
            canvas.transform.rotation = Quaternion.Lerp(ab1Canvas, canvas.transform.rotation, 0);

            if (Input.GetKeyUp(key))
            {
                mouvements.moveToPosition(transform.position);
                championControleur.photonView.RPC("launchMantleOfChaos", RpcTarget.All, position);
                activeCD();
            }
        }
    }

    [PunRPC]
    protected void launchMantleOfChaos(Vector3 pos)
    {
        mouvements.lookAt(pos);
        launchAbility();
    }

    new private void launchAbility()
    {
        GameObject mantleOfChaos = Instantiate(abilityVisuel, spawnPoint.transform.position, spawnPoint.transform.rotation);
        Skillshot principalClass = mantleOfChaos.GetComponent<Skillshot>();
        principalClass.send = gameObject;
        principalClass.player = championControleur.photonView.Owner;
        principalClass.speed = speed;
        principalClass.firstHit = false;
        principalClass.origin = spawnPoint.transform.position;
        principalClass.maxDistance = maxRange;
        principalClass.canSlow = true;
        principalClass.slowPourcent = slow;
        principalClass.slowTime = slowTime;
        principalClass.ratioDamage = ratioDamage;
        principalClass.setValue(getValueWithRatios());

        //if (principalClass != null)
        //{
        //    var damage = getValueWithRatios();

        //    if (Random.Range(0, 101) <= championControleur.getCritChance())
        //    {
        //        principalClass.setValue(damage * (championControleur.getCritDamage() / 100f));
        //    }
        //    else
        //    {
        //        principalClass.setValue(damage);
        //    }
        //}
    }




}
