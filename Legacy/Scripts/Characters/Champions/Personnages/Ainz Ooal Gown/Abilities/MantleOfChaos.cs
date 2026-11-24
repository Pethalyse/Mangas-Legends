using Mirror;
using System.Collections.Generic;
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

        if (isLocalPlayer)
        {

            abilityImageIcon = GameObject.Find("Ability 1 Icon").GetComponent<Image>();
            abilityImageIconCD = GameObject.Find("Ability 1 Icon CD").GetComponent<Image>();
            abilityText = GameObject.Find("Ability 1 CD").GetComponent<Text>();
            canvas = GameObject.Find("Ability1IndicatorCanvas").GetComponent<Canvas>();
        }

    }

    new void Start()
    {
        List<Behaviour> b = GUIControleur.instance.GetAbGui(1);

        abilityImageIcon = (Image)b[0];
        abilityImageIconCD = (Image)b[1];
        abilityText = (Text)b[2];

        base.Start();

        key = "Ab1";
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

            if (Input.GetButtonUp(key))
            {
                mouvements.moveToPosition(transform.position);
                CmdLaunchMantleOfChaos(position);
                activeCD();
            }
        }
    }

    [Command]
    private void CmdLaunchMantleOfChaos(Vector3 pos)
    {
        RpcLaunchMantleOfChaos(pos);
    }

    [ClientRpc]
    private void RpcLaunchMantleOfChaos(Vector3 pos)
    {
        mouvements.lookAt(pos);
        launchAbility();
    }

    new private void launchAbility()
    {
        GameObject mantleOfChaos = Instantiate(abilityVisuel, spawnPoint.transform.position, spawnPoint.transform.rotation);
        Skillshot principalClass = mantleOfChaos.GetComponent<Skillshot>();
        principalClass.send = championControleur;
        principalClass.speed = speed;
        principalClass.firstHit = false;
        principalClass.origin = spawnPoint.transform.position;
        principalClass.maxDistance = maxRange;
        principalClass.canSlow = true;
        principalClass.slowPourcent = slow;
        principalClass.slowTime = slowTime;
        principalClass.ratioDamage = ratioDamage;
        principalClass.setValue(getValueWithRatios());
        setValue(principalClass);
    }




}
