using UnityEngine;

// 1. Heredar de la clase Bullet
public class EggBullet : Bullet 
{
    // [speed, damage, maxBounces, bounceCount, rb, hasCollided]
    // Estas variables ahora son manejadas por la clase base Bullet.
    
    // Se mantienen las propiedades específicas del efecto visual
    [Header("Egg Crack Effect")]
    public GameObject eggCrackedPrefab;
    public float crackedLifetime = 4f;

    // 2. Sobrescribir el método de destrucción de la clase base
    // Este método se llamará cuando la bala golpee a un enemigo o se quede sin rebotes.
    protected override void OnDestroyAction()
    {
        SpawnCrackedEgg();
        
        // ¡Importante! Llamar al método base para completar la destrucción (Destroy(gameObject))
        base.OnDestroyAction(); 
    }
    
    // ELIMINADO: OnCollisionEnter2D ya no es necesario; la lógica de rebote y daño
    // se maneja en el OnCollisionEnter2D de la clase base Bullet.cs

    private void SpawnCrackedEgg()
    {
        if (eggCrackedPrefab == null) return;
        
        // Instancia el efecto visual
        GameObject crackedEgg = Instantiate(eggCrackedPrefab, transform.position, Quaternion.identity);
        Destroy(crackedEgg, crackedLifetime);
    }
}


// using UnityEngine;
//
// public class EggBullet : MonoBehaviour
// {
//     public float speed = 20f;
//     public int damage = 10;
//     public int maxBounces = 3;
//     
//     [Header("Egg Crack Effect")]
//     public GameObject eggCrackedPrefab;
//     public float crackedLifetime = 4f;
//     
//     private int bounceCount = 0;
//     private Rigidbody2D rb;
//     private bool hasCollided = false; // Evitar múltiples colisiones en el mismo frame
//
//     private void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         if (rb != null)
//         {
//             rb.velocity = transform.up * speed;
//         }
//         
//         // Ignorar colisión con el player
//         Player player = FindObjectOfType<Player>();
//         if (player != null)
//         {
//             Collider2D playerCollider = player.GetComponent<Collider2D>();
//             Collider2D bulletCollider = GetComponent<Collider2D>();
//         
//             if (playerCollider != null && bulletCollider != null)
//             {
//                 Physics2D.IgnoreCollision(bulletCollider, playerCollider);
//             }
//         }
//     }
//     
//     private void Update()
//     {
//         // Hacer que el sprite apunte en la dirección del movimiento
//         if (rb != null && rb.velocity.magnitude > 0.1f)
//         {
//             float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
//             transform.rotation = Quaternion.Euler(0, 0, angle);
//         }
//     }
//
//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         if (hasCollided) return; // Evitar procesamiento múltiple
//         
//         // Verificar si es damageable (enemigo)
//         IsDamageable damageable = collision.gameObject.GetComponent<IsDamageable>();
//         if (damageable != null)
//         {
//             hasCollided = true;
//             damageable.TakeDamage(damage);
//             SpawnCrackedEgg();
//             Destroy(gameObject);
//             return;
//         }
//
//         // Si choca con pared
//         // if (collision.gameObject.CompareTag("Wall"))
//         // {
//         //     bounceCount++;
//         //     
//         //     if (bounceCount >= maxBounces)
//         //     {
//         //         hasCollided = true;
//         //         SpawnCrackedEgg();
//         //         Destroy(gameObject);
//         //     }
//         //     else
//         //     {
//         //         // Calcular rebote en espejo
//         //         Vector2 inDirection = rb.velocity.normalized;
//         //         Vector2 inNormal = collision.contacts[0].normal;
//         //         
//         //         // Vector2.Reflect ya hace el rebote en espejo correcto
//         //         Vector2 reflectedDirection = Vector2.Reflect(inDirection, inNormal);
//         //         rb.velocity = reflectedDirection * speed;
//         //     }
//         // }
//         
//         // Si choca con pared
//         if (collision.gameObject.CompareTag("Wall"))
//         {
//             bounceCount++;
//             
//             if (bounceCount >= maxBounces)
//             { 
//                 hasCollided = true;
//                 SpawnCrackedEgg();
//                 Destroy(gameObject);
//             }
//             else
//             {
//                 // --- LÓGICA DE REBOTE MEJORADA ---
//                 
//                 Vector2 inDirection = rb.velocity.normalized;
//                 Vector2 inNormal = collision.contacts[0].normal;
//                 
//                 // 1. Calcular el vector reflejado
//                 Vector2 reflectedDirection = Vector2.Reflect(inDirection, inNormal);
//                 
//                 // 2. Aplicar la nueva velocidad, manteniendo la magnitud original (speed)
//                 rb.velocity = reflectedDirection * speed;
//
//                 // 3. APLICAR FUERZA DE SEPARACIÓN (CRÍTICO para ángulos de 90 grados)
//                 // Esto asegura que la bala no se atasque en la esquina.
//                 // 0.05f es un valor pequeño para empujar la bala fuera de la pared.
//                 rb.position += reflectedDirection * 0.05f; 
//             }
//         }
//     }
//
//     private void SpawnCrackedEgg()
//     {
//         if (eggCrackedPrefab == null) return;
//         
//         GameObject crackedEgg = Instantiate(eggCrackedPrefab, transform.position, Quaternion.identity);
//         Destroy(crackedEgg, crackedLifetime);
//     }
// }
//

// using UnityEngine;
//
// public class EggBullet : MonoBehaviour
// {
//     public float speed = 20f;
//     public int damage = 10;
//     public int maxBounces = 3;
//     
//     [Header("Egg Crack Effect")]
//     public GameObject eggCrackedPrefab; // El sprite del huevo roto
//     public float crackedLifetime = 4f;  // Duración del huevo roto
//     
//     private int bounceCount = 0;
//     private Rigidbody2D rb;
//
//     private void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         if (rb != null)
//         {
//             rb.velocity = transform.up * speed;
//         }
//         
//         // Ignorar colisión con el player que disparó
//         Player player = FindObjectOfType<Player>();
//         if (player != null)
//         {
//             Collider2D playerCollider = player.GetComponent<Collider2D>();
//             Collider2D bulletCollider = GetComponent<Collider2D>();
//         
//             if (playerCollider != null && bulletCollider != null)
//             {
//                 Physics2D.IgnoreCollision(bulletCollider, playerCollider);
//             }
//         }
//     }
//     
//     private void Update()
//     {
//         // Hacer que el sprite apunte en la dirección del movimiento
//         if (rb != null && rb.velocity.magnitude > 0.1f)
//         {
//             float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
//             transform.rotation = Quaternion.Euler(0, 0, angle);
//         }
//     }
//
//
//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         // Verificar si es damageable (enemigo)
//         IsDamageable damageable = collision.gameObject.GetComponent<IsDamageable>();
//         if (damageable != null)
//         {
//             damageable.TakeDamage(damage);
//             SpawnCrackedEgg();
//             Destroy(gameObject);
//             return;
//         }
//
//         // Si choca con pared
//         if (collision.gameObject.CompareTag("Wall"))
//         {
//             bounceCount++;
//             
//             if (bounceCount >= maxBounces)
//             {
//                 SpawnCrackedEgg();
//                 Destroy(gameObject);
//             }
//             else
//             {
//                 // Calcular rebote
//                 Vector2 inDirection = rb.velocity.normalized;
//                 Vector2 inNormal = collision.contacts[0].normal;
//                 Vector2 newVelocity = Vector2.Reflect(inDirection, inNormal) * speed;
//                 rb.velocity = newVelocity;
//                 
//                 // Rotar el bullet en la nueva dirección
//                 float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg - 90f;
//                 transform.rotation = Quaternion.Euler(0, 0, angle);
//             }
//         }
//     }
//
//     private void SpawnCrackedEgg()
//     {
//         if (eggCrackedPrefab == null)
//         {
//             return;
//         }
//         // Crear el huevo roto en la posición del impacto
//         GameObject crackedEgg = Instantiate(eggCrackedPrefab, transform.position, Quaternion.identity);
//         
//         // Destruir el huevo roto después del tiempo especificado
//         Destroy(crackedEgg, crackedLifetime);
//     }
// }