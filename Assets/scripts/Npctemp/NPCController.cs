using UnityEngine;

public class NPCController : MonoBehaviour, IAttackable
{
    public float walkingSpeed = 3.0f;
    public PathList pathList; // Her NPC için ayrý bir PathList referansý
    public int health = 3; // NPC'nin caný
    public int maxHealth = 3; // NPC'nin maksimum caný

    [SerializeField] private IState currentState;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // Oyuncuyu bul

        if (pathList != null)
        {
            // WalkingState'i baþlat ve PathList'i aktar
            ChangeState(new WalkingState(this, pathList, walkingSpeed));
        }
        else
        {
            Debug.LogWarning("PathList not assigned for NPC: " + gameObject.name);
        }
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
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
        Debug.Log("NPC'nin caný yenilendi: " + health);
    }
}