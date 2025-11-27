using UnityEngine;

[System.Serializable]
public class EggDropData
{
    public EggType eggType;
    public GameObject eggPowerUpPrefab;
    public bool hasDropped = false; // Para controlar que solo dropee una vez
}

public class EggDropper : MonoBehaviour
{
    public static EggDropper Instance;
    
    [Header("Egg Drop Configuration")]
    public EggDropData[] eggDrops;
    
    [Header("Drop Settings")]
    public float dropRadius = 0.5f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // public void TryDropSpecialEgg(Vector3 position)
    // {
    //     foreach (var eggDrop in eggDrops)
    //     {
    //         // Solo intentar drop si está habilitado
    //         if (!eggDrop.isEnabled) continue;
    //         
    //         // Tirar dado de probabilidad
    //         float randomValue = Random.Range(0f, 100f);
    //         
    //         if (randomValue <= eggDrop.dropChance)
    //         {
    //             // Dropear el huevo especial
    //             Vector3 spawnPosition = position + (Vector3)Random.insideUnitCircle * dropRadius;
    //             spawnPosition.z = 0;
    //             
    //             GameObject egg = Instantiate(eggDrop.eggPowerUpPrefab, spawnPosition, Quaternion.identity);
    //             
    //             Debug.Log($"Huevo especial dropeado: {eggDrop.eggType}");
    //             
    //             // Solo dropear uno
    //             break;
    //         }
    //     }
    // }
    
    // public void EnableEggDrop(EggType eggType)
    // {
    //     foreach (var eggDrop in eggDrops)
    //     {
    //         if (eggDrop.eggType == eggType)
    //         {
    //             eggDrop.isEnabled = true;
    //             Debug.Log($"{eggType} egg drop habilitado!");
    //             break;
    //         }
    //     }
    // }
    
    public void DropSpecialEgg(EggType eggType, Vector3 position)
    {
        foreach (var eggDrop in eggDrops)
        {
            if (eggDrop.eggType == eggType)
            {
                if (eggDrop.hasDropped)
                {
                    return;
                }
            
                Vector3 spawnPosition = position + (Vector3)Random.insideUnitCircle * dropRadius;
                spawnPosition.z = 0;
            
                GameObject egg = Instantiate(eggDrop.eggPowerUpPrefab, spawnPosition, Quaternion.identity);
            
                // DEBUG: Verificar qué se instanció
                EggPowerUp powerUp = egg.GetComponent<EggPowerUp>();
                if (powerUp != null)
                {
                    Debug.Log($"Huevo instanciado - Tipo: {powerUp.eggType}, Shots: {powerUp.shotsAvailable}"); // ← NUEVO
                }
                else
                {
                    Debug.LogError("El prefab no tiene EggPowerUp script!"); // ← NUEVO
                }
            
                // Aplicar escala configurada
                //egg.transform.localScale = eggDrop.spawnScale;
            
                eggDrop.hasDropped = true;
            
                Debug.Log($"Huevo especial dropeado: {eggType}");
                break;
            }
        }
    }
    
    public void ResetEggDrops()
    {
        foreach (var eggDrop in eggDrops)
        {
            eggDrop.hasDropped = false;
        }
    }
}