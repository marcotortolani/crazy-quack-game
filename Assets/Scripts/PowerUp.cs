using UnityEngine;

public enum PowerUpType
{
    Health,    // Kiwi - suma vida
    Speed,     // Uvas - suma velocidad
    FireRate   // Manzana - aumenta disparo
}

public class PowerUp : MonoBehaviour
{
    public PowerUpType type;
    
    [Header("Effects")]
    public int healthAmount = 2;           // Vida que restaura el Kiwi
    public float speedBoost = 1.5f;        // Multiplicador de velocidad de las Uvas
    public float speedDuration = 5f;       // Duración del boost de velocidad
    public int fireRateIncrease = 2;       // Cuánto aumenta el disparo la Manzana
    
    [Header("Visual")]
    public float floatSpeed = 0.5f;        // Velocidad de flotación
    public float floatAmount = 0.3f;       // Amplitud de flotación
    
    private Vector3 startPosition;
    private float timeOffset;
    
    private void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI); // Offset aleatorio para variedad
    }
    
    private void Update()
    {
        // Efecto de flotación
        float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Rotación suave
        transform.Rotate(Vector3.forward, 50f * Time.deltaTime);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                ApplyPowerUp(player);
                
                // Opcional: efecto visual o sonido
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("PowerUpCollected");
                }
                
                Destroy(gameObject);
            }
        }
    }
    
    private void ApplyPowerUp(Player player)
    {
        switch (type)
        {
            case PowerUpType.Health:
                player.AddLife(healthAmount);
                Debug.Log($"PowerUp: +{healthAmount} vida");
                break;
                
            case PowerUpType.Speed:
                player.ApplySpeedBoost(speedBoost, speedDuration);
                Debug.Log($"PowerUp: +{speedBoost}x velocidad por {speedDuration}s");
                break;
                
            case PowerUpType.FireRate:
                player.IncreaseFireRate(fireRateIncrease);
                Debug.Log($"PowerUp: +{fireRateIncrease} balas/segundo");
                break;
        }
    }
}