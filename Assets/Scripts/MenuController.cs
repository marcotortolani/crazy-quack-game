using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MenuController : MonoBehaviour
{

    public CanvasGroup menuCanvasGroup;
    public CanvasGroup levelsCanvasGroup;
    public CanvasGroup menuScreenCanvasGroup;
    public CanvasGroup controlsScreenCanvasGroup;
    public CanvasGroup creditsCanvasGroup;

    private void Start()
    {
        DOTween.Init();
    }
    

    public void GoToMenuPanel()
    {
        levelsCanvasGroup.transform.DOMoveX(-400f, 0.25f ).OnComplete(() =>
        {
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.transform.DOMoveX(100f, 0.25f);
            menuCanvasGroup.DOFade(1, 0.3f);
            menuCanvasGroup.interactable = true;
        });
    }

    public void GoToLevelsPanel()
    {
        menuCanvasGroup.transform.DOMoveX(-400f, 0.25f ).OnComplete(() =>
        {
            menuCanvasGroup.interactable = false;
            levelsCanvasGroup.transform.DOMoveX(100f, 0.25f);
            levelsCanvasGroup.DOFade(1, 0.3f);
            levelsCanvasGroup.interactable = true;
        });
    }
    
    // Transición horizontal (izquierda/derecha)
    private void SlideHorizontal(CanvasGroup fromScreen, CanvasGroup toScreen, float duration = 0.5f)
    {
        fromScreen.interactable = false;
        toScreen.interactable = false;
    
        // Determinar dirección basándose en posiciones actuales
        float fromTargetX = fromScreen.transform.localPosition.x < toScreen.transform.localPosition.x ? -2000f : 2000f;
    
        fromScreen.transform.DOLocalMoveX(fromTargetX, duration);
        toScreen.transform.DOLocalMoveX(0f, duration).OnComplete(() =>
        {
            toScreen.interactable = true;
        });
    }

    // Transición vertical (arriba/abajo)
    private void SlideVertical(CanvasGroup fromScreen, CanvasGroup toScreen, float duration = 0.5f)
    {
        fromScreen.interactable = false;
        toScreen.interactable = false;
    
        // Determinar dirección basado en posiciones actuales
        float fromTargetY = fromScreen.transform.localPosition.y < toScreen.transform.localPosition.y ? -1200f : 1200f;
    
        fromScreen.transform.DOLocalMoveY(fromTargetY, duration);
        toScreen.transform.DOLocalMoveY(0f, duration).OnComplete(() =>
        {
            toScreen.interactable = true;
        });
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
        SlideVertical(menuScreenCanvasGroup, creditsCanvasGroup);
    }

    public void GoToMenuFromCredits()
    {
        SlideVertical(creditsCanvasGroup, menuScreenCanvasGroup);
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
