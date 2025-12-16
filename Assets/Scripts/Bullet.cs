using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Propiedades heredables
    public float bulletSpeed = 20f;
    public int bulletDamage = 10;
    public int maxBounces = 3;
    
    // Variables internas
    protected int currentBounces = 0; 
    protected Rigidbody2D rb;
    
    // Usado para para inicializar el Rigidbody
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            // Establecer la velocidad inicial con el vector forward de la bala
            // (La rotación fue establecida por el Player al instanciarla)
            rb.velocity = transform.up * bulletSpeed; 
        }
        
        // Ignorar colisión con el player
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Collider2D bulletCollider = GetComponent<Collider2D>();
        
            if (playerCollider != null && bulletCollider != null)
            {
                // Ignorar colisión a nivel de componentes
                Physics2D.IgnoreCollision(bulletCollider, playerCollider);
            }
        }
    }
    
    protected virtual void Update()
    {
        // Hacer que el sprite apunte en la dirección del movimiento
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            // Ajuste de -90f para que el sprite (que apunta hacia arriba) siga el vector de velocidad
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f; 
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else if (rb == null)
        {
            // Fallback si no hay Rigidbody (aunque el Start requiere uno)
            transform.position += transform.up * (bulletSpeed * Time.deltaTime);
        }
    }

    // Usamos OnCollisionEnter2D para manejar colisiones de la física
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Colisión con Pared (Lógica de Rebote)
        if (collision.gameObject.CompareTag("Wall"))
        {
            currentBounces++;
        
            if (currentBounces >= maxBounces)
            {
                OnDestroyAction();
                return;
            }
        
            // --- LÓGICA DE REBOTE CON IMPULSO ---
        
            // La velocidad de entrada y la velocidad deseada después del rebote
            Vector2 velocityIn = rb.velocity;
            Vector2 inNormal = collision.GetContact(0).normal;
        
            // 1. Calcular el vector reflejado (Out)
            // La fórmula del rebote es: V_out = V_in - 2 * (V_in . N) * N
            Vector2 velocityOut = Vector2.Reflect(velocityIn, inNormal);

            // 2. Calcular el cambio de velocidad (Delta V): Fuerza = Delta V * Mass
            Vector2 deltaVelocity = velocityOut - velocityIn;
        
            // Aplicar la fuerza de impulso para el cambio de velocidad.
            // La magnitud del impulso debe ser suficiente para superar la resistencia de la colisión.
            rb.AddForce(deltaVelocity * rb.mass, ForceMode2D.Impulse); 
        
            // Opcional: Empuje extra de separación (si es necesario)
            // rb.position += velocityOut.normalized * 0.1f;
        
            // Mantenemos la velocidad de rebote de la bala fija
            if (rb.velocity.magnitude != bulletSpeed)
            {
                rb.velocity = rb.velocity.normalized * bulletSpeed;
            }
        
            return; 
        }
        // if (collision.gameObject.CompareTag("Wall"))
        // {
        //     currentBounces++;
        //     
        //     if (currentBounces >= maxBounces)
        //     {
        //         OnDestroyAction(); // Destrucción
        //         return;
        //     }
        //     
        //     // --- LÓGICA DE REBOTE EN ESPEJO (SOLUCIÓN 90 GRADOS) ---
        //     
        //     // Usar la velocidad actual para el cálculo
        //     Vector2 inDirection = rb.velocity.normalized; 
        //     
        //     // Obtener la normal del punto de contacto
        //     Vector2 inNormal = collision.GetContact(0).normal; 
        //     
        //     // Calcular el vector reflejado
        //     // Vector2 reflectedDirection = Vector2.Reflect(inDirection, inNormal);
        //     //
        //     // // // Aplicar la nueva velocidad, manteniendo la magnitud.
        //     // // rb.velocity = reflectedDirection * bulletSpeed;
        //     // //
        //     // // // FUERZA DE SEPARACIÓN: Mover la bala fuera de la pared para evitar atascos.
        //     // // rb.position += reflectedDirection * 0.05f;
        //     // rb.velocity = Vector2.zero; // Detener la bala momentáneamente
        //     // rb.AddForce(reflectedDirection * bulletSpeed * rb.mass * 2, ForceMode2D.Impulse); // Usa impulso
        //     // rb.position += reflectedDirection * 0.2f; // Mantener el empuje de separación
        //     
        //     return; // Terminar si es una pared
        // }

        // 2. Colisión con Dañable (Enemigo, etc.)
        // Esto reemplaza todas las llamadas específicas a PigEnemy, MushroomEnemy, etc.
        IsDamageable damageable = collision.gameObject.GetComponent<IsDamageable>();
        if (damageable != null)
        {
            // Si el objeto tiene la interfaz, aplicar daño.
            damageable.TakeDamage(bulletDamage);
            OnDestroyAction(); // Destrucción (o acción de la clase hija)
            return;
        }
        
        // Si tienes objetos que no son Walls ni Damageable (ej. obstáculos móviles), 
        // puedes añadir lógica aquí, pero por ahora se ignoran si no tienen la interfaz.
    }

    // Método virtual para ser sobrescrito por las clases hijas (ej. EggBullet)
    protected virtual void OnDestroyAction()
    {
        Destroy(gameObject);
    }
}


// using UnityEngine;
//
// public class Bullet : MonoBehaviour
// {
//     public float bulletSpeed = 20f;
//     public int bulletDamage = 10;
//     public int maxBounces = 3;
//     
//     protected int currentBounces = 0;
//     protected Rigidbody2D rb;
//
//     // Sólo para para inicializar el Rigidbody
//     protected virtual void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         if (rb != null)
//         {
//             // Establecer la velocidad inicial
//             rb.velocity = transform.up * bulletSpeed;
//         }
//         
//         // Ignorar colisión con el player (Buena práctica heredada de EggBullet)
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
//         // transform.position += transform.up * (bulletSpeed * Time.deltaTime);
//         // Hacer que el sprite apunte en la dirección del movimiento (Heredado de EggBullet)
//         if (rb != null && rb.velocity.magnitude > 0.1f)
//         {
//             float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
//             transform.rotation = Quaternion.Euler(0, 0, angle);
//         }
//     }
//
//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         // Colision con paredes, destruir balas despues de X rebotes
//         if (collision.gameObject.CompareTag("Wall"))
//         {
//             currentBounces++;
//             if (currentBounces >= maxBounces)
//             {
//                 Destroy(gameObject);
//                 return;
//             }
//             
//             // Obtener la normal del punto de contacto
//             Vector3 normal = collision.contacts[0].normal;
//             Vector3 incomingDirection = transform.up;
//             Vector3 reflectedDirection = Vector3.Reflect(incomingDirection, normal);
//             
//             // Rotar la bala en la nueva dirección
//             float angle = Mathf.Atan2(reflectedDirection.y, reflectedDirection.x) * Mathf.Rad2Deg;
//             transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 porque transform.up es la dirección
//             
//             return;
//             
//         }
//
//         // Colision con enemigos
//         // Enemy enemy = collision.gameObject.GetComponent<Enemy>();
//         // if (enemy != null)
//         // {
//         //     enemy.TakeDamage(bulletDamage);
//         //     Destroy(gameObject);
//         // }
//         
//         // Colision con enemigos (usando Tag)
//         if (collision.gameObject.CompareTag("Enemy"))
//         {
//             // Intentar con el script Enemy genérico
//             Enemy enemy = collision.gameObject.GetComponent<Enemy>();
//             if (enemy != null)
//             {
//                 enemy.TakeDamage(bulletDamage);
//                 Destroy(gameObject);
//                 return;
//             }
//
//             // Intentar con PigEnemy
//             PigEnemy pigEnemy = collision.gameObject.GetComponent<PigEnemy>();
//             if (pigEnemy != null)
//             {
//                 pigEnemy.TakeDamage(bulletDamage);
//                 Destroy(gameObject);
//                 return;
//             }
//
//             // Intentar con MushroomEnemy
//             MushroomEnemy mushroomEnemy = collision.gameObject.GetComponent<MushroomEnemy>();
//             if (mushroomEnemy != null)
//             {
//                 mushroomEnemy.TakeDamage(bulletDamage);
//                 Destroy(gameObject);
//                 return;
//             }
//             
//             PlantEnemy plantEnemy = collision.gameObject.GetComponent<PlantEnemy>();
//             if (plantEnemy != null)
//             {
//                 plantEnemy.TakeDamage(bulletDamage);
//                 Destroy(gameObject);
//                 return;
//             }
//         }
//     }
//     
// }