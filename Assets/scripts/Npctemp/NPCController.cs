using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour, IAttackable
{
    public float walkingSpeed = 3.0f;
    public PathList pathList;
    public int health = 3;
    public int maxHealth = 3;
    public NavMeshAgent navMeshAgent { get; private set; }
    [SerializeField] private bool _isInvulnerable;
    public bool isInvulnerable
    {
        get => _isInvulnerable;
        set => _isInvulnerable = value;
    }

    private bool isPaused = false;
    private Animator animator;
    private int currentWaypointIndex = 0;
    private IState currentState;
    private Transform playerTransform;
    private Renderer[] renderers;
    [HideInInspector] public KnockedOutState currentKnockoutState;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (pathList != null && pathList.waypoints.Count > 0)
        {
            ChangeState(new WalkingState(this, pathList, walkingSpeed));
        }
        else
        {
            Debug.LogWarning("NPC için PathList atanmadý veya boþ.");
        }
    }

    public void SetVisible(bool isVisible)
    {
        foreach (var rend in renderers)
        {
            rend.enabled = isVisible;
        }
    }

    void Update()
    {
        if (!isPaused && currentState != null)
            currentState.Update();
    }

    public void ChangeState(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void Pause()
    {
        isPaused = true;
        navMeshAgent.isStopped = true;
        animator.speed = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        navMeshAgent.isStopped = false;
        animator.speed = 1f;
    }

    public void Attack()
    {
        TakeDamage();
    }

    public void TakeDamage()
    {
        if (isInvulnerable) return; // Skip if invulnerable

        health--;

        if (health > 0)
        {
            ChangeState(new FleeState(this, playerTransform));
        }
        else
        {
            isInvulnerable = true; // Become invulnerable when knocked out
            ChangeState(new KnockedOutState(this));
        }
    }

    public bool IsVulnerable()
    {
        // currentState null ise ve invulnerable deðilse hasar alabilir
        return !isInvulnerable && (currentState == null || !(currentState is KnockedOutState));
    }
    public void ResetHealth()
    {
        health = maxHealth;
    }

    public void SetActiveState(bool active)
    {
        if (navMeshAgent != null)
        {
            if (active)
            {
                navMeshAgent.enabled = true;
                navMeshAgent.isStopped = false;
            }
            else
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.enabled = false;
                if (pathList != null && pathList.waypoints.Count > 0)
                {
                    transform.position = pathList.waypoints[0].position;
                    SetWaypointIndex(0);
                }
            }
        }

        if (animator != null)
        {
            animator.enabled = active;
            animator.speed = active ? 1f : 0f;
        }

        if (!active && currentState != null)
        {
            currentState.Exit();
            currentState = null;
        }
        else if (active && currentState == null && pathList != null && pathList.waypoints.Count > 0)
        {
            ChangeState(new WalkingState(this, pathList, walkingSpeed));
        }
    }

    public void SetWaypointIndex(int index)
    {
        currentWaypointIndex = index;
    }

    public int CurrentWaypointIndex => currentWaypointIndex;

    void OnEnable()
    {
        if (pathList != null && pathList.waypoints.Count > 0 && navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            ChangeState(new WalkingState(this, pathList, walkingSpeed));
        }
    }

    void OnDisable()
    {
        currentState?.Exit();
        currentState = null;
    }

    public void OnGetUpAnimationComplete()
    {
        if (currentKnockoutState != null)
        {
            currentKnockoutState.OnGetUpComplete();
        }
        else
        {
            isInvulnerable = false; // Safety measure
        }
    }
}