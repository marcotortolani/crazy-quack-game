using System.Linq.Expressions;
using UnityEngine;

public class Player : MonoBehaviour
{
    private EggType currentEggType = EggType.Normal;
    private int specialEggShots = 0;
    private int maxSpecialEggShots = 0;
    
    public float speed;
    public Vector3 movement;
    public Animator myAnimator;
    public int life;
    public int maxLife = 10;
    
    [Header("Shooting")]
    public GameObject gun;
    public GameObject bullet;
    public GameObject fireEggBullet;
    public GameObject radioactiveEggBullet;
    public Transform bulletSpawnOrigin;
    public int bulletsPerSecond = 4;
    public int maxBulletsPerSecond = 20;
    
    public bool canShoot = true;
    
    
    // VARIABLES PARA SPRAY SHOOT (CONO)
    [Header("Multi-Shot Setup")]
    // Umbral para 2 balas: Dispara 1 bala/vez hasta este BPS
    public int multiShotTwoThreshold = 5; 
    // Umbral para 3 balas: Dispara 2 balas/vez hasta este BPS
    public int multiShotThreeThreshold = 10; 
    
    // Ángulo de dispersión para 2 balas (ej: 20 grados)
    public float twoShotSpreadAngle = 20f; 
    // Ángulo de dispersión para 3 balas (ej: 45 grados)
    public float threeShotSpreadAngle = 45f;
    
    [Header("Damage Effect")]
    public float damageBlinkDuration = 1f;
    public float blinkInterval = 0.1f;
    public bool isInvulnerable = false;
    
    // variables privadas para el cálculo de la frecuencia de spawneo
    private float _nextFireTime = 0f;
    private float _bulletFireRate;
    private int _lastUpgradeSecond = 0;
    
    // Variables para el speed boost
    private float _speedBoostTimer = 0f;
    private float _originalSpeed;
    private bool _isSpeedBoosted = false;
    
    // Variables para el efecto de daño
    private SpriteRenderer _spriteRenderer;
    private bool _isBlinking = false;
    private float _blinkTimer = 0f;
    private float _blinkIntervalTimer = 0f;
    private bool _spriteVisible = true;
    
    // Variable para saber si está disparando
    private bool _isShooting = false;
    // Tiempo mínimo que debe durar la animación de disparo (ajusta este valor)
    private const float SHOOT_ANIM_DURATION = 0.15f; // 150 milisegundos
    private float _shootAnimTimer = 0f;

    private void Start()
    {
        life = maxLife;
        _originalSpeed = speed;
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (!canShoot)
        {
            canShoot = true;
        }
        // Ignorar colisiones entre el player y sus propias balas
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("PlayerBullet"));
    }

    private void Update()
    {
        // Manejar speed boost timer
        UpdateSpeedBoost();
        UpdateDamageBlink();
        Movement();
        UpdateFireRate();

        if(!GameManager.Instance) return;
        
        // Determinar si está disparando 
        // 1. Manejar el temporizador de animación
        if (_shootAnimTimer > 0)
        {
            _shootAnimTimer -= Time.deltaTime;
            _isShooting = true;
        }
        else
        {
            _isShooting = false;
        }
        
        // 2. Ejecutar Disparo (y reactivar el temporizador)
        if (canShoot && !GameManager.Instance.playerIsDead && !GameManager.Instance.playerIsWin)
        {
            if (Time.time >= _nextFireTime)
            {
                _nextFireTime = Time.time + _bulletFireRate;
                ShootDirection();
        
                // ¡Se disparó! Activamos el temporizador y el flag
                _shootAnimTimer = SHOOT_ANIM_DURATION; 
                _isShooting = true; 
            }
        }
        
        // 3. Actualizar parámetro de animación
        if (myAnimator != null)
        {
            // El valor de _isShooting ahora está controlado por el temporizador.
            myAnimator.SetBool("isShooting", _isShooting); 
        }
        
        IncreaseBulletsRate();
    }
    
    private void UpdateSpeedBoost()
    {
        if (_isSpeedBoosted)
        {
            _speedBoostTimer -= Time.deltaTime;
            
            if (_speedBoostTimer <= 0f)
            {
                speed = _originalSpeed;
                _isSpeedBoosted = false;
            }
        }
    }

    private void UpdateFireRate()
    { 
        if (bulletsPerSecond > 0)
        {
            _bulletFireRate = 1f / bulletsPerSecond;
        }
        else
        {
            _bulletFireRate = 0.25f;
        }
    }
    
    //  Método para manejar el parpadeo
    private void UpdateDamageBlink()
    {
        if (!_isBlinking) return;
        
        _blinkTimer -= Time.deltaTime;
        _blinkIntervalTimer -= Time.deltaTime;
        
        // Alternar visibilidad del sprite
        if (_blinkIntervalTimer <= 0f)
        {
            _spriteVisible = !_spriteVisible;
            
            if (_spriteRenderer != null)
            {
                // Alternar entre blanco y rojo
                _spriteRenderer.color = _spriteVisible ? Color.white : Color.red;
            }
            
            _blinkIntervalTimer = blinkInterval;
        }
        
        // Terminar el parpadeo
        if (_blinkTimer <= 0f)
        {
            _isBlinking = false;
            isInvulnerable = false;
            
            // Asegurar que el sprite esté visible
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = true;
                _spriteRenderer.color = Color.white;
            }
        }
    }
    
    // Iniciar el efecto de parpadeo
    private void StartDamageBlink()
    {
        _isBlinking = true;
        isInvulnerable = true;
        _blinkTimer = damageBlinkDuration;
        _blinkIntervalTimer = blinkInterval;
    }

    private void ShootDirection()
    {
        if (gun == null) return;
    
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseDirection = mousePosition - transform.position;
        mouseDirection.Normalize();

        float baseAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;
        gun.transform.rotation = Quaternion.AngleAxis(baseAngle, Vector3.forward);

        GameObject bulletToShoot = GetCurrentBullet();
    
        if (bulletToShoot == null)
        {
            Debug.LogWarning("No bullet prefab assigned!");
            return;
        }
    
        // Quaternion angleAdjustment = Quaternion.Euler(0, 0, -90);
        // GameObject instantiatedBullet = Instantiate(bulletToShoot, bulletSpawnOrigin.transform.position, gun.transform.rotation * angleAdjustment);
    
        
        // LÓGICA DE TIRO MÚLTIPLE (CONO)
        
        int bulletsToSpawn = 1;
        float currentSpreadAngle = 0f;
        
        if (bulletsPerSecond > multiShotThreeThreshold)
        {
            // Nivel 3: 3 balas
            bulletsToSpawn = 3;
            currentSpreadAngle = threeShotSpreadAngle;
        }
        else if (bulletsPerSecond > multiShotTwoThreshold)
        {
            // Nivel 2: 2 balas
            bulletsToSpawn = 2;
            currentSpreadAngle = twoShotSpreadAngle;
        }
        else
        {
            // Nivel 1: 1 bala (Disparo único)
            bulletsToSpawn = 1;
        }
        
        // -----------------------------------------------------
        
        // Calcular los ángulos de inicio y el incremento.
        float startAngle = baseAngle;
        
        // Si disparamos más de 1 bala, calculamos el ángulo inicial para centrar el cono
        if (bulletsToSpawn > 1)
        {
            // Mueve el ángulo inicial hacia atrás para que el tiro central esté en el medio del spread
            startAngle = baseAngle - (currentSpreadAngle / 2f);
        }
        
        // El incremento es el ángulo total de spread dividido por (número de balas - 1)
        float angleIncrement = currentSpreadAngle / (bulletsToSpawn > 1 ? (bulletsToSpawn - 1) : 1);


        for (int i = 0; i < bulletsToSpawn; i++)
        {
            float bulletAngle = 0f;
            
            if (bulletsToSpawn == 1)
            {
                bulletAngle = baseAngle;
            }
            else
            {
                // Calcula el ángulo actual dentro del spread
                bulletAngle = startAngle + (angleIncrement * i);
            }
            
            // Convertir el ángulo final a Quaternion (rotación)
            Quaternion finalRotation = Quaternion.Euler(0, 0, bulletAngle - 90);
            
            // Spawnea la bala con la nueva rotación
            Instantiate(bulletToShoot, bulletSpawnOrigin.position, finalRotation);
        }
        
        // Reducir contador DESPUÉS de disparar
        if (currentEggType != EggType.Normal)
        {
            specialEggShots--;
        
            if (specialEggShots <= 0)
            {
                currentEggType = EggType.Normal;
            }
        }
    
        AudioManager.Instance.PlaySound("PlayerShoot");
    }
    
    private GameObject GetCurrentBullet()
    {
        switch (currentEggType)
        {
            case EggType.Fire:
                
                return fireEggBullet != null ? fireEggBullet : bullet;
            
            case EggType.Radioactive:
                
                return radioactiveEggBullet != null ? radioactiveEggBullet : bullet;
            
            case EggType.Normal:
            default:
                
                return bullet;
        }
    }
    
    public void ChangeEggType(EggType newType, int shots)
    {
        currentEggType = newType;
        specialEggShots = shots;
        maxSpecialEggShots = shots;
        
        string eggName = newType == EggType.Fire ? "Fuego" : "Radiactivo";
    }
    
    // Método para obtener info del huevo actual
    public EggType GetCurrentEggType()
    {
        return currentEggType;
    }
    
    // Método para obtener los disparos máximos
    public int GetMaxSpecialEggShots()
    {
        return maxSpecialEggShots;
    }
    
    public int GetSpecialEggShots()
    {
        return specialEggShots;
    }

    private void Movement()
    {
        float directionX = Input.GetAxis("Horizontal");
        float directionY = Input.GetAxis("Vertical");
    
        movement.x = directionX;
        movement.y = directionY;
    
        // Actualizar animación
        if (movement != Vector3.zero)
        {
            myAnimator.SetBool("isWalking", true);
        
            // Simplificado: solo -1 o 1
            if (directionX < 0)
            {
                myAnimator.SetFloat("MovementX", -1f); // Izquierda
                _spriteRenderer.flipX = false;
            }
            else if (directionX > 0)
            {
                myAnimator.SetFloat("MovementX", 1f); // Derecha
                _spriteRenderer.flipX = true;
            }
            // Si solo se mueve en Y, mantener la última dirección X
        }
        else
        {
            myAnimator.SetBool("isWalking", false);
        }
    
        movement = Vector3.ClampMagnitude(movement, 1);
        transform.position += movement * (speed * Time.deltaTime);
    }

    private void IncreaseBulletsRate()
    {
        if (GameManager.Instance.secondsAlive >= _lastUpgradeSecond + 10)
        {
            // Solo aumentar si no ha llegado al máximo
            if (bulletsPerSecond < maxBulletsPerSecond)
            {
                bulletsPerSecond += 1;
                _lastUpgradeSecond = GameManager.Instance.secondsAlive;
            }
            else
            {
                _lastUpgradeSecond = GameManager.Instance.secondsAlive; // Actualizar para evitar spam de logs
            }
        }
    }
    
    public void AddLife(int amount)
    {
        life += amount;
        if (life > maxLife) life = maxLife;
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (!_isSpeedBoosted)
        {
            _originalSpeed = speed;
        }
        
        speed = _originalSpeed * multiplier;
        _speedBoostTimer = duration;
        _isSpeedBoosted = true;
    }
    
    public void IncreaseFireRate(int amount)
    {
        // Aplicar el aumento pero con límite
        bulletsPerSecond += amount;
        
        // Clampear al máximo
        if (bulletsPerSecond > maxBulletsPerSecond)
        {
            bulletsPerSecond = maxBulletsPerSecond;
        }
    }

    public void TakeDamage(int amount)
    {
        // No recibir daño si está en invulnerabilidad
        if (isInvulnerable) return;
        
        life -= amount;
        if (life < 0) life = 0;
        
        if (myAnimator != null)
        {
            myAnimator.SetTrigger("hit");
        }
        
        // Reproducir sonido de daño
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("PlayerHurt");
        }
        
        // Iniciar efecto de parpadeo
        StartDamageBlink();
        
        if (life <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerIsDead = true;
            }
            enabled = false;
            AudioManager.Instance.PlaySound("PlayerDeath");
        }
    }
}