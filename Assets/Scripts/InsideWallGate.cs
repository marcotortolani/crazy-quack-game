using UnityEngine;

public class InsideWallGate : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verificar si el objeto que entra es un Enemigo (Tag: "Enemy")
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            // 2. Si es un enemigo y aún no está marcado como 'dentro'
            if (enemy != null && !enemy.IsInsideArena)
            {
                // 3. Llamar a la función del enemigo para activar la colisión con los muros
                enemy.ActivateWallCollision();
            }
        }
    }
}