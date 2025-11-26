using UnityEngine;

public class GameManagerOld : MonoBehaviour
{
    
    public static GameManagerOld Instance; // Referencia global
    
    public int enemiesKilled = 0;
    public int enemiesToKill = 80;
    public int secondsAlive = 0;
    public int secondsToSurvive = 120;
    public bool playerIsDead  = false;
    public bool playerIsWin = false;

    private float timeCounter = 0f;
    
    private bool hasShownWinMessage = false;
    private bool hasShownLoseMessage = false;
    
    private void Awake()
    {
        // Singleton: solo puede haber uno
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

        if (enemiesKilled >= enemiesToKill  && !playerIsWin)
        {
            PlayerWin();
        }
        
        if (playerIsDead && !hasShownLoseMessage)
        {
            Debug.Log("El Player está muerto");
            Debug.Log("Perdiste");
            PrintStatus();
            hasShownLoseMessage = true;
        }
        
    }

    private void UpdateTime()
    {
        if (!playerIsDead && !playerIsWin)
        {
            timeCounter += Time.deltaTime; 
            secondsAlive = (int)timeCounter; 
            
            // Verificar si alcanzó el tiempo objetivo
            if (secondsAlive >= secondsToSurvive)
            {
                PlayerWin();
            }
        }
    }

    private void PlayerWin()
    {
        if (!hasShownWinMessage)
        {
            playerIsWin = true;
            Debug.Log("El Player sobrevivió");
            Debug.Log("Ganaste");
            PrintStatus();
            hasShownWinMessage = true;
        }
    }

    private void PrintStatus()
    {
        Debug.Log(">> Enemigos eliminados " + enemiesKilled);
        Debug.Log(">> Te mantuviste de pie " + secondsAlive + " segundos.");
    }
}
