using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class TrainingManager : MonoBehaviour
{
    [Header("Training Panels")]
    public GameObject movementPanel;
    public TextMeshProUGUI movementText;
    public Image movementImage;
    
    public GameObject aimPanel;
    public TextMeshProUGUI aimText;
    public Image aimImage;
    
    public GameObject completionPanel;
    public TextMeshProUGUI completionText;
    public Button continueButton;
    
    [Header("Player")]
    public Player player;
    
    [Header("Training Settings")]
    public int enemiesToKill = 3;
    public string nextSceneName = "Level_1";
    
    private enum TrainingPhase
    {
        Movement,
        Aiming,
        Completed
    }
    
    private TrainingPhase currentPhase = TrainingPhase.Movement;
    private int enemiesKilled = 0;
    private bool hasMovedWithWASD = false;

    void Start()
    {
        DOTween.Init();
        
        // Desactivar disparo del player al inicio
        if (player != null)
        {
            player.canShoot = false; // Desactivar completamente el script del player
        }
        
        // Configurar paneles
        ShowMovementPanel();
        
        if (aimPanel != null) aimPanel.SetActive(false);
        if (completionPanel != null) completionPanel.SetActive(false);
        
        // Configurar botón de continuar
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(LoadNextScene);
        }
        
        // Configurar textos
        if (movementText != null)
        {
            movementText.text = "USE WASD TO MOVE";
        }
        
        if (aimText != null)
        {
            aimText.text = "USE MOUSE TO AIM";
        }
        
        if (completionText != null)
        {
            completionText.text = "TRAINING COMPLETE\nTIME FOR ACTION!";
        }
    }

    void Update()
    {
        switch (currentPhase)
        {
            case TrainingPhase.Movement:
                CheckMovementInput();
                break;
                
            case TrainingPhase.Aiming:
                CheckEnemiesKilled();
                break;
                
            case TrainingPhase.Completed:
                // Esperar a que el jugador presione continuar
                break;
        }
    }

    void CheckMovementInput()
    {
        // Detectar si presionó WASD
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
            Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (!hasMovedWithWASD)
            {
                hasMovedWithWASD = true;
                StartCoroutine(TransitionToAimingPhase());
            }
        }
    }

    IEnumerator TransitionToAimingPhase()
    {
        // Ocultar panel de movimiento
        HideMovementPanel();
        
        yield return new WaitForSeconds(0.5f);
        
        // Cambiar a fase de apuntado
        currentPhase = TrainingPhase.Aiming;
        
        // Mostrar panel de apuntado
        ShowAimPanel();
        
        // Activar disparo del player
        if (player != null)
        {
            player.canShoot = true; // ← Activar disparo aquí
        }
    }

    void CheckEnemiesKilled()
    {
        // Contar enemigos vivos en la escena
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        int aliveEnemies = enemies.Length;
        
        // Si quedan menos enemigos que al inicio, se mataron algunos
        if (aliveEnemies <= (enemiesToKill - 1) && currentPhase == TrainingPhase.Aiming)
        {
            // Verificar si se mataron todos
            if (aliveEnemies == 0)
            {
                StartCoroutine(ShowCompletionPanel());
            }
        }
    }

    void ShowMovementPanel()
    {
        if (movementPanel == null) return;
        
        movementPanel.SetActive(true);
        
        // Animación de entrada
        CanvasGroup cg = movementPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = movementPanel.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        cg.DOFade(1f, 0.5f);
        
        // Animación del texto
        if (movementText != null)
        {
            movementText.transform.localScale = Vector3.zero;
            movementText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f);
        }
        
        // Animación de la imagen
        if (movementImage != null)
        {
            movementImage.transform.localScale = Vector3.zero;
            movementImage.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.3f);
        }
    }

    void HideMovementPanel()
    {
        if (movementPanel == null) return;
        
        CanvasGroup cg = movementPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOFade(0f, 0.3f).OnComplete(() => movementPanel.SetActive(false));
        }
        else
        {
            movementPanel.SetActive(false);
        }
    }

    void ShowAimPanel()
    {
        if (aimPanel == null) return;
        
        aimPanel.SetActive(true);
        
        // Animación de entrada
        CanvasGroup cg = aimPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = aimPanel.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        cg.DOFade(1f, 0.5f);
        
        // Animación del texto
        if (aimText != null)
        {
            aimText.transform.localScale = Vector3.zero;
            aimText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f);
        }
        
        // Animación de la imagen
        if (aimImage != null)
        {
            aimImage.transform.localScale = Vector3.zero;
            aimImage.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.3f);
        }
    }

    void HideAimPanel()
    {
        if (aimPanel == null) return;
        
        CanvasGroup cg = aimPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOFade(0f, 0.3f).OnComplete(() => aimPanel.SetActive(false));
        }
        else
        {
            aimPanel.SetActive(false);
        }
    }

    IEnumerator ShowCompletionPanel()
    {
        currentPhase = TrainingPhase.Completed;
        
        // Ocultar panel de apuntado
        HideAimPanel();
        
        yield return new WaitForSeconds(0.5f);
        
        if (completionPanel == null) yield break;
        
        completionPanel.SetActive(true);
        
        // Animación de entrada
        CanvasGroup cg = completionPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = completionPanel.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        cg.DOFade(1f, 0.5f);
        
        // Animación del texto
        if (completionText != null)
        {
            completionText.transform.localScale = Vector3.zero;
            completionText.transform.DOScale(1f, 0.6f).SetEase(Ease.OutElastic).SetDelay(0.2f);
        }
        
        // Animación del botón
        if (continueButton != null)
        {
            continueButton.transform.localScale = Vector3.zero;
            continueButton.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.5f);
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}