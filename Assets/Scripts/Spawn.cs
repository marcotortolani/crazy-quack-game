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
        Gizmos.color = Color.red;
        foreach (Vector3 spawnPoint in spawnPoints) Gizmos.DrawSphere(spawnPoint, 0.2f * spawnTime / spawnTime);
    }

    private void SpawnEnemies()
    {
        if (enemiesToSpawn == null || spawnPoints == null || spawnPoints.Length == 0) return;
        
        // recorre el array de todos los enemigos a spawnear
        foreach (EnemySpawnData spawnData in enemiesToSpawn)

            // por cada enemy, spawnea la cantidad necesaria en ubicaciones random
            for (int i = 0; i < spawnData.amount; i++)
            {
                int randomPosition = Random.Range(0, spawnPoints.Length);
                Instantiate(spawnData.prefab, spawnPoints[randomPosition], transform.rotation);
            }
    }
}