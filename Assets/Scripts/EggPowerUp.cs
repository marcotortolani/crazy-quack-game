using UnityEngine;

public enum EggType
{
    Normal,
    Fire,
    Radioactive
}

public class EggPowerUp : MonoBehaviour
{
    public EggType eggType;
    public int shotsAvailable = 10; // Cuántos disparos con este huevo
    
    [Header("Visual")]
    public float floatSpeed = 0.5f;
    public float floatAmount = 0.3f;
    
    private Vector3 startPosition;
    private float timeOffset;
    
    private void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
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
                player.ChangeEggType(eggType, shotsAvailable);
                
                // Opcional: sonido
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("EggPowerUpCollected");
                }
                
                Destroy(gameObject);
            }
        }
    }
}