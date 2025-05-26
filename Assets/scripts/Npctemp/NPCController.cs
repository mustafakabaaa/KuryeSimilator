using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour, IAttackable
{
    public float walkingSpeed = 3.0f;
    public PathList pathList; // Her NPC için ayrý bir PathList referansý
    public int health = 3; // NPC'nin caný
    public int maxHealth = 3; // NPC'nin maksimum caný
    public NavMeshAgent navMeshAgent { get; private set; } // Eriþim saðlar, dýþarýdan deðiþtirilemez
    private bool isPaused = false;
    private Animator animator;
   
    private int currentWaypointIndex = 0;

    [SerializeField] private IState currentState;
    private Transform playerTransform;
    private Renderer[] renderers;

    // NPCController.cs'de þu deðiþiklikleri yapýn:
    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Player'ý sadece bir kere bul ve cache'le
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

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
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();
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
        // Oyuncu NPC'ye saldýrdýðýnda
        TakeDamage();
    }

    public void TakeDamage()
    {
        health--; // Can azalt

        if (health > 0)
        {
            // Kaçma durumuna geç
            ChangeState(new FleeState(this, playerTransform));
        }
        else
        {
            // Bayýlma durumuna geç
            ChangeState(new KnockedOutState(this));
        }
    }
    public void ResetHealth()
    {
        health = maxHealth; // Caný yenile
        //Debug.Log("NPC'nin caný yenilendi: " + health);
    }
    public void SetActiveState(bool active)
    {
        if (navMeshAgent != null)
        {
            if (active)
            {
                // NavMesh üzerinde mi diye kontrol et
                if (!navMeshAgent.isOnNavMesh)
                {
                    Debug.LogWarning("NPC NavMesh üzerinde deðil!");
                    return;
                }

                navMeshAgent.enabled = true;
                navMeshAgent.isStopped = false;
            }
            else
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.enabled = false;
            }
        }

        if (animator != null)
        {
            animator.enabled = active;
        }

        // FSM’i baþlat/durdur
        if (!active)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }
        }
        else
        {
            if (currentState == null && pathList != null && pathList.waypoints.Count > 0)
            {
                ChangeState(new WalkingState(this, pathList, walkingSpeed));
            }
        }
    }

    public void SetWaypointIndex(int index)
    {
        currentWaypointIndex = index;
    }
    // NPCController.cs'de þu eklemeleri yapýn:

    void OnEnable()
    {
        if (pathList != null && pathList.waypoints.Count > 0 && navMeshAgent != null)
        {
            if (navMeshAgent.isOnNavMesh)
            {
                ChangeState(new WalkingState(this, pathList, walkingSpeed));
            }
            else
            {
                Debug.LogWarning("NPC OnEnable içinde NavMesh üzerinde deðil!");
            }
        }
    }


    void OnDisable()
    {
        // NPC devre dýþý býrakýldýðýnda mevcut durumu temizle
        if (currentState != null)
        {
            currentState.Exit();
            currentState = null;
        }
    }

}