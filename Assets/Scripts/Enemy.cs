using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float speedRotation;
    public Transform target;
    public int life;
    
    public GameObject deathEffect;

    private void Start()
    {
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
            // Seguimiento + dirección mirando hacia el Player
            Vector3 direction = target.position - transform.position;
            direction.Normalize();
            
            transform.position += transform.up * (speed * Time.deltaTime);

            // Rotación con smooth
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speedRotation * Time.deltaTime);
        }
    }
}