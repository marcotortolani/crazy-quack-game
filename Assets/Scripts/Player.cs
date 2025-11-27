using System.Linq.Expressions;
using UnityEngine;

public class Player : MonoBehaviour
{
    private EggType currentEggType = EggType.Normal;
    private int specialEggShots = 0; // Disparos restantes con huevo especial
    private int maxSpecialEggShots = 0; // Para calcular el porcentaje
    
    public float speed;
    public Vector3 movement;
    public Animator myAnimator;
    public int life;
    public int maxLife = 10;
    
    [Header("Shooting")]
    public GameObject gun;
    public GameObject bullet;               // Huevo normal
    public GameObject fireEggBullet;        // Huevo de fuego
    public GameObject radioactiveEggBullet; // Huevo radiactivo
    public Transform bulletSpawnOrigin;
    public int bulletsPerSecond = 4;
    public int maxBulletsPerSecond = 20; // Límite máximo para evitar excesivas instancias de balas
    
    public bool canShoot = true;
    
    [Header("Damage Effect")]
    public float damageBlinkDuration = 1f;      // Duración total del parpadeo
    public float blinkInterval = 0.1f;          // Velocidad del parpadeo
    public bool isInvulnerable = false;         // Invulnerabilidad temporal
    
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
        if (canShoot && !GameManager.Instance.playerIsDead && !GameManager.Instance.playerIsWin)
        {
            if (Time.time >= _nextFireTime)
            {
                _nextFireTime = Time.time + _bulletFireRate;
                ShootDirection();
            }
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

        float angle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;
        gun.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject bulletToShoot = GetCurrentBullet();
    
        if (bulletToShoot == null)
        {
            Debug.LogWarning("No bullet prefab assigned!");
            return;
        }
    
        Quaternion angleAdjustment = Quaternion.Euler(0, 0, -90);
        GameObject instantiatedBullet = Instantiate(bulletToShoot, bulletSpawnOrigin.transform.position, gun.transform.rotation * angleAdjustment);
    
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
        maxSpecialEggShots = shots; // Guardar el máximo
        
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

        if (movement != Vector3.zero)
        {
            myAnimator.SetBool("isWalking", true);
            myAnimator.SetFloat("MovementY", directionY);
            myAnimator.SetFloat("MovementX", directionX);
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