using UnityEngine;
using UnityEngine.AI;

public class Animations : MonoBehaviour
{
    [SerializeField] private Avatar avatarRun;
    [SerializeField] private Avatar avatarIdle;
    protected Avatar avatarAA;

    protected Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    protected void Start()
    {
        if (animator != null)
        {
            animator.avatar = avatarIdle;
        }
        else
        {
            Debug.Log(gameObject.name + ": Animator missing");
        }
    }

    public void runAnimation(NavMeshAgent agent, float motionSmoothTime)
    {
        if(avatarRun != null)
        {
            animator.avatar = avatarRun;
            float speed = agent.velocity.magnitude / agent.speed;
            animator.SetFloat("Speed", speed, motionSmoothTime, Time.deltaTime);
        }
        else
        {
            Debug.Log(gameObject.name + ": Avatar Run missing");
        }
        
    }

    public void startAaAnimation()
    {
        if (avatarAA != null)
        {
            animator.avatar = avatarAA;
            animator.SetBool("isAttack", true);
        }
        else
        {
            Debug.Log(gameObject.name + ": Avatar AA missing");
        }
    }

    public void stopAaAnimation()
    {
        animator.SetBool("isAttack", false);
    }

    internal void changeAttackSpeed(float attackSpeed)
    {
        animator.SetFloat("AttackSpeed", attackSpeed);
    }
}
