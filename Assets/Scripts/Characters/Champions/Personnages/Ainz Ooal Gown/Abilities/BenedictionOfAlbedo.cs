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

        if (championControleur.photonView.IsMine)
        {
            abilityImageIcon = GameObject.Find("Passif Icon").GetComponent<Image>();
            abilityImageIconCD = GameObject.Find("Passif Icon CD").GetComponent<Image>();
            abilityText = GameObject.Find("Passif CD").GetComponent<Text>();
        }

    }
}
