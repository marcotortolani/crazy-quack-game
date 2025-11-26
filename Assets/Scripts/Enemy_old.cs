using UnityEngine;

public class EnemyOld : MonoBehaviour
{
    public float speed;
    public float speedRotation;
    public Transform target;
    public int life;

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

        if (GameManagerOld.Instance.playerIsWin)
        {
            Destroy(gameObject);
        }
    }
    private void FindTarget()
    {
        PlayerOld player = FindObjectOfType<PlayerOld>();
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerOld player = collision.gameObject.GetComponent<PlayerOld>();
            player.TakeDamage(1);
            Destroy(gameObject);
        }
    }


    public void TakeDamage(int damage)
    {
        life -= damage;
        if (life <= 0)
        {
            if (GameManagerOld.Instance)
            {
                GameManagerOld.Instance.enemiesKilled++;
            }
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