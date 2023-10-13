using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AinzOoalGownAnimations : Animations
{
    [SerializeField] private Avatar mageAvatarAA;

    new private void Start()
    {
        base.Start();

        if(mageAvatarAA != null)
        {
            avatarAA = mageAvatarAA;
        }
        else
        {
            Debug.Log(gameObject.name + ": Avatar AA Mage missing");
        }

    }
}
