using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BenedictionDeAlbedo : Ability
{
    //PASSIF DANS LA CLASS CONTROLEUR DE AINZ

    new void Awake()
    {
        base.Awake();

        if (isLocalPlayer)
        {
            abilityImageIcon = GameObject.Find("Passif Icon").GetComponent<Image>();
            abilityImageIconCD = GameObject.Find("Passif Icon CD").GetComponent<Image>();
            abilityText = GameObject.Find("Passif CD").GetComponent<Text>();
        }

    }

    new void Start()
    {
        List<Behaviour> b = GUIControleur.instance.GetAbGui(0);

        abilityImageIcon = (Image)b[0];
        abilityImageIconCD = (Image)b[1];
        abilityText = (Text)b[2];

        base.Start();
    }
}
