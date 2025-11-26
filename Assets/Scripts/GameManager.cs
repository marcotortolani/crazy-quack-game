using UnityEngine;

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
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        UpdateTime();

        // Verificar condiciones de victoria (OR entre las dos condiciones)
        if (CheckVictoryConditions() && !playerIsWin && !playerIsDead)
        {
            PlayerWin();
        }
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
        if (killObjectives == null) return; 
        
        foreach (var objective in killObjectives)
        {
            if (objective.enemyName == enemyType)
            {
                objective.currentKills++;
                Debug.Log($"{enemyType} eliminado. Progreso: {objective.currentKills}/{objective.killsRequired}");
                break;
            }
        }
    }
    
    // Verificar si se completó AL MENOS UNA condición de victoria
    private bool CheckVictoryConditions()
    {
        // CONDICIÓN 1: Sobrevivir el tiempo requerido
        if (secondsAlive >= secondsToSurvive)
        {
            Debug.Log("Victoria por tiempo sobrevivido!");
            return true;
        }

        // CONDICIÓN 2: Completar TODOS los objetivos individuales de kills
        // (no el total, sino cada uno por separado)
        if (AreAllIndividualObjectivesComplete())
        {
            Debug.Log("Victoria por completar TODOS los objetivos individuales!");
            return true;
        }
    
        return false;
    }

    // Verificar si TODOS los objetivos individuales están completos
    // Ejemplo: 15/15 gallinas AND 10/10 conejos AND 5/5 hongos
    public bool AreAllIndividualObjectivesComplete()
    {
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
            
            Debug.Log("¡VICTORIA!");
            PrintStatus();
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
    
        Debug.Log("GameManager reseteado");
    }
    
    private void PrintStatus()
    {
        Debug.Log(">> Estado del nivel:");
        Debug.Log($">> Tiempo sobrevivido: {secondsAlive}/{secondsToSurvive} segundos");
        Debug.Log(">> Objetivos individuales:");
        
        foreach (var objective in killObjectives)
        {
            string status = objective.currentKills >= objective.killsRequired ? "✓ COMPLETO" : "✗ Incompleto";
            Debug.Log($"   - {objective.enemyName}: {objective.currentKills}/{objective.killsRequired} {status}");
        }
        
        Debug.Log($">> Total de kills: {GetTotalCurrentKills()}/{GetTotalKillsRequired()} (informativo)");
    }
}