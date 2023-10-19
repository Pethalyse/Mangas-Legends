using UnityEngine;
using UnityEngine.AI;

public class Mouvements : MonoBehaviour
{
    //stats mouvements
    [SerializeField] float rotateSpeedMovement = 0.05f;
    private float rotateVelocity;

    private float motionSmoothTime = 0.1f;

    //components
    private NavMeshAgent navigation;
    private ChampionControleur championControleur;
    private Animations animationControleur;

    private void Awake()
    {
        navigation = GetComponent<NavMeshAgent>();
        if (!navigation)
        {
            gameObject.AddComponent<NavMeshAgent>();
            navigation = GetComponent<NavMeshAgent>();
        }

        championControleur = GetComponent<ChampionControleur>();
        animationControleur = GetComponent<Animations>();
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        {
            navigation.speed = championControleur.getMoveSpeed();
            if (!championControleur.getIsAttack())
            {
                animationControleur.runAnimation(navigation, motionSmoothTime);
            }
            move();
        }



    }

    public void move()
    {

        if(Input.GetKeyDown(KeyCode.S))
        {
            moveToPosition(transform.position);
            championControleur.RpcTargetToNull();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity) && hit.collider.CompareTag("Sol"))
            {
                moveToPosition(hit.point);
                //transform.LookAt(hit.point);

            }
            else if (!championControleur.inSameTeam(hit.collider.gameObject) && (LayerMask.GetMask("Characters") & (1 << hit.collider.gameObject.layer)) != 0)
            {
                moveToObject(hit.collider.gameObject, championControleur.getAutoRange());
            }

        }
        
        if(championControleur.getTarget() != null && !championControleur.inSameTeam(championControleur.getTarget().gameObject) && (LayerMask.GetMask("Characters") & (1 << championControleur.getTarget().gameObject.layer)) != 0)
        {
            if(Vector3.Distance(transform.position, championControleur.getTarget().position) > championControleur.getAutoRange())
            {
                navigation.SetDestination(championControleur.getTarget().position);
            }
        }
    }

    public void moveToPosition(Vector3 position)
    {
        //attack gestion
        championControleur.setIsAttack(false);
        championControleur.setCanAuto(true);

        //target null
        championControleur.RpcTargetToNull();

        //move
        navigation.SetDestination(position);
        navigation.stoppingDistance = 0f;

        if(position != transform.position)
        {
            lookAt(position);
        }
        

        if(championControleur.getTarget() != null )
        {
            championControleur.setTarget(null);
        }
    }

    public void moveToObject(GameObject obj, float range)
    {
        navigation.SetDestination(obj.transform.position);
        navigation.stoppingDistance = range;

        lookAt(obj.transform.position);
    }

    public void lookAt(Vector3 look)
    {
        Quaternion rotationLookAt = Quaternion.LookRotation(look - transform.position);
        float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationLookAt.eulerAngles.y, ref rotateVelocity, rotateSpeedMovement * (Time.deltaTime * 5));
        transform.eulerAngles = new Vector3(0, rotationY, 0);
    }

    public NavMeshAgent getNavigation() { return navigation; }
    public Animations getAnimations() { return animationControleur; }
}
