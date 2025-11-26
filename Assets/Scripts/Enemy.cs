using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Type")]
    public string enemyType = "Chicken"; // Configurable desde el Inspector
    
    [Header("Movement")]
    public float speed;
    public float speedRotation;
    
    [Header("Stats")]
    public int life;
    public GameObject deathEffect;
    
    private Transform target;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        FindTarget();
    }

    private void Update()
    {
        if (target == null)
        {
            FindTarget();
        }
        
        FollowingTarget();

        if (GameManager.Instance.playerIsWin)
        {
            Destroy(gameObject);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.TakeDamage(1);
            Destroy(gameObject);
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
        
        // Intentar dropear power-up
        if (PowerUpDropper.Instance != null)
        {
            PowerUpDropper.Instance.TryDropPowerUp(transform.position);
        }
    
        // Destruir con delay para evitar errores de referencia
        Destroy(gameObject, 0.1f);
    }

    private void FollowingTarget()
    {
        if (target != null)
        {
            Vector3 direction = target.position - transform.position;
            direction.Normalize();
        
            // Mover hacia el player
            transform.position += direction * (speed * Time.deltaTime);

            // Determinar la dirección predominante
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Movimiento horizontal predominante
                if (direction.x < 0)
                {
                    spriteRenderer.flipX = false;  // Derecha
                }
                else
                {
                    spriteRenderer.flipX = true;  // Izquierda
                }
            }
        }
    }
}