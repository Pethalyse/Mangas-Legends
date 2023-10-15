using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LifeEssence : Ability
{
    [Header("Stats")]
    [SerializeField] float duration3 = 3;

    new void Awake()
    {
        base.Awake();

        if (championControleur.photonView.IsMine)
        {
            abilityImageIcon = GameObject.Find("Ability 3 Icon").GetComponent<Image>();
            abilityImageIconCD = GameObject.Find("Ability 3 Icon CD").GetComponent<Image>();
            abilityText = GameObject.Find("Ability 3 CD").GetComponent<Text>();
            canvas = GameObject.Find("Ability3IndicatorCanvas").GetComponent<Canvas>();
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
            if (Input.GetKeyUp(key))
            {
                championControleur.photonView.RPC("launchLifeEssence", RpcTarget.All);
                activeCD();
            }
        }
    }

    [PunRPC]
    protected void launchLifeEssence()
    {
        launchAbility();
    }

    new private void launchAbility()
    {
        GameObject lifeEssence = Instantiate(abilityVisuel, gameObject.transform);
        HealCircle principalClass = lifeEssence.GetComponent<HealCircle>();
        principalClass.player = championControleur.photonView.Owner;
        principalClass.send = gameObject;
        principalClass.duration = duration3;
        principalClass.reference = gameObject;
        var heal = getValueWithRatios();
        principalClass.setValue(heal);
    }
}
