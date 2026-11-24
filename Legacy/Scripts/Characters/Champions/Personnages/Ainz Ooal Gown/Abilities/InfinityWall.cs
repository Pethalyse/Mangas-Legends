using Mirror;
using System.Collections.Generic;
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

        if (isLocalPlayer)
        {
            abilityImageIcon = GameObject.Find("Ability 2 Icon").GetComponent<Image>();
            abilityImageIconCD = GameObject.Find("Ability 2 Icon CD").GetComponent<Image>();
            abilityText = GameObject.Find("Ability 2 CD").GetComponent<Text>();
            canvas = GameObject.Find("Ability2IndicatorCanvas").GetComponent<Canvas>();
        }

    }

    new void Start()
    {
        List<Behaviour> b = GUIControleur.instance.GetAbGui(2);

        abilityImageIcon = (Image)b[0];
        abilityImageIconCD = (Image)b[1];
        abilityText = (Text)b[2];

        base.Start();

        key = "Ab2";
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

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Characters")) && (Input.GetMouseButtonUp(0) || Input.GetButtonUp(key)))
            {
                if (!championControleur.inSameTeam(hit.collider.gameObject)) { return; }

                if (Vector3.Distance(transform.position, hit.collider.transform.position) <= maxRange)
                {
                    CmdlaunchInfinityWall(hit.collider.gameObject);
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

                    CmdlaunchInfinityWall(championControleur.getTarget().gameObject);
                    activeCD();
                    championControleur.RpcTargetToNull();
                }
            }

            if (Input.GetButtonUp(key) && (!championControleur.getTarget() || !championControleur.inSameTeam(championControleur.getTarget().gameObject)))
            {
                canvas.enabled = false;
                Cursor.visible = true;
            }
        }
    }

    [Command]
    private void CmdlaunchInfinityWall(GameObject target)
    {
        RpcLaunchInfinityWall(target);
    }

    [ClientRpc]
    private void RpcLaunchInfinityWall(GameObject target)
    {
        ShieldAP sa = target.AddComponent<ShieldAP>();
        sa.setAntiCC();
        sa.setShield((int)Mathf.Round(getValueWithRatios()));
        sa.setTime(duration);
    }

}
