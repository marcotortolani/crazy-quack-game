using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MenuController : MonoBehaviour
{

    public CanvasGroup menuCanvasGroup;
    public CanvasGroup levelsCanvasGroup;
    public RectTransform menuScreenCanvasGroup;
    public RectTransform controlsScreenCanvasGroup;
    public RectTransform creditsScreenCanvasGroup;

    private void Start()
    {
        DOTween.Init();
    }
    

    public void GoToMenuPanel()
    {
        levelsCanvasGroup.transform.DOMoveX(-400f, 0.25f ).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            menuCanvasGroup.transform.DOMoveX(100f, 0.25f);
        });
    }

    public void GoToLevelsPanel()
    {
        menuCanvasGroup.transform.DOMoveX(-400f, 0.25f ).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            levelsCanvasGroup.transform.DOMoveX(100f, 0.25f);
        });
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
