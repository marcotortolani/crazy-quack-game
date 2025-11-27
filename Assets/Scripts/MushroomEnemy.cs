using UnityEngine;

public class MushroomEnemy : MonoBehaviour, IsDamageable
{
    [Header("Movement")]
    public float chaseSpeed = 5f;
    public float detectionRange = 5f; // Radio para detectar al player
    
    [Header("Explosion")]
    public float explosionTimer = 3f; // Tiempo hasta explotar
    public float explosionRange = 2f; // Radio de daño de la explosión
    public int minDamage = 1; // Daño mínimo (lejos)
    public int maxDamage = 5; // Daño máximo (muy cerca)
    public GameObject explosionEffect;
    
    [Header("Stats")]
    public int life = 10;
    
    private Transform target;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isChasing = false;
    private float chaseTimeCounter = 0f;
    private bool hasExploded = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        FindTarget();
        
        // Iniciar en idle
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
        }
    }

    private void Update()
    {
        if (hasExploded || GameManager.Instance.playerIsWin)
        {
            return;
        }
        
        if (target == null)
        {
            FindTarget();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        // Si el player entra en el rango de detección, activarse
        if (!isChasing && distanceToPlayer <= detectionRange)
        {
            ActivateChase();
        }

        // Si está persiguiendo
        if (isChasing)
        {
            ChasePlayer();
            
            // Contar tiempo hasta explotar
            chaseTimeCounter += Time.deltaTime;
            if (chaseTimeCounter >= explosionTimer)
            {
                Explode();
            }
        }
    }

    private void FindTarget()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void ActivateChase()
    {
        isChasing = true;
        chaseTimeCounter = 0f;
        
        // Cambiar a animación de correr
        if (animator != null)
        {
            animator.SetBool("isRunning", true);
        }
        
        // Opcional: reproducir sonido de activación
        // AudioManager.Instance.PlaySound("MushroomActivate");
    }
    
    private void ChasePlayer()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.Normalize();
        transform.position += direction * (chaseSpeed * Time.deltaTime);

        if (direction.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x > 0)
        {
            spriteRenderer.flipX = true;
        }

        // Efecto visual: parpadear más rápido cuando está por explotar
        float timeLeft = explosionTimer - chaseTimeCounter;
        if (timeLeft <= 1f)
        {
            // Parpadeo rápido en el último segundo
            float blinkSpeed = 10f;
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * blinkSpeed, 1));
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        
        hasExploded = true;
        
        // Desactivar collider inmediatamente
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    
        // Desactivar script
        enabled = false;

        // Activar animación de hit/explosion
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // Buscar al player y calcular daño según distancia
        if (target != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            
            if (distanceToPlayer <= explosionRange)
            {
                Player player = target.GetComponent<Player>();
                if (player != null)
                {
                    // Calcular daño basado en distancia (más cerca = más daño)
                    float damagePercent = 1f - (distanceToPlayer / explosionRange);
                    int damage = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, damagePercent));
                    
                    player.TakeDamage(damage);
                    Debug.Log("Mushroom explotó! Daño: " + damage + " (distancia: " + distanceToPlayer.ToString("F2") + ")");
                }
            }
        }

        // Efecto de explosión
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Sonido de explosión
        AudioManager.Instance.PlaySound("MushroomExplosion");

        // Contar como enemigo eliminado
        if (GameManager.Instance)
        {
            GameManager.Instance.RegisterEnemyKill("Mushroom");
        }
        
        // Ocultar sprite
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
        
        // Intentar dropear power-up normal
        if (PowerUpDropper.Instance != null)
        {
            PowerUpDropper.Instance.TryDropPowerUp(transform.position);
        }

        // Destruir después de un pequeño delay para que se vea la animación
        Destroy(gameObject, 0.3f);
    }

    public void TakeDamage(int damage)
    {
        life -= damage;
        if (life <= 0)
        {
            // Si lo matan antes de explotar
            Explode();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Si choca con el player, explotar inmediatamente
            Explode();
        }
    }

    // Visualizar rangos en el editor
    private void OnDrawGizmosSelected()
    {
        // Radio de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Radio de explosión (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}