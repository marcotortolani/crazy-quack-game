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
    private bool _hasShownLoseMessage = false;
    
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
        if (CheckAllObjectivesCompleted() && !playerIsWin)
        {
            PlayerWin();
        }
        
        if (playerIsDead && !_hasShownLoseMessage)
        {
            Debug.Log("El Player está muerto");
            Debug.Log("Perdiste");
            PrintStatus();
            _hasShownLoseMessage = true;
        }
    }

    private void UpdateTime()
    {
        if (!playerIsDead && !playerIsWin)
        {
            _timeCounter += Time.deltaTime; 
            secondsAlive = (int)_timeCounter; 
            
            // Verificar si alcanzó el tiempo objetivo Y completó todos los objetivos
            if (secondsAlive >= secondsToSurvive && CheckAllObjectivesCompleted())
            {
                PlayerWin();
            }
        }
    }

    // Método para registrar muerte de un enemigo específico
    public void RegisterEnemyKill(string enemyType)
    {
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

    // Verificar si se completaron todos los objetivos de kills
    private bool CheckAllObjectivesCompleted()
    {
        // Primero verificar si sobrevivió el tiempo mínimo
        if (secondsAlive < secondsToSurvive)
            return false;

        // Luego verificar si completó todos los objetivos de kills
        foreach (var objective in killObjectives)
        {
            if (objective.currentKills < objective.killsRequired)
                return false;
        }
        
        return true;
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
            Debug.Log("¡Ganaste!");
            Debug.Log("Completaste todos los objetivos y sobreviviste el tiempo necesario");
            PrintStatus();
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
        _hasShownLoseMessage = false;
    
        // Resetear contadores de kills
        foreach (var objective in killObjectives)
        {
            objective.currentKills = 0;
        }
    
        Debug.Log("GameManager reseteado");
    }


    private void PrintStatus()
    {
        Debug.Log(">> Tiempo sobrevivido: " + secondsAlive + " segundos.");
        Debug.Log(">> Objetivos completados:");
        foreach (var objective in killObjectives)
        {
            Debug.Log($"   - {objective.enemyName}: {objective.currentKills}/{objective.killsRequired}");
        }
    }
}



// using UnityEngine;
//
// public class GameManager : MonoBehaviour
// {
//     
//     public static GameManager Instance; // Referencia global
//     
//     public int enemiesKilled = 0;
//     public int enemiesToKill = 80;
//     public int secondsAlive = 0;
//     public int secondsToSurvive = 120;
//     public bool playerIsDead  = false;
//     public bool playerIsWin = false;
//
//     private float _timeCounter = 0f;
//     private bool _hasShownWinMessage = false;
//     private bool _hasShownLoseMessage = false;
//     
//     private void Awake()
//     {
//         // Singleton: solo puede haber uno
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }
//     
//     void Update()
//     {
//         UpdateTime();
//
//         if (enemiesKilled >= enemiesToKill  && !playerIsWin)
//         {
//             PlayerWin();
//         }
//         
//         if (playerIsDead && !_hasShownLoseMessage)
//         {
//             Debug.Log("El Player está muerto");
//             Debug.Log("Perdiste");
//             PrintStatus();
//             _hasShownLoseMessage = true;
//         }
//         
//     }
//
//     private void UpdateTime()
//     {
//         if (!playerIsDead && !playerIsWin)
//         {
//             _timeCounter += Time.deltaTime; 
//             secondsAlive = (int)_timeCounter; 
//             
//             // Verificar si alcanzó el tiempo objetivo
//             if (secondsAlive >= secondsToSurvive)
//             {
//                 PlayerWin();
//             }
//         }
//     }
//
//     private void PlayerWin()
//     {
//         if (!_hasShownWinMessage)
//         {
//             playerIsWin = true;
//             Debug.Log("El Player sobrevivió");
//             Debug.Log("Ganaste");
//             PrintStatus();
//             _hasShownWinMessage = true;
//         }
//     }
//
//     private void PrintStatus()
//     {
//         Debug.Log(">> Enemigos eliminados " + enemiesKilled);
//         Debug.Log(">> Te mantuviste de pie " + secondsAlive + " segundos.");
//     }
// }
