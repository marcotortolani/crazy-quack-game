using UnityEngine;

public class SlimeEnemy : MonoBehaviour, IsDamageable
{
    [Header("Slime Settings")]
    public bool isGiant = true;
    public GameObject smallSlimePrefab; // Asignar el prefab del slime pequeño aquí
    public int life = 60;
    public float speed = 2f;
    
    [Header("Effects & Stats")]
    public string enemyType = "SlimeBoss";
    public GameObject deathEffect;
    
    [Header("VFX")]
    public TrailRenderer movementTrail; // Arrastra aquí el objeto "Rastro"

    private Transform target;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // Importante: Para que atraviese paredes, el Rigidbody2D debe ser Kinematic 
        // o las capas de colisión deben estar configuradas para ignorar "Wall".
        FindTarget();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerIsWin)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null) FindTarget();
        Move();
    }

    private void FindTarget()
    {
        Player p = FindObjectOfType<Player>();
        if (p != null) target = p.transform;
    }

    private void Move()
    {
        if (target == null) 
        {
            if(movementTrail != null) movementTrail.emitting = false;
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * speed;

        // Activar el rastro solo si tiene velocidad
        if(movementTrail != null)
        {
            movementTrail.emitting = rb.velocity.magnitude > 0.1f;
        }

        spriteRenderer.flipX = direction.x > 0;
    }

    public void TakeDamage(int damage)
    {
        life -= damage;
        
        // Trigger de animación de Hit
        if (animator != null) animator.SetTrigger("hit");

        if (life <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Registrar muerte
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemyKill(isGiant ? "SlimeBoss" : "SlimeSmall");

        // Efectos
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // LÓGICA DE SUBDIVISIÓN
        if (isGiant && smallSlimePrefab != null)
        {
            for (int i = 0; i < 3; i++)
            {
                // Aparecen con un ligero offset aleatorio para que no se solapen perfecto
                Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(smallSlimePrefab, transform.position + spawnOffset, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    // El slime no se destruye al tocar al jugador, solo le hace daño
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(1); 
                // Al ser OnCollisionStay, hará daño mientras lo toque (según el cooldown del Player)
            }
        }
    }
}