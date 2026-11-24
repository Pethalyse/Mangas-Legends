using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeEssence : Ability
{
    [Header("Stats")]
    [SerializeField] float duration3 = 3;

    new void Awake()
    {
        base.Awake();

        if (isLocalPlayer)
        {
            abilityImageIcon = GameObject.Find("Ability 3 Icon").GetComponent<Image>();
            abilityImageIconCD = GameObject.Find("Ability 3 Icon CD").GetComponent<Image>();
            abilityText = GameObject.Find("Ability 3 CD").GetComponent<Text>();
            canvas = GameObject.Find("Ability3IndicatorCanvas").GetComponent<Canvas>();
        }


    }

    new void Start()
    {
        List<Behaviour> b = GUIControleur.instance.GetAbGui(3);

        abilityImageIcon = (Image)b[0];
        abilityImageIconCD = (Image)b[1];
        abilityText = (Text)b[2];

        base.Start();

        key = "Ab3";
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
            if (Input.GetButtonUp(key))
            {
                CmdLaunchLifeEssence();
                activeCD();
            }
        }
    }

    [Command]
    protected void CmdLaunchLifeEssence()
    {
        RpcLaunchLifeEssence();
    }

    [ClientRpc]
    private void RpcLaunchLifeEssence()
    {
        launchAbility();
    }

    new private void launchAbility()
    {
        GameObject lifeEssence = Instantiate(abilityVisuel, gameObject.transform);
        HealCircle principalClass = lifeEssence.GetComponent<HealCircle>();
        principalClass.send = championControleur;
        principalClass.duration = duration3;
        principalClass.reference = gameObject;
        var heal = getValueWithRatios();
        principalClass.setValue(heal);
    }
}
