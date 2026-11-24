using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PenetrateUp : Ability
{
    [Header("GUI")]
    [SerializeField] private Image canvasIndicator;

    [Header("Stats")]
    [SerializeField] private float maxRange = 22.5f;
    [SerializeField] private float speed = 5f;


    new void Awake()
    {
        base.Awake();

        if (isLocalPlayer)
        {
            abilityImageIcon = GameObject.Find("Ability 4 Icon").GetComponent<Image>();
            abilityImageIconCD = GameObject.Find("Ability 4 Icon CD").GetComponent<Image>();
            abilityText = GameObject.Find("Ability 4 CD").GetComponent<Text>();
            canvas = GameObject.Find("Ability4IndicatorCanvas").GetComponent<Canvas>();
            canvasIndicator = GameObject.Find("Indicator4").GetComponent<Image>();
        }

    }

    new void Start()
    {
        List<Behaviour> b = GUIControleur.instance.GetAbGui(4);

        abilityImageIcon = (Image)b[0];
        abilityImageIconCD = (Image)b[1];
        abilityText = (Text)b[2];

        base.Start();

        key = "Ab4";
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
                position = hit.point;
            }

            var hitPosDir = (hit.point - transform.position).normalized;
            float distance = Vector3.Distance(hit.point, transform.position);
            distance = Mathf.Min(distance, maxRange);

            var newHitPos = transform.position + hitPosDir * distance;
            canvasIndicator.transform.position = newHitPos;
            canvasIndicator.transform.position = new Vector3(canvasIndicator.transform.position.x, 0.1f, canvasIndicator.transform.position.z);


            if (Input.GetButtonUp(key))
            {
                CmdLaunchPenetrateUp(new Vector3(hit.point.x, 30, hit.point.z));
                activeCD();
                Cursor.visible = true;
            }
        }

    }

    [Command]
    protected void CmdLaunchPenetrateUp(Vector3 pos)
    {
        RpcLaunchPenetrateUp(pos);
    }

    [ClientRpc]
    protected void RpcLaunchPenetrateUp(Vector3 pos)
    {
        launchAbility(pos);
    }

    protected void launchAbility(Vector3 pos)
    {
        GameObject penetrateUp = Instantiate(abilityVisuel, pos, gameObject.transform.rotation);
        FromSky principalClass = penetrateUp.GetComponent<FromSky>();
        principalClass.send = championControleur;
        principalClass.speed = speed;
        principalClass.ratioDamage = ratioDamage;
        principalClass.setValue(getValueWithRatios());
    }
}
