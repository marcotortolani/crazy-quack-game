using System;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class PowerUpDropCondition // Ante era EggDropCondition
{
    public string conditionName;
    public EggType bulletType; // Se mantiene EggType para no romper los Enums actuales
    public EnemyKillRequirement[] requirements;
    public bool hasDropped = false;
}

[System.Serializable]
public class EnemyKillRequirement
{
    public string enemyName;
    public int killsRequired;
}

[System.Serializable]
public class EnemyKillObjective
{
    public string enemyName;
    public int killsRequired;
    public int currentKills;
    public Sprite enemyIcon; 
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Level Objectives")]
    public EnemyKillObjective[] killObjectives; 
    public int secondsToSurvive = 60; 
    
    [Header("Game State")]
    public int secondsAlive = 0;
    public bool playerIsDead = false;
    public bool playerIsWin = false;

    private float _timeCounter = 0f;
    private bool _hasShownWinMessage = false;
    
    [Header("Power Up Drop Conditions")]
    public PowerUpDropCondition[] powerUpDropConditions; // Renombrado

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
    
    void Update()
    {
        if (Instance != this) return;
        
        UpdateTime();

        if (CheckVictoryConditions() && !playerIsWin && !playerIsDead)
        {
            PlayerWin();
        }
        
        CheckPowerUpDropConditions(); // Lógica genérica
    }

    private void UpdateTime()
    {
        if (!playerIsDead && !playerIsWin)
        {
            _timeCounter += Time.deltaTime; 
            secondsAlive = (int)_timeCounter;
        }
    }

    public void RegisterEnemyKill(string enemyType)
    {
        if (Instance != this || killObjectives == null) return;
        
        foreach (var objective in killObjectives)
        {
            if (objective.enemyName == enemyType)
            {
                objective.currentKills++;
                break;
            }
        }
    }
    
    private bool CheckVictoryConditions()
    {
        if (Instance != this || killObjectives == null) return false;
        
        if (secondsAlive >= secondsToSurvive) return true;

        return AreAllIndividualObjectivesComplete();
    }

    public bool AreAllIndividualObjectivesComplete()
    {
        if (Instance != this || killObjectives == null || killObjectives.Length == 0) return false;
        
        foreach (var objective in killObjectives)
        {
            if (objective.currentKills < objective.killsRequired) return false;
        }
        return true;
    }

    public int GetTotalKillsRequired()
    {
        int total = 0;
        foreach (var objective in killObjectives) total += objective.killsRequired;
        return total;
    }

    public int GetTotalCurrentKills()
    {
        int total = 0;
        foreach (var objective in killObjectives) total += objective.currentKills;
        return total;
    }

    private void PlayerWin()
    {
        if (!_hasShownWinMessage)
        {
            playerIsWin = true;
            _hasShownWinMessage = true;
        }
    }
    
    private void CheckPowerUpDropConditions() // Método renombrado y limpio
    {
        if (Instance != this || powerUpDropConditions == null) return;
        
        foreach (var condition in powerUpDropConditions)
        {
            if (condition.hasDropped) continue;
            
            bool allRequirementsMet = true;
            foreach (var req in condition.requirements)
            {
                bool reqMet = false;
                foreach (var obj in killObjectives)
                {
                    if (obj.enemyName == req.enemyName && obj.currentKills >= req.killsRequired)
                    {
                        reqMet = true;
                        break;
                    }
                }
                if (!reqMet) { allRequirementsMet = false; break; }
            }
            
            if (allRequirementsMet)
            {
                condition.hasDropped = true;
                SpawnPowerUp(condition.bulletType);
            }
        }
    }

    private void SpawnPowerUp(EggType type)
    {
        // Esta función ahora centraliza el spawn independientemente de si es huevo o roca
        if (EggDropper.Instance != null)
        {
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            Vector3 dropPosition;

            if (enemies.Length > 0)
            {
                dropPosition = enemies[Random.Range(0, enemies.Length)].transform.position;
            }
            else
            {
                Player player = FindObjectOfType<Player>();
                dropPosition = player != null ? player.transform.position : Vector3.zero;
            }

            // El EggDropper debe ser el que tenga los prefabs (huevos en lvl 1, rocas en lvl 2)
            EggDropper.Instance.DropSpecialEgg(type, dropPosition);
        }
    }
    
    public void ResetGameState()
    {
        secondsAlive = 0;
        playerIsDead = false;
        playerIsWin = false;
        _timeCounter = 0f;
        _hasShownWinMessage = false;
    
        foreach (var objective in killObjectives) objective.currentKills = 0;
        
        if (PowerUpDropper.Instance != null) PowerUpDropper.Instance.ResetDropCounts();
        
        foreach (var condition in powerUpDropConditions) condition.hasDropped = false;
        
        if (EggDropper.Instance != null) EggDropper.Instance.ResetEggDrops();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}