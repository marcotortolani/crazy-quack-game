using UnityEngine;

public class EggBullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public int maxBounces = 3;
    
    [Header("Egg Crack Effect")]
    public GameObject eggCrackedPrefab; // El sprite del huevo roto
    public float crackedLifetime = 4f;  // Duración del huevo roto
    
    private int bounceCount = 0;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = transform.up * speed;
        }
        
        // Ignorar colisión con el player que disparó
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Collider2D bulletCollider = GetComponent<Collider2D>();
        
            if (playerCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, playerCollider);
            }
        }
    }
    
    private void Update()
    {
        // Hacer que el sprite apunte en la dirección del movimiento
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Bullet collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        // Verificar si es damageable (enemigo)
        IsDamageable damageable = collision.gameObject.GetComponent<IsDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            SpawnCrackedEgg();
            Destroy(gameObject);
            return;
        }

        // Si choca con pared
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log($"Hit wall! Bounce count: {bounceCount}/{maxBounces}");
            bounceCount++;
            
            if (bounceCount >= maxBounces)
            {
                Debug.Log("Max bounces reached, destroying bullet");
                SpawnCrackedEgg();
                Destroy(gameObject);
            }
            else
            {
                // Calcular rebote
                Vector2 inDirection = rb.velocity.normalized;
                Vector2 inNormal = collision.contacts[0].normal;
                Vector2 newVelocity = Vector2.Reflect(inDirection, inNormal) * speed;
                rb.velocity = newVelocity;
                
                Debug.Log($"Bounced! New velocity: {newVelocity}");
                
                // Rotar el bullet en la nueva dirección
                float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        else
        {
            Debug.Log($"Hit something else: {collision.gameObject.name}");
        }
    }

    private void SpawnCrackedEgg()
    {
        if (eggCrackedPrefab == null)
        {
            Debug.LogWarning("Egg Cracked Prefab is not assigned!");
            return;
        }
        // Crear el huevo roto en la posición del impacto
        GameObject crackedEgg = Instantiate(eggCrackedPrefab, transform.position, Quaternion.identity);
        
        // Destruir el huevo roto después del tiempo especificado
        Destroy(crackedEgg, crackedLifetime);
    }
}