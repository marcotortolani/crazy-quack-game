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
    public EnemySpawnData[] enemiesToSpawn;

    public float spawnTime;
    public float spawnTimeInit;
    public Vector3[] spawnPoints;

    private void Start()
    {
        spawnTimeInit = spawnTime;
    }

    private void Update()
    {
        // Spawnear enemigos mientras el Player esta vivo o todavia no ganó
        if (!GameManager.Instance) return;
        if (!GameManager.Instance.playerIsDead && !GameManager.Instance.playerIsWin)
        {
            if (spawnTime > 0)
            {
                spawnTime -= Time.deltaTime; // countdown del spawnTime
            }
            else
            {
                spawnTime = spawnTimeInit;
                SpawnEnemies();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        
        Gizmos.color = Color.red;
        foreach (Vector3 spawnPoint in spawnPoints)
        {
            Gizmos.DrawSphere(spawnPoint, 0.2f * spawnTime / spawnTime);
        };
    }

    private void SpawnEnemies()
    {
        if (enemiesToSpawn == null || spawnPoints == null || spawnPoints.Length == 0) return;
        
        // recorre el array de todos los enemigos a spawnear
        foreach (EnemySpawnData spawnData in enemiesToSpawn)
        {
            // Verificar que el prefab existe antes de spawnear
            if (spawnData == null || spawnData.prefab == null)
            {
                Debug.LogWarning("Spawn: prefab es null, saltando...");
                continue;
            }
            
            // por cada enemy, spawnea la cantidad necesaria en ubicaciones random
            for (int i = 0; i < spawnData.amount; i++)
            {
                int randomPosition = Random.Range(0, spawnPoints.Length);
                //Instantiate(spawnData.prefab, spawnPoints[randomPosition], transform.rotation);
                // Verificar que el spawn point es válido
                if (randomPosition >= 0 && randomPosition < spawnPoints.Length)
                {
                    GameObject spawnedEnemy = Instantiate(spawnData.prefab, spawnPoints[randomPosition], Quaternion.identity);
                
                    // Verificar que se instanció correctamente
                    if (spawnedEnemy == null)
                    {
                        Debug.LogError("Spawn: Falló al instanciar enemigo!");
                    }
                }
            }
        }
    }
}