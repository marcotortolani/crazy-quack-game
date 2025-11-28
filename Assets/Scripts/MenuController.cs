using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MenuController : MonoBehaviour
{

    public CanvasGroup menuCanvasGroup;
    public CanvasGroup levelsCanvasGroup;
    public RectTransform menuScreenCanvasGroup;
    public RectTransform controlsScreenCanvasGroup;
    public RectTransform creditsScreenCanvasGroup;
    
    [Header("Level Buttons")]
    public Button trainingButton;
    public Button level1Button;
    public Button level2Button;
    
    [Header("Lock Icons (Optional)")] 
    public GameObject trainingLockIcon;
    public GameObject level1LockIcon;
    public GameObject level2LockIcon;
    
    [Header("Scene Names")]
    public string trainingSceneName = "Training_Level";
    public string level1SceneName = "Level_1";
    public string level2SceneName = "Level_2";

    private void Start()
    {
        DOTween.Init();
        Cursor.visible = true;
        
        // Limpiar listeners existentes ← IMPORTANTE
        if (trainingButton != null)
        {
            trainingButton.onClick.RemoveAllListeners();
            trainingButton.onClick.AddListener(() => LoadLevel(trainingSceneName));
        }
        
        if (level1Button != null)
        {
            level1Button.onClick.RemoveAllListeners();
            level1Button.onClick.AddListener(() => LoadLevel(level1SceneName));
        }
        
        if (level2Button != null)
        {
            level2Button.onClick.RemoveAllListeners();
            level2Button.onClick.AddListener(() => LoadLevel(level2SceneName));
        }
        
        UpdateLevelButtons();
    }
    
    // Método para cargar nivel
    void LoadLevel(string sceneName)
    {
        Debug.Log($"Cargando escena: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
    
    // Actualizar estado de los botones de nivel
    void UpdateLevelButtons()
    {
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogWarning("LevelProgressManager no encontrado!");
            return;
        }
        
        // Training - siempre desbloqueado
        SetButtonState(trainingButton, true, trainingLockIcon);
        
        // Level 1 - desbloqueado si completó Training
        bool level1Unlocked = LevelProgressManager.Instance.IsLevelUnlocked("Level1");
        SetButtonState(level1Button, level1Unlocked, level1LockIcon);
        
        // Level 2 - desbloqueado si completó Level 1
        bool level2Unlocked = LevelProgressManager.Instance.IsLevelUnlocked("Level2");
        SetButtonState(level2Button, level2Unlocked, level2LockIcon);
    }
    
    //  Configurar estado visual del botón
    void SetButtonState(Button button, bool unlocked, GameObject lockIcon)
    {
        if (button == null) return;
        
        button.interactable = unlocked;
        
        // Cambiar opacidad del botón
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = unlocked ? 1f : 0.5f;
            buttonImage.color = color;
        }
        
        // Mostrar/ocultar ícono de candado
        if (lockIcon != null)
        {
            lockIcon.SetActive(!unlocked);
        }
    }
    
    // Método público para resetear progreso (testing)
    public void ResetAllProgress()
    {
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ResetProgress();
            UpdateLevelButtons();
        }
    }

    public void GoToMenuPanel()
    {
        levelsCanvasGroup.transform.DOMoveX(-400f, 0.25f ).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            levelsCanvasGroup.DOFade(0f, 0.25f);
            levelsCanvasGroup.interactable = false;
            menuCanvasGroup.transform.DOMoveX(100f, 0.25f);
            menuCanvasGroup.DOFade(1f, 0.25f);
        });
    }

    public void GoToLevelsPanel()
    {
        menuCanvasGroup.transform.DOMoveX(-400f, 0.25f ).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            menuCanvasGroup.DOFade(0f, 0.25f);
            levelsCanvasGroup.transform.DOMoveX(100f, 0.25f);
            levelsCanvasGroup.interactable = true;
            levelsCanvasGroup.DOFade(1f, 0.25f);
        });
        
        // Actualizar estado de botones al entrar al panel de niveles
        UpdateLevelButtons();
    }
    
    // Transición horizontal (izquierda/derecha)
    private void SlideHorizontal(RectTransform fromScreen, RectTransform toScreen, float duration = 0.5f)
    {
        // Determinar dirección basándose en posiciones actuales
        float fromTargetX = fromScreen.transform.localPosition.x < toScreen.transform.localPosition.x ? -2000f : 2000f;
        fromScreen.DOAnchorPosX(fromTargetX, duration);
        toScreen.DOAnchorPosX(0f, duration);
    }

    // Transición vertical (arriba/abajo)
    private void SlideVertical(RectTransform fromScreen, RectTransform toScreen, float duration = 0.5f)
    {
        // Determinar dirección basado en posiciones actuales
        float fromTargetY = fromScreen.transform.localPosition.y < toScreen.transform.localPosition.y ? -1200f : 1200f;
        fromScreen.DOAnchorPosY(fromTargetY, duration);
        toScreen.DOAnchorPosY(0f, duration);
    }

    // Slides entre pantallas del menú
    public void GoToControlsScreen()
    {
        SlideHorizontal(menuScreenCanvasGroup, controlsScreenCanvasGroup);
    }

    public void GoToMenuScreen()
    {
        SlideHorizontal(controlsScreenCanvasGroup, menuScreenCanvasGroup);
    }

    public void GoToCreditsScreen()
    {
        SlideVertical(menuScreenCanvasGroup, creditsScreenCanvasGroup);
    }

    public void GoToMenuFromCredits()
    {
        SlideVertical(creditsScreenCanvasGroup, menuScreenCanvasGroup);
    }


    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
