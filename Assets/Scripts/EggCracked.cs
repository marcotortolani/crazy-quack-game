using UnityEngine;

public class EggCracked : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallSpeed = 2f;
    public float fallDistance = 0.5f; // Qué tan abajo cae antes de detenerse
    public float fadeDuration = 1f;   // Duración del fade out al final
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float fallTimer = 0f;
    private float fallTime = 0.5f;
    private bool hasFallen = false;
    private SpriteRenderer spriteRenderer;
    private float lifetime;
    private float fadeStartTime;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configurar posiciones
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.down * fallDistance;
        
        // Calcular tiempo de caída basado en velocidad
        fallTime = fallDistance / fallSpeed;
        
        // Obtener el lifetime del objeto (establecido por el Bullet)
        lifetime = GetComponent<EggCracked>() ? 4f : 4f; // Default 4 segundos
        fadeStartTime = lifetime - fadeDuration;
    }

    private void Update()
    {
        // Hacer que caiga al suelo
        if (!hasFallen)
        {
            fallTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(fallTimer / fallTime);
            
            // Movimiento con easing (suavizado)
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            transform.position = Vector3.Lerp(startPosition, targetPosition, easedProgress);
            
            if (progress >= 1f)
            {
                hasFallen = true;
            }
        }
        
        // Fade out gradual antes de destruirse
        if (spriteRenderer != null)
        {
            float age = Time.time - (Time.time - lifetime + GetLifetimeElapsed());
            
            if (age >= fadeStartTime)
            {
                float fadeProgress = (age - fadeStartTime) / fadeDuration;
                Color color = spriteRenderer.color;
                color.a = 1f - fadeProgress;
                spriteRenderer.color = color;
            }
        }
    }
    
    private float GetLifetimeElapsed()
    {
        // Esto es aproximado, puedes mejorarlo si necesitas precisión
        return fallTimer;
    }
}