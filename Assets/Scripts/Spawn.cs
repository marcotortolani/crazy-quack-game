using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EnemySpawnData
{
    public int amount;
    public GameObject prefab;
}

public class Spawn : MonoBehaviour
{
    [Header("Enemigos Comunes")]
    public EnemySpawnData[] enemiesToSpawn;
    public float spawnTime;
    private float spawnTimeInit;

    [Header("Configuración Slime Boss")]
    public GameObject slimeBossPrefab;
    public float bossSpawnInterval = 30f; // Cada cuánto tiempo intenta spawnear uno
    public float bossStartDelay = 60f;    // Tiempo inicial antes de que aparezca el primero
    public int maxBossesInScene = 1;      // Máximo de Slimes gigantes vivos a la vez
    
    private float bossTimer;
    private float initialDelayTimer;

    public Vector3[] spawnPoints;

    private void Start()
    {
        spawnTimeInit = spawnTime;
        bossTimer = bossSpawnInterval;
        initialDelayTimer = bossStartDelay;
    }

    private void Update()
    {
        if (!GameManager.Instance) return;
        if (GameManager.Instance.playerIsDead || GameManager.Instance.playerIsWin) return;

        // --- Lógica Enemigos Comunes ---
        if (spawnTime > 0)
        {
            spawnTime -= Time.deltaTime;
        }
        else
        {
            spawnTime = spawnTimeInit;
            SpawnEnemies();
        }

        // --- Lógica Slime Boss ---
        HandleBossSpawn();
    }

    private void HandleBossSpawn()
    {
        // 1. Esperar el retraso inicial del juego
        if (initialDelayTimer > 0)
        {
            initialDelayTimer -= Time.deltaTime;
            return;
        }

        // 2. Timer para el siguiente spawn de boss
        bossTimer -= Time.deltaTime;

        if (bossTimer <= 0)
        {
            bossTimer = bossSpawnInterval;

            // 3. Verificar cuántos bosses hay en escena antes de spawnear
            // Buscamos objetos con el script SlimeEnemy que tengan isGiant = true
            int currentBosses = 0;
            SlimeEnemy[] allSlimes = FindObjectsOfType<SlimeEnemy>();
            foreach (var s in allSlimes) if (s.isGiant) currentBosses++;

            if (currentBosses < maxBossesInScene)
            {
                SpawnSingleBoss();
            }
        }
    }

    private void SpawnEnemies()
    {
        if (enemiesToSpawn == null || spawnPoints == null || spawnPoints.Length == 0) return;
        
        foreach (EnemySpawnData spawnData in enemiesToSpawn)
        {
            if (spawnData == null || spawnData.prefab == null) continue;
            
            for (int i = 0; i < spawnData.amount; i++)
            {
                SpawnAtRandomPoint(spawnData.prefab);
            }
        }
    }

    private void SpawnSingleBoss()
    {
        if (slimeBossPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;
        
        Debug.Log("¡Un Slime Gigante ha aparecido!");
        SpawnAtRandomPoint(slimeBossPrefab);
    }

    private void SpawnAtRandomPoint(GameObject prefab)
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Instantiate(prefab, spawnPoints[randomIndex], Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        Gizmos.color = Color.red;
        foreach (Vector3 pt in spawnPoints) Gizmos.DrawSphere(pt, 0.3f);
    }
}


// using System;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// [Serializable]
// public class EnemySpawnData
// {
//     public int amount;
//     public GameObject prefab;
// }
//
// public class Spawn : MonoBehaviour
// {
//     public EnemySpawnData[] enemiesToSpawn;
//
//     public float spawnTime;
//     public float spawnTimeInit;
//     public Vector3[] spawnPoints;
//
//     private void Start()
//     {
//         spawnTimeInit = spawnTime;
//     }
//
//     private void Update()
//     {
//         // Spawnear enemigos mientras el Player esta vivo o todavia no ganó
//         if (!GameManager.Instance) return;
//         if (!GameManager.Instance.playerIsDead && !GameManager.Instance.playerIsWin)
//         {
//             if (spawnTime > 0)
//             {
//                 spawnTime -= Time.deltaTime; // countdown del spawnTime
//             }
//             else
//             {
//                 spawnTime = spawnTimeInit;
//                 SpawnEnemies();
//             }
//         }
//     }
//
//     private void OnDrawGizmosSelected()
//     {
//         if (spawnPoints == null || spawnPoints.Length == 0) return;
//         
//         Gizmos.color = Color.red;
//         foreach (Vector3 spawnPoint in spawnPoints)
//         {
//             Gizmos.DrawSphere(spawnPoint, 0.2f * spawnTime / spawnTime);
//         };
//     }
//
//     private void SpawnEnemies()
//     {
//         if (enemiesToSpawn == null || spawnPoints == null || spawnPoints.Length == 0) return;
//         
//         // recorre el array de todos los enemigos a spawnear
//         foreach (EnemySpawnData spawnData in enemiesToSpawn)
//         {
//             // Verificar que el prefab existe antes de spawnear
//             if (spawnData == null || spawnData.prefab == null)
//             {
//                 Debug.LogWarning("Spawn: prefab es null, saltando...");
//                 continue;
//             }
//             
//             // por cada enemy, spawnea la cantidad necesaria en ubicaciones random
//             for (int i = 0; i < spawnData.amount; i++)
//             {
//                 int randomPosition = Random.Range(0, spawnPoints.Length);
//                 //Instantiate(spawnData.prefab, spawnPoints[randomPosition], transform.rotation);
//                 // Verificar que el spawn point es válido
//                 if (randomPosition >= 0 && randomPosition < spawnPoints.Length)
//                 {
//                     GameObject spawnedEnemy = Instantiate(spawnData.prefab, spawnPoints[randomPosition], Quaternion.identity);
//                 
//                     // Verificar que se instanció correctamente
//                     if (spawnedEnemy == null)
//                     {
//                         Debug.LogError("Spawn: Falló al instanciar enemigo!");
//                     }
//                 }
//             }
//         }
//     }
// }