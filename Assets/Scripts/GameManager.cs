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

        // Verificar condiciones de victoria
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

        // CONDICIÓN 2: Eliminar todos los enemigos requeridos
        bool allEnemiesKilled = true;
        foreach (var objective in killObjectives)
        {
            if (objective.currentKills < objective.killsRequired)
            {
                allEnemiesKilled = false;
                break;
            }
        }
    
        if (allEnemiesKilled)
        {
            Debug.Log("Victoria por completar objetivos de kills!");
            return true;
        }
    
        return false;
    }

    // Obtener progreso total (para mostrar en UI)
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
    
        Debug.Log("GameManager reseteado");
    }
}