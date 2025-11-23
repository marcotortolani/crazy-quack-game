using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed;
    public int bulletDamage;
    public int maxBounces = 3;
    
    private int currentBounces = 0;

    private void Update()
    {
        transform.position += transform.up * (bulletSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Colision con paredes, destruir balas despues de X rebotes
        if (collision.gameObject.CompareTag("Wall"))
        {
            currentBounces++;
            if (currentBounces >= maxBounces)
            {
                Destroy(gameObject);
                return;
            }
            
            // Obtener la normal del punto de contacto
            Vector3 normal = collision.contacts[0].normal;
            Vector3 incomingDirection = transform.up;
            Vector3 reflectedDirection = Vector3.Reflect(incomingDirection, normal);
            
            // Rotar la bala en la nueva dirección
            float angle = Mathf.Atan2(reflectedDirection.y, reflectedDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 porque transform.up es la dirección
            
            return;
            
        }

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(bulletDamage);
            Destroy(gameObject);
        }
    }
    
}