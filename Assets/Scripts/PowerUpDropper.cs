using UnityEngine;

[System.Serializable]
public class PowerUpDropData
{
    public GameObject powerUpPrefab;
    public int dropCount;           // Cuántos se van a dropear en total
    [Range(0f, 100f)]
    public float dropChance = 20f;  // % de probabilidad de drop al matar enemigo
    
    [HideInInspector]
    public int droppedCount = 0;    // Contador de cuántos se han dropeado
}

public class PowerUpDropper : MonoBehaviour
{
    public static PowerUpDropper Instance;
    
    [Header("PowerUp Drop Configuration")]
    public PowerUpDropData[] powerUpDrops;
    
    [Header("Drop Settings")]
    public float dropRadius = 0.5f; // Radio de spawn alrededor de la posición del enemigo
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
    
        Instance = this;
    }
    
    public void TryDropPowerUp(Vector3 position)
    {
        // Recorrer todos los tipos de power-ups configurados
        foreach (var dropData in powerUpDrops)
        {
            // Verificar si ya se dropeó la cantidad máxima
            if (dropData.droppedCount >= dropData.dropCount)
                continue;
            
            // Calcular chance
            float randomValue = Random.Range(0f, 100f);
            
            if (randomValue <= dropData.dropChance)
            {
                // Dropear el power-up
                Vector3 spawnPosition = position + (Vector3)Random.insideUnitCircle * dropRadius;
                spawnPosition.z = 0; // Asegurar que esté en 2D
                
                GameObject powerUp = Instantiate(dropData.powerUpPrefab, spawnPosition, Quaternion.identity);
                dropData.droppedCount++;
                
                break;
            }
        }
    }
    
    // Método para resetear contadores (útil al reiniciar nivel)
    public void ResetDropCounts()
    {
        foreach (var dropData in powerUpDrops)
        {
            dropData.droppedCount = 0;
        }
    }
}