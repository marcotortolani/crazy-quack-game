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
            if (GameManager.Instance)
            {
                GameManager.Instance.RegisterEnemyKill(enemyType); // ← Usar el campo enemyType
            }
            Instantiate(deathEffect, transform.position, Quaternion.identity);
            AudioManager.Instance.PlaySound("EnemyDeath");
            Destroy(gameObject);
        }
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