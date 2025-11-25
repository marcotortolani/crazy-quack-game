using UnityEngine;

public class PlantEnemy : MonoBehaviour
{
    [Header("Detection & Shooting")]
    public float detectionRange = 6f;
    public float shootingCooldown = 0.5f; // Disparo rápido
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    
    [Header("Stats")]
    public int life = 20;
    public GameObject deathEffect;
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform playerTarget;
    private float nextShootTime = 0f;
    private bool playerInRange = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        FindPlayer();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerIsWin)
        {
            return;
        }

        if (playerTarget == null)
        {
            FindPlayer();
            return;
        }

        // Verificar si el player está en rango
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        playerInRange = distanceToPlayer <= detectionRange;

        // Actualizar animación
        if (animator != null)
        {
            animator.SetBool("isAttacking", playerInRange);
        }

        // Mirar siempre hacia el player si está en rango
        if (playerInRange)
        {
            LookAtPlayer();
            TryShoot();
        }
    }

    private void LookAtPlayer()
    {
        // Determinar si el player está a la izquierda o derecha
        float directionX = playerTarget.position.x - transform.position.x;
        
        // Para el sprite original que mira a la izquierda:
        spriteRenderer.flipX = directionX > 0;
    }

    private void FindPlayer()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }

    private void TryShoot()
    {
        if (!playerInRange) return;
        if (Time.time < nextShootTime) return;
        
        Shoot();
        nextShootTime = Time.time + shootingCooldown;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || bulletSpawnPoint == null) return;

        // Calcular dirección hacia el player
        Vector2 direction = (playerTarget.position - bulletSpawnPoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Crear la bala
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.Euler(0, 0, angle - 90));
        
        // Opcional: sonido de disparo
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("PlantShoot");
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
        if (GameManager.Instance)
        {
            GameManager.Instance.RegisterEnemyKill("Pig");
        }
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("EnemyDeath");
        }
        
        Destroy(gameObject);
    }

    // Visualización en el editor
    private void OnDrawGizmos()
    {
        // Rango de detección (siempre visible)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Naranja transparente
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Rango de detección más visible cuando está seleccionado
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Dibujar línea hacia el spawn point de la bala
        if (bulletSpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, bulletSpawnPoint.position);
            Gizmos.DrawSphere(bulletSpawnPoint.position, 0.1f);
        }
    }
}