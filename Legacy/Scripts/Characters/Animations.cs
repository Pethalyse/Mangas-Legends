using UnityEngine;
using UnityEngine.AI;

public class Animations : MonoBehaviour
{
    protected Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    protected void Start()
    {
        if (animator == null)
        {
            Debug.Log(gameObject.name + ": Animator missing");
        }
    }

    public void runAnimation(NavMeshAgent agent, float motionSmoothTime)
    {
        float speed = agent.velocity.magnitude / agent.speed;
        animator.SetFloat("Speed", speed, motionSmoothTime, Time.deltaTime);
        
    }

    public void startAaAnimation()
    {
        animator.SetBool("isAttack", true);
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
