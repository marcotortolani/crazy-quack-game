using UnityEngine;

public class Enemy : MonoBehaviour, IsDamageable
{
    [Header("Enemy Type")]
    public string enemyType = "Chicken"; // Configurable desde el Inspector
    
    [Header("Movement")]
    public float speed;
    public float speedRotation;
    
    [Header("Stats")]
    public int life;
    public GameObject deathEffect;
    
    [Header("Obstacle Avoidance")]
    public float unstuckDuration = 0.5f; // Tiempo moviéndose aleatoriamente
    
    private Transform target;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb; 
    
    // Variables para esquivar obstáculos
    private bool isStuck = false;
    private Vector2 randomDirection;
    private float unstuckTimer = 0f;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        FindTarget();
    }

    private void Update()
    {
        // Verificar que existe el GameManager
        if(GameManager.Instance == null) return;

        // Si el player gano, destruir el enemigo
        if (GameManager.Instance.playerIsWin)
        {
            Destroy(gameObject);
            return;
        }
        
        if (target == null)
        {
            FindTarget();
        }
        
        FollowingTarget();
    }
    
    private void FindTarget()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Colisión con el player
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.TakeDamage(1);
            Destroy(gameObject);
        }
        
        // Colisión con pared
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Generar dirección aleatoria para esquivar
            if (target != null)
            {
                Vector2 directionToPlayer = (target.position - transform.position).normalized;
                float randomAngle = Random.Range(-90f, 90f); // Ángulo aleatorio
                randomDirection = Quaternion.Euler(0, 0, randomAngle) * directionToPlayer;
            }
            else
            {
                // Si no hay target, dirección completamente aleatoria
                randomDirection = Random.insideUnitCircle.normalized;
            }
            
            // Activar modo "atascado"
            isStuck = true;
            unstuckTimer = unstuckDuration;
        }
    }

    public void TakeDamage(int damage)
    {
        life -= damage;
        if (life <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Desactivar el collider inmediatamente para evitar más colisiones
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        // Desactivar el script para que no siga ejecutándose
        enabled = false;
    
        // Registrar kill
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemyKill(enemyType.ToString());
        }
    
        // Efectos
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
    
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("EnemyDeath");
        }
    
        // Ocultar sprite inmediatamente
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
        
        // Intentar dropear power-up normal
        if (PowerUpDropper.Instance != null)
        {
            PowerUpDropper.Instance.TryDropPowerUp(transform.position);
        }
    
        // Destruir con delay para evitar errores de referencia
        Destroy(gameObject, 0.1f);
    }

    private void FollowingTarget()
    {
        if (target == null) return;
        
        Vector2 direction;
        
        if (isStuck)
        {
            unstuckTimer -= Time.deltaTime;
            direction = randomDirection;
            
            if (unstuckTimer <= 0f)
            {
                isStuck = false;
            }
        }
        else
        {
            direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        }
        
        // CAMBIO IMPORTANTE: Usar Rigidbody2D en lugar de transform.position
        if (rb != null)
        {
            rb.velocity = direction * speed; // ← Usar velocity
        }
        else
        {
            // Fallback si no hay Rigidbody2D
            transform.position += (Vector3)direction * (speed * Time.deltaTime);
        }

        // Flipear sprite según dirección
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x < 0)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
        }
    }
}