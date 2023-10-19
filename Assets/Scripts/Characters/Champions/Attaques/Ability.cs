using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Ability : NetworkBehaviour
{

    [Header("GUI")]
    [SerializeField] protected Sprite abilityTexture;
    [SerializeField] protected Image abilityImageIcon;
    [SerializeField] protected Image abilityImageIconCD;
    [SerializeField] protected Text abilityText;

    [Header("GUI player")]
    [SerializeField] protected Canvas canvas;

    [Header("Visuel")]
    [SerializeField] protected GameObject abilityVisuel;

    [Header("CD")]
    [SerializeField] private float abilityCD;
    private bool isAbilityCD = false;
    private float currentAbilityCD;

    [Header("Values")]
    [SerializeField] private int baseValueAbility;
    [SerializeField] private int[] ratiosAbility;
    [SerializeField] protected int ratioDamage;

    //components
    [Header("Components")]
    [SerializeField] protected GameObject spawnPoint;
    protected ChampionControleur championControleur;
    protected Mouvements mouvements;

    [Header("Other Ability")]
    [SerializeField] private Ability[] abilities;

    //ray
    protected Vector3 position;
    protected RaycastHit hit;
    protected Ray ray;

    protected string key;

    protected void Awake()
    {
        championControleur = GetComponent<ChampionControleur>();
        mouvements = GetComponent<Mouvements>();
    }

    protected void Start()
    {
        if (isLocalPlayer)
        {
            abilityImageIconCD.fillAmount = 0;
            abilityText.text = "";
            abilityImageIcon.sprite = abilityTexture;
            abilityImageIconCD.sprite = abilityTexture;
        }

        if (canvas) { canvas.enabled = false; }
    }

    virtual protected void Update()
    {
        if (isLocalPlayer)
        {
            if (canvas)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                input();
                cancelAbility();
                gestionCD();
            }
        } 
        
    }

    virtual protected void abilityCanvas() { }
    virtual protected void launchAbility() { }

    private void input()
    {
        if (key == "") { return; }
        if (Input.GetButtonDown(key) && !isAbilityCD)
        {
            canvas.enabled = true;

            foreach(Ability ab in abilities)
            {
                ab.canvasDisable();
            }
        }
    }

    private void canvasDisable()
    {
        canvas.enabled = false;
    }

    private void cancelAbility()
    {
        if (Input.GetMouseButtonDown(1) && !isAbilityCD)
        {
            canvas.enabled = false;
            Cursor.visible = true;
        }
    }

    protected void activeCD()
    {
        isAbilityCD = true;
        currentAbilityCD = abilityCD;

        canvas.enabled = false;

        Cursor.visible = true;
    }

    private void gestionCD()
    {
        if (isAbilityCD)
        {
            currentAbilityCD -= Time.deltaTime;

            if (currentAbilityCD <= 0f)
            {
                isAbilityCD = false;
                currentAbilityCD = 0f;

                if (abilityImageIconCD != null)
                {
                    abilityImageIconCD.fillAmount = 0f;
                }

                if (abilityText != null)
                {
                    abilityText.text = "";
                }
            }
            else
            {
                if (abilityImageIconCD != null)
                {
                    abilityImageIconCD.fillAmount = currentAbilityCD / abilityCD;
                }

                if (abilityText != null)
                {
                    abilityText.text = Mathf.Ceil(currentAbilityCD).ToString();
                }
            }
        }
    }

    protected float getValueWithRatios()
    {
        return baseValueAbility + championControleur.getAD() * ratiosAbility[0] /100 + championControleur.getAP() * ratiosAbility[1]/100;
    }

}
