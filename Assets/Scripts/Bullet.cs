using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed;
    public int bulletDamage;
    public int maxBounces = 3;
    
    private int _currentBounces = 0;

    private void Update()
    {
        transform.position += transform.up * (bulletSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Colision con paredes, destruir balas despues de X rebotes
        if (collision.gameObject.CompareTag("Wall"))
        {
            _currentBounces++;
            if (_currentBounces >= maxBounces)
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

        // Colision con enemigos
        // Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        // if (enemy != null)
        // {
        //     enemy.TakeDamage(bulletDamage);
        //     Destroy(gameObject);
        // }
        
        // Colision con enemigos (usando Tag)
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Intentar con el script Enemy genérico
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(bulletDamage);
                Destroy(gameObject);
                return;
            }

            // Intentar con PigEnemy
            PigEnemy pigEnemy = collision.gameObject.GetComponent<PigEnemy>();
            if (pigEnemy != null)
            {
                pigEnemy.TakeDamage(bulletDamage);
                Destroy(gameObject);
                return;
            }

            // Intentar con MushroomEnemy
            MushroomEnemy mushroomEnemy = collision.gameObject.GetComponent<MushroomEnemy>();
            if (mushroomEnemy != null)
            {
                mushroomEnemy.TakeDamage(bulletDamage);
                Destroy(gameObject);
                return;
            }
            
            PlantEnemy plantEnemy = collision.gameObject.GetComponent<PlantEnemy>();
            if (plantEnemy != null)
            {
                plantEnemy.TakeDamage(bulletDamage);
                Destroy(gameObject);
                return;
            }
        }
    }
    
}