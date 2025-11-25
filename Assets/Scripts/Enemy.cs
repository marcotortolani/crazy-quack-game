using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float speedRotation;
    public Transform target;
    public int life;
    
    private SpriteRenderer spriteRenderer;
    private Animator animator; // Si tienes animaciones de caminar
    
    public GameObject deathEffect;

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
                GameManager.Instance.enemiesKilled++;
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
            // Si necesitas sprites diferentes para arriba/abajo, puedes usar animator
        
            // Para animaciones con 4 direcciones
            //if (animator != null)
            // {
            //     animator.SetFloat("MovementX", direction.x);
            //     animator.SetFloat("MovementY", direction.y);
            // }
        }
    }
   
}