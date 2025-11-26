using UnityEngine;

public class PigEnemy : MonoBehaviour
{
    [Header("Patrol Settings")]
    public bool isHorizontalPatrol = true; // true = horizontal, false = vertical
    public float walkSpeed = 2f;
    public float idleTime = 3f;
    public float walkTime = 10f;
    
    [Header("Patrol Limits")]
    public float minLimit = -10f; // Límite mínimo (izquierda o abajo)
    public float maxLimit = 10f;  // Límite máximo (derecha o arriba)
    
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float shootingRange = 5f;
    public float shootingCooldown = 2f;
    
    [Header("Stats")]
    public int life = 30;
    public GameObject deathEffect;
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;
    
    private bool isWalking = false;
    private float stateTimer = 0f;
    private int moveDirection = 1; // 1 = derecha/arriba, -1 = izquierda/abajo
    
    private Transform playerTarget;
    private float nextShootTime = 0f;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        FindPlayer();
        
        // Empezar en walk
        //StartIdleState();
        StartWalkState();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerIsWin)
        {
            return;
        }

        // Actualizar timer del estado actual
        stateTimer += Time.deltaTime;

        if (isWalking)
        {
            // Si terminó el tiempo de caminar, volver a idle
            if (stateTimer >= walkTime)
            {
                StartIdleState();
            }
            else
            {
                Patrol();
                TryShoot();
            }
        }
        else // está en idle
        {
            // Si terminó el tiempo de idle, empezar a caminar
            if (stateTimer >= idleTime)
            {
                StartWalkState();
            }
        }
    }

    private void StartIdleState()
    {
        isWalking = false;
        stateTimer = 0f;
        
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void StartWalkState()
    {
        isWalking = true;
        stateTimer = 0f;
        
        if (animator != null)
        {
            animator.SetBool("isWalking", true);
        }
    }

    private void Patrol()
    {
        // Mover según la dirección de patrulla
        if (isHorizontalPatrol)
        {
            // Patrulla horizontal
            float newX = transform.position.x + (moveDirection * walkSpeed * Time.deltaTime);
            
            // Verificar límites
            if (newX <= minLimit || newX >= maxLimit)
            {
                moveDirection *= -1; // Invertir dirección
                newX = Mathf.Clamp(newX, minLimit, maxLimit); // Asegurar que no se pase
            }
            
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            
            // Flip del sprite según dirección
            // // Si moveDirection es positivo (1) = va a la derecha → flipX = false
            // // Si moveDirection es negativo (-1) = va a la izquierda → flipX = true
            spriteRenderer.flipX = moveDirection > 0;
        }
        else
        {
            // Patrulla vertical
            float newY = transform.position.y + (moveDirection * walkSpeed * Time.deltaTime);
            
            // Verificar límites
            if (newY <= minLimit || newY >= maxLimit)
            {
                moveDirection *= -1; // Invertir dirección
                newY = Mathf.Clamp(newY, minLimit, maxLimit); // Asegurar que no se pase
            }
            
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si choca con una pared, dar la vuelta
        if (collision.gameObject.CompareTag("Wall"))
        {
            moveDirection *= -1;
        }
        
        // Si choca con el player, hacer daño
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
        }
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
        if (!isWalking) return;
        if (Time.time < nextShootTime) return;
        
        if (playerTarget == null)
        {
            FindPlayer();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        
        if (distanceToPlayer <= shootingRange)
        {
            Shoot();
            nextShootTime = Time.time + shootingCooldown;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || bulletSpawnPoint == null) return;

        Vector2 direction = (playerTarget.position - bulletSpawnPoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.Euler(0, 0, angle - 90));
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("EnemyShoot");
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
        // Desactivar collider inmediatamente
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    
        // Desactivar script
        enabled = false;
        
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
        
        // Ocultar sprite
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
        
        // Intentar dropear power-up
        if (PowerUpDropper.Instance != null)
        {
            PowerUpDropper.Instance.TryDropPowerUp(transform.position);
        }
        
        Destroy(gameObject, 0.1f);
    }

    // Visualización en el editor
    private void OnDrawGizmos()
    {
        // Dibujar rango de patrulla
        Gizmos.color = Color.yellow;
        
        if (isHorizontalPatrol)
        {
            // Línea horizontal mostrando el rango
            Vector3 leftPoint = new Vector3(minLimit, transform.position.y, transform.position.z);
            Vector3 rightPoint = new Vector3(maxLimit, transform.position.y, transform.position.z);
            
            Gizmos.DrawLine(leftPoint, rightPoint);
            
            // Puntos en los límites
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leftPoint, 0.2f);
            Gizmos.DrawSphere(rightPoint, 0.2f);
        }
        else
        {
            // Línea vertical mostrando el rango
            Vector3 bottomPoint = new Vector3(transform.position.x, minLimit, transform.position.z);
            Vector3 topPoint = new Vector3(transform.position.x, maxLimit, transform.position.z);
            
            Gizmos.DrawLine(bottomPoint, topPoint);
            
            // Puntos en los límites
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(bottomPoint, 0.2f);
            Gizmos.DrawSphere(topPoint, 0.2f);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Rango de disparo (solo cuando está seleccionado)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
        
        // Línea más gruesa para los límites cuando está seleccionado
        Gizmos.color = Color.green;
        
        if (isHorizontalPatrol)
        {
            Vector3 leftPoint = new Vector3(minLimit, transform.position.y, transform.position.z);
            Vector3 rightPoint = new Vector3(maxLimit, transform.position.y, transform.position.z);
            
            // Dibujar línea más visible
            for (float i = -0.1f; i <= 0.1f; i += 0.05f)
            {
                Gizmos.DrawLine(
                    new Vector3(leftPoint.x, leftPoint.y + i, leftPoint.z),
                    new Vector3(rightPoint.x, rightPoint.y + i, rightPoint.z)
                );
            }
        }
        else
        {
            Vector3 bottomPoint = new Vector3(transform.position.x, minLimit, transform.position.z);
            Vector3 topPoint = new Vector3(transform.position.x, maxLimit, transform.position.z);
            
            // Dibujar línea más visible
            for (float i = -0.1f; i <= 0.1f; i += 0.05f)
            {
                Gizmos.DrawLine(
                    new Vector3(bottomPoint.x + i, bottomPoint.y, bottomPoint.z),
                    new Vector3(topPoint.x + i, topPoint.y, topPoint.z)
                );
            }
        }
    }
}