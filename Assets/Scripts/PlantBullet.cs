using UnityEngine;

public class PlantBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 8f;
    public int damage = 1;
    public float lifeTime = 5f;
    
    [Header("Impact Effect")]
    public GameObject bulletPiecesPrefab; // El sprite que queda en el suelo
    public float piecesLifeTime = 3f; // Tiempo antes de que desaparezcan los pedazos

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += transform.up * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool shouldDestroy = false;
        
        // Colisión con el player
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            shouldDestroy = true;
        }
        
        // Colisión con paredes
        if (collision.CompareTag("Wall"))
        {
            shouldDestroy = true;
        }
        
        // Si debe destruirse, crear los pedazos
        if (shouldDestroy)
        {
            CreateBulletPieces();
            Destroy(gameObject);
        }
    }

    private void CreateBulletPieces()
    {
        if (bulletPiecesPrefab == null) return;
        
        // Instanciar los pedazos en la posición del impacto
        GameObject pieces = Instantiate(bulletPiecesPrefab, transform.position, Quaternion.identity);
        
        // Destruir los pedazos después de un tiempo
        Destroy(pieces, piecesLifeTime);
    }
}