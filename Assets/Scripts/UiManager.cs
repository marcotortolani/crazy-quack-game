using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class EnemyKillUI
{
    public string enemyName;
    public Image enemyIcon;
    public TextMeshProUGUI killCountText;
}

public class UiManager : MonoBehaviour
{
    [Header("Life Bar")]
    public Image lifeBar;
    public Player player;
    
    [Header("Countdown")]
    public TextMeshProUGUI countdownText;
    
    [Header("Kill Objectives")]
    public EnemyKillUI[] enemyKillDisplays;
    public TextMeshProUGUI totalKillsText;
    
    [Header("Death Alert")]
    public GameObject deathAlertPanel;
    
    [Header("Special Egg Power")]
    public GameObject specialEggPanel;
    public Image eggIcon;
    public Image progressBarFill;
    public Sprite fireEggSprite;
    public Sprite radioactiveEggSprite;
    
    [Header("Victory Panel")]
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryTitleText;
    public TextMeshProUGUI victorySubtitleText;
    public Button nextLevelButton;
    public Button victoryMenuButton;
    
    [Header("Defeat Panel")]
    public GameObject defeatPanel;
    public TextMeshProUGUI defeatTitleText;
    public TextMeshProUGUI defeatSubtitleText;
    public Button restartButton;
    public Button defeatMenuButton;
    
    [Header("Pause Panel")]
    public GameObject pausePanel;
    public TextMeshProUGUI pauseTitleText;
    public TextMeshProUGUI pauseSubtitleText;
    public Button resumeButton;
    public Button pauseMenuButton;

    private bool isPaused = false;
    
    [Header("Victory Messages")]
    public string[] victorySubtitles = new string[]
    {
        "THE CRAZIEST QUAKER",
        "UNSTOPPABLE SURVIVOR",
        "LEGENDARY WARRIOR",
        "CHAOS MASTER",
        "FLAWLESS VICTORY",
        "ULTIMATE CHAMPION",
        "DEATH DEFIER",
        "MASTER OF MAYHEM"
    };
    
    [Header("Defeat Messages")]
    public string[] defeatSubtitles = new string[]
    {
        "BETTER LUCK NEXT TIME",
        "TRY AGAIN, WARRIOR",
        "DON'T GIVE UP",
        "ALMOST THERE...",
        "GAME OVER",
        "SO CLOSE!",
        "YOU'LL GET IT NEXT TIME",
        "PRACTICE MAKES PERFECT"
    };
    
    [Header("Scene Names")]
    public string nextLevelSceneName = "Level2";
    public string menuSceneName = "MainMenu";
    
    private CanvasGroup deathAlertCanvasGroup;
    private bool isAlertActive = false;
    private Tweener currentAlertTween;
    private bool hasShownEndScreen = false;
    
    void Start()
    {
        DOTween.Init();
        
        // Buscar al player al inicio
        player = FindObjectOfType<Player>();
        
        // Ocultar cursor al inicio del juego
        Cursor.visible = false;
        
        // Limpiar/inicializar los textos de objetivos
        InitializeKillObjectivesDisplay();
        
        // Ocultar panel de huevo especial al inicio
        if (specialEggPanel != null)
        {
            specialEggPanel.SetActive(false);
        }
        
        // Configurar Death Alert
        if (deathAlertPanel != null)
        {
            deathAlertCanvasGroup = deathAlertPanel.GetComponent<CanvasGroup>();
            if (deathAlertCanvasGroup == null)
            {
                deathAlertCanvasGroup = deathAlertPanel.AddComponent<CanvasGroup>();
            }
            deathAlertCanvasGroup.alpha = 0f;
            deathAlertPanel.SetActive(false);
        }
        
        // Ocultar pantallas de victoria, derrota y pausa
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false); 
        
        // Configurar botones
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        if (victoryMenuButton != null)
            victoryMenuButton.onClick.AddListener(LoadMenu);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
        if (defeatMenuButton != null)
            defeatMenuButton.onClick.AddListener(LoadMenu);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (pauseMenuButton != null)
            pauseMenuButton.onClick.AddListener(LoadMenuFromPause);
    }

    void LateUpdate()
    {
        // Detectar input de pausa (solo si el juego no ha terminado)
        if (!hasShownEndScreen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)))
        {
            TogglePause();
        }
        
        // Si no tenemos referencia al player, intentar encontrarlo
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player == null)
            {
                return;
            }
            
        }
        
        // Verificar que el player todavía existe
        if (player.gameObject == null)
        {
            player = null;
            return;
        }
        
        
        // Verificar condiciones de fin de juego
        if (GameManager.Instance != null && !hasShownEndScreen)
        {
            if (GameManager.Instance.playerIsWin)
            {
                ShowVictoryScreen();
                hasShownEndScreen = true;
            }
            else if (GameManager.Instance.playerIsDead)
            {
                ShowDefeatScreen();
                hasShownEndScreen = true;
            }
        }
        
        // Actualizar HUD normal solo si el juego está activo
        if (!hasShownEndScreen && !isPaused)
        {
            float lifePercent;
            
            if (player != null)
            {
                lifePercent = (float)player.life / player.maxLife;
            }
            else
            {
                lifePercent = 0f;
            }
            
            UpdateLifeBar(lifePercent);
            UpdateDeathAlert(lifePercent);
            UpdateCountdown();
            UpdateKillObjectives();
            UpdateSpecialEggDisplay();
        }
    }
    
    // Actualizar display del huevo especial
    void UpdateSpecialEggDisplay()
    {
        if (player == null || specialEggPanel == null) return;
    
        EggType currentEgg = player.GetCurrentEggType();
        int shotsRemaining = player.GetSpecialEggShots();
        int maxShots = player.GetMaxSpecialEggShots(); // ← Usar el valor real
    
        if (currentEgg != EggType.Normal && shotsRemaining > 0)
        {
            if (!specialEggPanel.activeSelf)
            {
                specialEggPanel.SetActive(true);
                AnimateEggPanelIn();
            }
        
            if (eggIcon != null)
            {
                eggIcon.sprite = currentEgg == EggType.Fire ? fireEggSprite : radioactiveEggSprite;
            }
        
            if (progressBarFill != null)
            {
                // Calcular porcentaje dinámicamente
                float fillAmount = maxShots > 0 ? (float)shotsRemaining / maxShots : 0f;
                progressBarFill.fillAmount = fillAmount;
            
                // Color según tipo
                if (currentEgg == EggType.Fire)
                {
                    progressBarFill.color = new Color(1f, 0.5f, 0f); // Naranja
                }
                else
                {
                    progressBarFill.color = new Color(0f, 1f, 0f); // Verde
                }
            }
        }
        else
        {
            if (specialEggPanel.activeSelf)
            {
                AnimateEggPanelOut();
            }
        }
    }

    
    // Animación de entrada del panel
    void AnimateEggPanelIn()
    {
        if (specialEggPanel == null) return;
        
        CanvasGroup cg = specialEggPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = specialEggPanel.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        cg.DOFade(1f, 0.3f);
        
        specialEggPanel.transform.localScale = Vector3.zero;
        specialEggPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }
    
    // Animación de salida del panel
    void AnimateEggPanelOut()
    {
        if (specialEggPanel == null) return;
        
        CanvasGroup cg = specialEggPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOFade(0f, 0.3f).OnComplete(() => 
            {
                if (specialEggPanel != null)
                {
                    specialEggPanel.SetActive(false);
                }
            });
        }
        else
        {
            specialEggPanel.SetActive(false);
        }
        
        specialEggPanel.transform.DOScale(0.8f, 0.3f);
    }
    
    void InitializeKillObjectivesDisplay()
    {
        if (GameManager.Instance == null) return;
        Debug.Log("Inicializando el panel de objetivos");

        // Inicializar los displays de objetivos
        foreach (var display in enemyKillDisplays)
        {
            foreach (var objective in GameManager.Instance.killObjectives)
            {
                if (objective.enemyName == display.enemyName)
                {
                    // Establecer texto inicial
                    if (display.killCountText != null)
                    {
                        display.killCountText.text = $"0/{objective.killsRequired}";
                        display.killCountText.color = Color.white;
                    }
                
                    // Establecer icono
                    if (objective.enemyIcon != null && display.enemyIcon != null)
                    {
                        display.enemyIcon.sprite = objective.enemyIcon;
                    }
                    Debug.Log($"Objetivo: {objective.enemyName} " );
                
                    break;
                }
            }
        }

        // Inicializar el texto de total
        if (totalKillsText != null)
        {
            int required = GameManager.Instance.GetTotalKillsRequired();
            totalKillsText.text = $"Total: 0";
        }
    }

    string GetRandomVictorySubtitle()
    {
        if (victorySubtitles.Length == 0)
            return "THE CRAZIEST QUAKER";
        
        int randomIndex = Random.Range(0, victorySubtitles.Length);
        return victorySubtitles[randomIndex];
    }

    string GetRandomDefeatSubtitle()
    {
        if (defeatSubtitles.Length == 0)
            return "BETTER LUCK NEXT TIME";
        
        int randomIndex = Random.Range(0, defeatSubtitles.Length);
        return defeatSubtitles[randomIndex];
    }

    void ShowVictoryScreen()
    {
        if (victoryPanel == null) return;
        
        // Marcar nivel actual como completado
        if (LevelProgressManager.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
        
            if (currentSceneName == "Level_1")
            {
                LevelProgressManager.Instance.CompleteLevel("Level1");
            }
            else if (currentSceneName == "Level_2")
            {
                LevelProgressManager.Instance.CompleteLevel("Level2");
            }
        }
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Mostrar y desbloquear cursor
        Cursor.visible = true;
        
        // Ocultar alerta de muerte si está activa
        if (deathAlertPanel != null)
            deathAlertPanel.SetActive(false);
        
        // Establecer subtítulo aleatorio
        if (victorySubtitleText != null)
        {
            victorySubtitleText.text = GetRandomVictorySubtitle();
        }
        
        // Mostrar panel
        victoryPanel.SetActive(true);
        
        // Animación de entrada
        CanvasGroup cg = victoryPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = victoryPanel.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        cg.DOFade(1f, 0.5f).SetUpdate(true);
        
        // Animación del título
        if (victoryTitleText != null)
        {
            victoryTitleText.transform.localScale = Vector3.zero;
            victoryTitleText.transform.DOScale(1f, 0.6f).SetEase(Ease.OutElastic).SetDelay(0.2f).SetUpdate(true);
        }
        
        // Animación del subtítulo
        if (victorySubtitleText != null)
        {
            victorySubtitleText.transform.localScale = Vector3.zero;
            victorySubtitleText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.5f).SetUpdate(true);
        }
    }

    void ShowDefeatScreen()
    {
        if (defeatPanel == null) return;
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Mostrar cursor
        Cursor.visible = true;
        
        // Ocultar alerta de muerte si está activa
        if (deathAlertPanel != null)
            deathAlertPanel.SetActive(false);
        
        // Establecer subtítulo aleatorio
        if (defeatSubtitleText != null)
        {
            defeatSubtitleText.text = GetRandomDefeatSubtitle();
        }
        
        // Mostrar panel
        defeatPanel.SetActive(true);
        
        // Animación de entrada
        CanvasGroup cg = defeatPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = defeatPanel.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        cg.DOFade(1f, 0.5f).SetUpdate(true);
        
        // Animación del título
        if (defeatTitleText != null)
        {
            defeatTitleText.transform.localScale = Vector3.zero;
            defeatTitleText.transform.DOScale(1f, 0.6f).SetEase(Ease.OutElastic).SetDelay(0.2f).SetUpdate(true);
        }
        
        // Animación del subtítulo
        if (defeatSubtitleText != null)
        {
            defeatSubtitleText.transform.localScale = Vector3.zero;
            defeatSubtitleText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.5f).SetUpdate(true);
        }
    }

    // Métodos de botones
    void LoadNextLevel()
    {
        // Ocultar paneles antes de cambiar de escena
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
    
        Time.timeScale = 1f;
        
        // Limpiar/inicializar los textos de objetivos
        InitializeKillObjectivesDisplay();
        SceneManager.LoadScene(nextLevelSceneName);
    }

    public void RestartLevel()
    {
        
        // Resetear el GameManager ANTES de recargar la escena
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameState();
        }
        
        // Limpiar/inicializar los textos de objetivos
        InitializeKillObjectivesDisplay();
    
        // Ocultar paneles antes de reiniciar
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
    
        // Ocultar cursor nuevamente para el gameplay
        Cursor.visible = false;
    
        // Despausar el juego
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMenu()
    {
        // Resetear el GameManager ANTES de recargar la escena
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameState();
        }
        
        // Ocultar paneles antes de ir al menú
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        
        // Asegurar que el cursor esté visible en el menú
        Cursor.visible = true;
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    void PauseGame()
    {
        if (pausePanel == null) return;
        
        isPaused = true;
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Mostrar cursor
        Cursor.visible = true;
        
        // Mostrar panel
        pausePanel.SetActive(true);
        
        // Animación de entrada (con SetUpdate(true) para que funcione con timeScale = 0)
        CanvasGroup cg = pausePanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = pausePanel.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        cg.DOFade(1f, 0.3f).SetUpdate(true);
        
        // Animación del título (opcional)
        if (pauseTitleText != null)
        {
            pauseTitleText.transform.localScale = Vector3.zero;
            pauseTitleText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        }
        
        // Animación del subtítulo (opcional)
        if (pauseSubtitleText != null)
        {
            pauseSubtitleText.transform.localScale = Vector3.zero;
            pauseSubtitleText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.2f).SetUpdate(true);
        }
    }

    void ResumeGame()
    {
        if (pausePanel == null) return;
        
        isPaused = false;
        
        // Ocultar cursor
        Cursor.visible = false;
        
        // Ocultar panel con animación
        CanvasGroup cg = pausePanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => 
            {
                pausePanel.SetActive(false);
                // Despausar el juego después de la animación
                Time.timeScale = 1f;
            });
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void LoadMenuFromPause()
    {
        // Resetear el GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameState();
        }
        
        // Ocultar panel de pausa
        if (pausePanel != null) pausePanel.SetActive(false);
        
        // Mostrar cursor
        Cursor.visible = true;
        
        // Despausar antes de cambiar de escena
        Time.timeScale = 1f;
        
        SceneManager.LoadScene(menuSceneName);
    }
    
    void UpdateDeathAlert(float lifePercent)
    {
        if (deathAlertPanel == null || deathAlertCanvasGroup == null) return;

        if (lifePercent <= 0.4f) // vida menor a 40% se activa
        {
            if (!deathAlertPanel.activeSelf)
            {
                deathAlertPanel.SetActive(true);
            }

            float blinkSpeed;
            if (lifePercent <= 0.2f) // 20% o menos parpadea rapido
            {
                blinkSpeed = 0.25f;
            }
            else // entre 20% y 40% parpadea normal
            {
                blinkSpeed = 0.5f;
            }

            if (!isAlertActive || currentAlertTween == null)
            {
                StartDeathAlert(blinkSpeed);
            }
            else
            {
                currentAlertTween.timeScale = 1f / blinkSpeed;
            }
        }
        else
        {
            if (isAlertActive)
            {
                StopDeathAlert();
            }
        }
    }

    void StartDeathAlert(float blinkSpeed)
    {
        isAlertActive = true;
        
        if (currentAlertTween != null)
        {
            currentAlertTween.Kill();
        }

        currentAlertTween = deathAlertCanvasGroup.DOFade(0.6f, blinkSpeed)
            .From(0f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    void StopDeathAlert()
    {
        isAlertActive = false;
        
        if (currentAlertTween != null)
        {
            currentAlertTween.Kill();
            currentAlertTween = null;
        }

        deathAlertCanvasGroup.DOFade(0f, 0.3f).OnComplete(() => 
        {
            if (deathAlertPanel != null)
            {
                deathAlertPanel.SetActive(false);
            }
        });
    }

    void UpdateCountdown()
    {
        if (!GameManager.Instance) return;
        
        int timeRemaining = GameManager.Instance.secondsToSurvive - GameManager.Instance.secondsAlive;
        if (timeRemaining < 0) timeRemaining = 0;
        
        int minutes = timeRemaining / 60;
        int seconds = timeRemaining % 60;
        
        countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        
        if (timeRemaining <= 10)
        {
            countdownText.color = Color.red;
        }
        else if (timeRemaining <= 30)
        {
            countdownText.color = Color.yellow;
        }
        else
        {
            countdownText.color = Color.white;
        }
    }

    void UpdateKillObjectives()
    {
        if (!GameManager.Instance) return;

        foreach (var display in enemyKillDisplays)
        {
            foreach (var objective in GameManager.Instance.killObjectives)
            {
                if (objective.enemyName == display.enemyName)
                {
                    display.killCountText.text = $"{objective.currentKills}/{objective.killsRequired}";
                    
                    if (objective.enemyIcon != null && display.enemyIcon != null)
                    {
                        display.enemyIcon.sprite = objective.enemyIcon;
                    }
                    
                    if (objective.currentKills >= objective.killsRequired)
                    {
                        display.killCountText.color = Color.green;
                    }
                    else
                    {
                        display.killCountText.color = Color.white;
                    }
                    
                    break;
                }
            }
        }

        if (totalKillsText != null)
        {
            int current = GameManager.Instance.GetTotalCurrentKills();
            int required = GameManager.Instance.GetTotalKillsRequired();
            totalKillsText.text = $"Total: {current}";
        }
    }
    
    void UpdateLifeBar(float life)
    {
        if (lifeBar == null)
        {
            return;
        }

        DOTween.Kill(lifeBar.fillAmount);
        lifeBar.DOFillAmount(life, 0.25f).SetEase(Ease.Linear);
        
        if (life <= 0.25f && life > 0f)
        {
            //lifeBar.DOKill(false); 
            lifeBar.DOColor(Color.red, 0.5f).SetLoops(-1, LoopType.Yoyo).SetId(lifeBar);
        }
        else if (life <= 0f)
        {
            //lifeBar.DOKill(false); 
            lifeBar.color = Color.red;
        }
        else
        {
            //lifeBar.DOKill(false); 
            lifeBar.color = Color.white;
        }
    }
    
    private void OnDestroy()
    {
        if (currentAlertTween != null)
        {
            currentAlertTween.Kill();
        }
        
        // Asegurar que el cursor esté visible al destruir (por si acaso)
        Cursor.visible = true;
    }
}