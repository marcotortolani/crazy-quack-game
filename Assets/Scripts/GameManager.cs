using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class EggDropCondition
{
    public string conditionName;
    public EggType eggType;
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
    public Sprite enemyIcon; // Para mostrar en el HUD
}




public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Level Objectives")]
    public EnemyKillObjective[] killObjectives; // Configurable por nivel
    public int secondsToSurvive = 60; // 1 minuto por defecto
    
    [Header("Game State")]
    public int secondsAlive = 0;
    public bool playerIsDead = false;
    public bool playerIsWin = false;

    private float _timeCounter = 0f;
    private bool _hasShownWinMessage = false;
    
    [Header("Egg Drop Conditions")]
    public EggDropCondition[] eggDropConditions;

    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        // if (Instance == null)
        // {
        //     Instance = this;
        //     //DontDestroyOnLoad(gameObject);
        // }
        // else
        // {
        //     Destroy(gameObject);
        // }
    }
    
    void Update()
    {
        if (Instance != this) return;
        
        UpdateTime();

        // Verificar condiciones de victoria (OR entre las dos condiciones)
        if (CheckVictoryConditions() && !playerIsWin && !playerIsDead)
        {
            PlayerWin();
        }
        
        // Verificar condiciones de drop de huevos especiales
        CheckEggDropConditions();
    }

    private void UpdateTime()
    {
        if (!playerIsDead && !playerIsWin)
        {
            _timeCounter += Time.deltaTime; 
            secondsAlive = (int)_timeCounter;
        }
    }

    // Método para registrar muerte de un enemigo específico
    public void RegisterEnemyKill(string enemyType)
    {
        // Verificación de seguridad
        if (Instance != this) return;
        if (killObjectives == null) return;
        
        foreach (var objective in killObjectives)
        {
            if (objective.enemyName == enemyType)
            {
                objective.currentKills++;
                break;
            }
        }
    }
    
    // Verificar si se completó AL MENOS UNA condición de victoria
    private bool CheckVictoryConditions()
    {
        // Verificación de seguridad
        if (Instance != this) return false;
        if (killObjectives == null) return false;
        
        // CONDICIÓN 1: Sobrevivir el tiempo requerido
        if (secondsAlive >= secondsToSurvive)
        {
            return true;
        }

        // CONDICIÓN 2: Completar TODOS los objetivos individuales de kills
        // (no el total, sino cada uno por separado)
        if (AreAllIndividualObjectivesComplete())
        {
            return true;
        }
    
        return false;
    }

    // Verificar si TODOS los objetivos individuales están completos
    // Ejemplo: 15/15 gallinas AND 10/10 conejos AND 5/5 hongos
    public bool AreAllIndividualObjectivesComplete()
    {
        // Verificación de seguridad
        if (Instance != this) return false;
        if (killObjectives == null || killObjectives.Length == 0) return false;
        
        if (killObjectives == null || killObjectives.Length == 0)
            return false;
        
        foreach (var objective in killObjectives)
        {
            if (objective.currentKills < objective.killsRequired)
            {
                // Si falta completar aunque sea UN objetivo, retornar false
                return false;
            }
        }
        
        // Todos los objetivos individuales están completos
        return true;
    }

    // Obtener progreso total (SOLO para mostrar en UI, NO para condición de victoria)
    public int GetTotalKillsRequired()
    {
        int total = 0;
        foreach (var objective in killObjectives)
        {
            total += objective.killsRequired;
        }
        return total;
    }

    public int GetTotalCurrentKills()
    {
        int total = 0;
        foreach (var objective in killObjectives)
        {
            total += objective.currentKills;
        }
        return total;
    }

    private void PlayerWin()
    {
        if (!_hasShownWinMessage)
        {
            playerIsWin = true;
            _hasShownWinMessage = true;
            PrintStatus();
        }
    }
    
    private void CheckEggDropConditions()
    {
        if (Instance != this) return;
        if (eggDropConditions == null || eggDropConditions.Length == 0) return;
        if (killObjectives == null || killObjectives.Length == 0) return;
        
        foreach (var condition in eggDropConditions)
        {
            // Si ya se dropeó este huevo, saltar
            if (condition.hasDropped) continue;
            
            // Verificar si se cumplen todos los requisitos
            bool allRequirementsMet = true;
            
            foreach (var requirement in condition.requirements)
            {
                bool requirementMet = false;
                
                foreach (var objective in killObjectives)
                {
                    if (objective.enemyName == requirement.enemyName)
                    {
                        if (objective.currentKills >= requirement.killsRequired)
                        {
                            requirementMet = true;
                        }
                        break;
                    }
                }
                
                if (!requirementMet)
                {
                    allRequirementsMet = false;
                    break;
                }
            }
            
            // Si se cumplen todos los requisitos, dropear INMEDIATAMENTE
            if (allRequirementsMet)
            {
                condition.hasDropped = true;
                
                if (EggDropper.Instance != null)
                {
                    // Buscar un enemigo vivo aleatorio para dropear el huevo
                    Enemy[] enemies = FindObjectsOfType<Enemy>();
                    
                    if (enemies.Length > 0)
                    {
                        // Elegir un enemigo aleatorio
                        Enemy randomEnemy = enemies[Random.Range(0, enemies.Length)];
                        Vector3 dropPosition = randomEnemy.transform.position;
                        
                        EggDropper.Instance.DropSpecialEgg(condition.eggType, dropPosition);
                    }
                    else
                    {
                        // Si no hay enemigos, dropear en el centro de la pantalla o cerca del player
                        Player player = FindObjectOfType<Player>();
                        Vector3 dropPosition = player != null ? player.transform.position : Vector3.zero;
                        
                        EggDropper.Instance.DropSpecialEgg(condition.eggType, dropPosition);
                    }
                }
            }
        }
    }
    
    // Método para resetear el estado del juego
    public void ResetGameState()
    {
        // Resetear variables de estado
        secondsAlive = 0;
        playerIsDead = false;
        playerIsWin = false;
        _timeCounter = 0f;
        _hasShownWinMessage = false;
    
        // Resetear contadores de kills
        foreach (var objective in killObjectives)
        {
            objective.currentKills = 0;
        }
        
        // Resetear power-ups dropeados
        if (PowerUpDropper.Instance != null)
        {
            PowerUpDropper.Instance.ResetDropCounts();
        }
        
        // Resetear condiciones de drop de huevos
        foreach (var condition in eggDropConditions)
        {
            condition.hasDropped = false;
        }
        
        if (EggDropper.Instance != null)
        {
            EggDropper.Instance.ResetEggDrops();
        }
    }

    private void OnDestroy()
    {
        // Limpiar la referencia si este es el Instance actual
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void PrintStatus()
    {
        foreach (var objective in killObjectives)
        {
            string status = objective.currentKills >= objective.killsRequired ? "✓ COMPLETO" : "✗ Incompleto";
            
        }
    }
}