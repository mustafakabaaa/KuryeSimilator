using UnityEngine;

public class NPCController : MonoBehaviour
{
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    public float attackRange = 2.0f;
    public float detectionRange = 10.0f;

    public Transform player;
    public Animator animator;
    [SerializeField]
    private INPCState currentState;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();

        // Baþlangýç durumu
        TransitionToState(new IdleState());
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState(this);
        }

    }

    public void TransitionToState(INPCState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }

        currentState = newState;
        currentState.EnterState(this);
    }

    public void MoveTowardsPlayer(float speed)
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(player);
    }

    public void AttackPlayer()
    {
        Debug.Log("NPC is attacking the player!");
    }
}