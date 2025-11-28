using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance;
    
    // Claves para PlayerPrefs
    private const string TRAINING_COMPLETED = "TrainingCompleted";
    private const string LEVEL1_COMPLETED = "Level1Completed";
    private const string LEVEL2_COMPLETED = "Level2Completed";
    
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
    
    // Verificar si un nivel está completado
    public bool IsLevelCompleted(string levelName)
    {
        switch (levelName)
        {
            case "Training":
                return PlayerPrefs.GetInt(TRAINING_COMPLETED, 0) == 1;
            case "Level1":
                return PlayerPrefs.GetInt(LEVEL1_COMPLETED, 0) == 1;
            case "Level2":
                return PlayerPrefs.GetInt(LEVEL2_COMPLETED, 0) == 1;
            default:
                return false;
        }
    }
    
    // Marcar un nivel como completado
    public void CompleteLevel(string levelName)
    {
        switch (levelName)
        {
            case "Training":
                PlayerPrefs.SetInt(TRAINING_COMPLETED, 1);
                Debug.Log("Training completado y guardado!");
                break;
            case "Level1":
                PlayerPrefs.SetInt(LEVEL1_COMPLETED, 1);
                Debug.Log("Level 1 completado y guardado!");
                break;
            case "Level2":
                PlayerPrefs.SetInt(LEVEL2_COMPLETED, 1);
                Debug.Log("Level 2 completado y guardado!");
                break;
        }
        
        PlayerPrefs.Save(); // Guardar inmediatamente
    }
    
    // Verificar si un nivel está desbloqueado (puede jugarse)
    public bool IsLevelUnlocked(string levelName)
    {
        switch (levelName)
        {
            case "Training":
                return true; // Training siempre está desbloqueado
            case "Level1":
                return IsLevelCompleted("Training"); // Se desbloquea al completar Training
            case "Level2":
                return IsLevelCompleted("Level1"); // Se desbloquea al completar Level 1
            default:
                return false;
        }
    }
    
    // Resetear todo el progreso (útil para testing)
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(TRAINING_COMPLETED);
        PlayerPrefs.DeleteKey(LEVEL1_COMPLETED);
        PlayerPrefs.DeleteKey(LEVEL2_COMPLETED);
        PlayerPrefs.Save();
        Debug.Log("Progreso reseteado!");
    }
}