using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InfinityWall : Ability
{

    [Header("Stats")]
    [SerializeField] private float maxRange = 4f;
    [SerializeField] private float duration;

    new void Awake()
    {
        base.Awake();

        if (championControleur.photonView.IsMine)
        {
            abilityImageIcon = GameObject.Find("Ability 2 Icon").GetComponent<Image>();
            abilityImageIconCD = GameObject.Find("Ability 2 Icon CD").GetComponent<Image>();
            abilityText = GameObject.Find("Ability 2 CD").GetComponent<Text>();
            canvas = GameObject.Find("Ability2IndicatorCanvas").GetComponent<Canvas>();
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

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Characters")) && (Input.GetMouseButtonUp(0) || Input.GetKeyUp(key)))
            {
                if(!championControleur.inSameTeam(hit.collider.gameObject)) { return; }
                
                if (Vector3.Distance(transform.position, hit.collider.transform.position) <= maxRange)
                {
                    championControleur.photonView.RPC("launchInfinityWall", RpcTarget.All, hit.collider.GetComponent<ChampionControleur>().photonView.ViewID);
                    activeCD();
                }
                else
                {
                    championControleur.setTarget(hit.collider.transform);
                    mouvements.moveToObject(hit.collider.gameObject, maxRange);
                }

            }

            if (championControleur.getTarget() && championControleur.inSameTeam(championControleur.getTarget().gameObject))
            {
                if (Vector3.Distance(transform.position, championControleur.getTarget().transform.position) <= maxRange)
                {

                    championControleur.photonView.RPC("launchInfinityWall", RpcTarget.All, championControleur.getTarget().GetComponent<ChampionControleur>().photonView.ViewID);
                    activeCD();
                    championControleur.targetToNull();
                }
            }

            if (Input.GetKeyUp(key) && (!championControleur.getTarget() || !championControleur.inSameTeam(championControleur.getTarget().gameObject)))
            {
                canvas.enabled = false;
                Cursor.visible = true;
            }
        }
    }

    [PunRPC]
    protected void launchInfinityWall(int target)
    { 
        launchAbility(PhotonView.Find(target).gameObject);
    }

    private void launchAbility(GameObject target) 
    {
        ShieldAP sa = target.AddComponent<ShieldAP>();
        sa.setAntiCC();
        sa.setShield((int) Mathf.Round(getValueWithRatios()));
        sa.setTime(duration);
    }

}
