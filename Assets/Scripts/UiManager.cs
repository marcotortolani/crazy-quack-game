using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UiManager : MonoBehaviour
{
    public Image lifeBar;
    public Player player;
    public GameManager gameManager;
    public TextMeshProUGUI countdownText;
    private int _lastTimeRemaining = -1;
    
    void Start()
    {
        DOTween.Init();
    }

    void Update()
    {
        if (!player) return;
        float lifePercent = (float)player.life / player.maxLife;
        UpdateLifeBar(lifePercent);
        UpdateCountdown();
    }

    void UpdateCountdown()
    {
        if (!GameManager.Instance) return;
    
        int timeRemaining = GameManager.Instance.secondsToSurvive - GameManager.Instance.secondsAlive;
        if (timeRemaining < 0) timeRemaining = 0;
    
        int minutes = timeRemaining / 60;
        int seconds = timeRemaining % 60;
    
        countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    
        // Animación de escala cada vez que cambia el segundo
        if (timeRemaining != _lastTimeRemaining)
        {
            _lastTimeRemaining = timeRemaining;
        
            // Pequeño "pop" cada segundo
            countdownText.transform.DOScale(1.2f, 0.1f).OnComplete(() => 
                countdownText.transform.DOScale(1f, 0.1f)
            );
        
            // Cambiar color según tiempo restante
            if (timeRemaining <= 10)
            {
                countdownText.DOColor(Color.red, 0.2f);
            }
            else if (timeRemaining <= 30)
            {
                countdownText.DOColor(Color.yellow, 0.2f);
            }
        }
    }
    // void UpdateCountdown()
    // {
    //     if (!GameManager.Instance) return;
    //     
    //     // Calcular tiempo restante
    //     int timeRemaining = GameManager.Instance.secondsToSurvive - GameManager.Instance.secondsAlive;
    //     
    //     // Asegurar que no sea negativo
    //     if (timeRemaining < 0) timeRemaining = 0;
    //     
    //     // Convertir a minutos y segundos
    //     int minutes = timeRemaining / 60;
    //     int seconds = timeRemaining % 60;
    //     
    //     // Formatear con ceros a la izquierda (ejemplo: 01:05)
    //     countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    //     
    //     // Opcional: cambiar color cuando queda poco tiempo
    //     if (timeRemaining <= 10)
    //     {
    //         countdownText.color = Color.red;
    //     }
    //     else if (timeRemaining <= 30)
    //     {
    //         countdownText.color = Color.yellow;
    //     }
    //     else
    //     {
    //         countdownText.color = Color.black;
    //     }
    // }
    
    void UpdateLifeBar(float life)
    {
        if (life <= 0.25f)
        {
            lifeBar.DOColor(Color.black, 1f).OnComplete(()=> lifeBar.DOColor(Color.white, 1f));
        };
        lifeBar.DOFillAmount(life, 0.5f ).SetEase(Ease.Linear);
    }
}
