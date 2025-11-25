using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public GameObject deathAlertPanel; // Panel rojo semi-transparente
    
    private CanvasGroup deathAlertCanvasGroup;
    private bool isAlertActive = false;
    private Tweener currentAlertTween;
    private float lastLifePercent = 1f;
    
    void Start()
    {
        DOTween.Init();
        
        // Obtener o agregar CanvasGroup al panel de alerta
        if (deathAlertPanel != null)
        {
            deathAlertCanvasGroup = deathAlertPanel.GetComponent<CanvasGroup>();
            if (deathAlertCanvasGroup == null)
            {
                deathAlertCanvasGroup = deathAlertPanel.AddComponent<CanvasGroup>();
            }
            
            // Asegurarse de que empiece invisible
            deathAlertCanvasGroup.alpha = 0f;
            deathAlertPanel.SetActive(false);
        }
    }

    void Update()
    {
        float lifePercent;
        if (player != null)
        {
            lifePercent = (float)player.life / player.maxLife;
            lastLifePercent = lifePercent;
        }
        else
        {
            lifePercent = 0f;
        }
        
        UpdateLifeBar(lifePercent);
        UpdateDeathAlert(lifePercent);
        
        UpdateCountdown();
        UpdateKillObjectives();
    }

    void UpdateDeathAlert(float lifePercent)
    {
        if (deathAlertPanel == null || deathAlertCanvasGroup == null) return;

        if (lifePercent <= 0.4f) // 40% o menos
        {
            if (!deathAlertPanel.activeSelf)
            {
                deathAlertPanel.SetActive(true);
            }

            // Determinar velocidad de parpadeo según la vida
            float blinkSpeed;
            if (lifePercent <= 0.2f) // 20% o menos - parpadeo rápido
            {
                blinkSpeed = 0.25f;
            }
            else // Entre 20% y 40% - parpadeo normal
            {
                blinkSpeed = 0.5f;
            }

            // Iniciar o actualizar el parpadeo
            if (!isAlertActive || currentAlertTween == null)
            {
                StartDeathAlert(blinkSpeed);
            }
            else
            {
                // Actualizar velocidad si cambió
                currentAlertTween.timeScale = 1f / blinkSpeed;
            }
        }
        else // Más del 40% de vida
        {
            // Detener alerta si estaba activa
            if (isAlertActive)
            {
                StopDeathAlert();
            }
        }
    }

    void StartDeathAlert(float blinkSpeed)
    {
        isAlertActive = true;
        
        // Matar cualquier animación previa
        if (currentAlertTween != null)
        {
            currentAlertTween.Kill();
        }

        // Crear animación de parpadeo infinita
        currentAlertTween = deathAlertCanvasGroup.DOFade(0.6f, blinkSpeed)
            .From(0f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    void StopDeathAlert()
    {
        isAlertActive = false;
        
        // Matar la animación
        if (currentAlertTween != null)
        {
            currentAlertTween.Kill();
            currentAlertTween = null;
        }

        // Fade out suave y desactivar
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
            totalKillsText.text = $"Total: {current}/{required}";
        }
    }
    
    void UpdateLifeBar(float life)
    {
        if (life <= 0.25f)
        {
            lifeBar.DOColor(Color.red, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            // Detener el parpadeo de la barra si la vida sube
            lifeBar.DOKill();
            lifeBar.color = Color.white;
        }
        
        lifeBar.DOFillAmount(life, 0.25f).SetEase(Ease.Linear);
    }

    private void OnDestroy()
    {
        // Limpiar tweens al destruir el objeto
        if (currentAlertTween != null)
        {
            currentAlertTween.Kill();
        }
    }
}


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using DG.Tweening;
//
// [System.Serializable]
// public class EnemyKillUI
// {
//     public string enemyName;
//     public Image enemyIcon;
//     public TextMeshProUGUI killCountText;
// }
//
// public class UiManager : MonoBehaviour
// {
//     [Header("Life Bar")]
//     public Image lifeBar;
//     public Player player;
//     
//     [Header("Countdown")]
//     public TextMeshProUGUI countdownText;
//     
//     [Header("Kill Objectives")]
//     public EnemyKillUI[] enemyKillDisplays; // Array de UI para cada enemigo
//     public TextMeshProUGUI totalKillsText; // Opcional: contador total
//     
//     void Start()
//     {
//         DOTween.Init();
//     }
//
//     void Update()
//     {
//         if (!player) return;
//         
//         float lifePercent = (float)player.life / player.maxLife;
//         UpdateLifeBar(lifePercent);
//         
//         UpdateCountdown();
//         UpdateKillObjectives();
//     }
//
//     void UpdateCountdown()
//     {
//         if (!GameManager.Instance) return;
//         
//         int timeRemaining = GameManager.Instance.secondsToSurvive - GameManager.Instance.secondsAlive;
//         if (timeRemaining < 0) timeRemaining = 0;
//         
//         int minutes = timeRemaining / 60;
//         int seconds = timeRemaining % 60;
//         
//         countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
//         
//         if (timeRemaining <= 10)
//         {
//             countdownText.color = Color.red;
//         }
//         else if (timeRemaining <= 30)
//         {
//             countdownText.color = Color.yellow;
//         }
//         else
//         {
//             countdownText.color = Color.white;
//         }
//     }
//
//     void UpdateKillObjectives()
//     {
//         if (!GameManager.Instance) return;
//
//         // Actualizar cada display de enemigo
//         foreach (var display in enemyKillDisplays)
//         {
//             // Buscar el objetivo correspondiente en el GameManager
//             foreach (var objective in GameManager.Instance.killObjectives)
//             {
//                 if (objective.enemyName == display.enemyName)
//                 {
//                     // Actualizar el texto: "5/10" por ejemplo
//                     display.killCountText.text = $"{objective.currentKills}/{objective.killsRequired}";
//                     
//                     // Actualizar el icono si está configurado en el GameManager
//                     if (objective.enemyIcon != null && display.enemyIcon != null)
//                     {
//                         display.enemyIcon.sprite = objective.enemyIcon;
//                     }
//                     
//                     // Opcional: cambiar color si está completado
//                     if (objective.currentKills >= objective.killsRequired)
//                     {
//                         display.killCountText.color = Color.green;
//                     }
//                     else
//                     {
//                         display.killCountText.color = Color.white;
//                     }
//                     
//                     break;
//                 }
//             }
//         }
//
//         // Actualizar contador total (opcional)
//         if (totalKillsText != null)
//         {
//             int current = GameManager.Instance.GetTotalCurrentKills();
//             int required = GameManager.Instance.GetTotalKillsRequired();
//             totalKillsText.text = $"Total: {current}/{required}";
//         }
//     }
//     
//     void UpdateLifeBar(float life)
//     {
//         if (life <= 0.25f)
//         {
//             lifeBar.DOColor(Color.red, 0.5f).OnComplete(() => lifeBar.DOColor(Color.black, 0.5f));
//         }
//         lifeBar.DOFillAmount(life, 0.25f).SetEase(Ease.Linear);
//     }
// }

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using DG.Tweening;
// using TMPro;
//
// public class UiManager : MonoBehaviour
// {
//     public Image lifeBar;
//     public Player player;
//     public GameManager gameManager;
//     public TextMeshProUGUI countdownText;
//     private int _lastTimeRemaining = -1;
//     
//     void Start()
//     {
//         DOTween.Init();
//     }
//
//     void Update()
//     {
//         if (!player) return;
//         float lifePercent = (float)player.life / player.maxLife;
//         UpdateLifeBar(lifePercent);
//         UpdateCountdown();
//     }
//
//     void UpdateCountdown()
//     {
//         if (!GameManager.Instance) return;
//     
//         int timeRemaining = GameManager.Instance.secondsToSurvive - GameManager.Instance.secondsAlive;
//         if (timeRemaining < 0) timeRemaining = 0;
//     
//         int minutes = timeRemaining / 60;
//         int seconds = timeRemaining % 60;
//     
//         countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
//     
//         // Animación de escala cada vez que cambia el segundo
//         if (timeRemaining != _lastTimeRemaining)
//         {
//             _lastTimeRemaining = timeRemaining;
//         
//             // Pequeño "pop" cada segundo
//             countdownText.transform.DOScale(1.2f, 0.1f).OnComplete(() => 
//                 countdownText.transform.DOScale(1f, 0.1f)
//             );
//         
//             // Cambiar color según tiempo restante
//             if (timeRemaining <= 10)
//             {
//                 countdownText.DOColor(Color.red, 0.2f);
//             }
//             else if (timeRemaining <= 30)
//             {
//                 countdownText.DOColor(Color.yellow, 0.2f);
//             }
//         }
//     }
//     // void UpdateCountdown()
//     // {
//     //     if (!GameManager.Instance) return;
//     //     
//     //     // Calcular tiempo restante
//     //     int timeRemaining = GameManager.Instance.secondsToSurvive - GameManager.Instance.secondsAlive;
//     //     
//     //     // Asegurar que no sea negativo
//     //     if (timeRemaining < 0) timeRemaining = 0;
//     //     
//     //     // Convertir a minutos y segundos
//     //     int minutes = timeRemaining / 60;
//     //     int seconds = timeRemaining % 60;
//     //     
//     //     // Formatear con ceros a la izquierda (ejemplo: 01:05)
//     //     countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
//     //     
//     //     // Opcional: cambiar color cuando queda poco tiempo
//     //     if (timeRemaining <= 10)
//     //     {
//     //         countdownText.color = Color.red;
//     //     }
//     //     else if (timeRemaining <= 30)
//     //     {
//     //         countdownText.color = Color.yellow;
//     //     }
//     //     else
//     //     {
//     //         countdownText.color = Color.black;
//     //     }
//     // }
//     
//     void UpdateLifeBar(float life)
//     {
//         if (life <= 0.25f)
//         {
//             lifeBar.DOColor(Color.black, 1f).OnComplete(()=> lifeBar.DOColor(Color.white, 1f));
//         };
//         lifeBar.DOFillAmount(life, 0.5f ).SetEase(Ease.Linear);
//     }
// }
