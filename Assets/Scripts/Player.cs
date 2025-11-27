using System.Linq.Expressions;
using UnityEngine;

public class Player : MonoBehaviour
{
    private EggType currentEggType = EggType.Normal;
    private int specialEggShots = 0; // Disparos restantes con huevo especial
    
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
    
    // variables privadas para el cálculo de la frecuencia de spawneo
    private float _nextFireTime = 0f;
    private float _bulletFireRate;
    private int _lastUpgradeSecond = 0;
    
    // Variables para el speed boost
    private float _speedBoostTimer = 0f;
    private float _originalSpeed;
    private bool _isSpeedBoosted = false;

    private void Start()
    {
        life = maxLife;
        _originalSpeed = speed;
        
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
                Debug.Log($"Velocidad restaurada: {speed}");
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

    private void ShootDirection()
    {
        if (gun == null) return;
    
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseDirection = mousePosition - transform.position;
        mouseDirection.Normalize();

        float angle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;
        gun.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject bulletToShoot = GetCurrentBullet();
    
        Debug.Log($"Bullet a disparar: {(bulletToShoot != null ? bulletToShoot.name : "NULL")}"); // ← NUEVO
    
        if (bulletToShoot == null)
        {
            Debug.LogWarning("No bullet prefab assigned!");
            return;
        }
    
        Quaternion angleAdjustment = Quaternion.Euler(0, 0, -90);
        GameObject instantiatedBullet = Instantiate(bulletToShoot, bulletSpawnOrigin.transform.position, gun.transform.rotation * angleAdjustment);
    
        Debug.Log($"Bullet instanciado: {instantiatedBullet.name}"); // ← NUEVO
    
        // Reducir contador DESPUÉS de disparar
        if (currentEggType != EggType.Normal)
        {
            specialEggShots--;
            Debug.Log($"Disparo especial usado. Quedan: {specialEggShots}");
        
            if (specialEggShots <= 0)
            {
                currentEggType = EggType.Normal;
                Debug.Log("Huevo especial agotado, volviendo a huevo normal");
            }
        }
    
        AudioManager.Instance.PlaySound("PlayerShoot");
    }
    
    private GameObject GetCurrentBullet()
    {
        Debug.Log($"GetCurrentBullet llamado. Current type: {currentEggType}, Shots: {specialEggShots}");
        
        switch (currentEggType)
        {
            case EggType.Fire:
                Debug.Log($"Devolviendo Fire Egg. Prefab asignado: {fireEggBullet != null}");
                return fireEggBullet != null ? fireEggBullet : bullet;
            
            case EggType.Radioactive:
                Debug.Log($"Devolviendo Radioactive Egg. Prefab asignado: {radioactiveEggBullet != null}");
                return radioactiveEggBullet != null ? radioactiveEggBullet : bullet;
            
            case EggType.Normal:
            default:
                Debug.Log("Devolviendo huevo normal");
                return bullet;
        }
    }
    
    public void ChangeEggType(EggType newType, int shots)
    {
        currentEggType = newType;
        specialEggShots = shots;
        
        string eggName = newType == EggType.Fire ? "Fuego" : "Radiactivo";
        Debug.Log($"Huevo especial recogido: {eggName} ({shots} disparos)");
    }
    
    // Método para obtener info del huevo actual
    public EggType GetCurrentEggType()
    {
        return currentEggType;
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
                Debug.Log($"Fire rate aumentado: {bulletsPerSecond}/{maxBulletsPerSecond} balas/segundo");
            }
            else
            {
                Debug.Log($"Fire rate al máximo: {maxBulletsPerSecond} balas/segundo");
                _lastUpgradeSecond = GameManager.Instance.secondsAlive; // Actualizar para evitar spam de logs
            }
        }
    }
    
    public void AddLife(int amount)
    {
        life += amount;
        if (life > maxLife) life = maxLife;
        Debug.Log($"Vida restaurada: {life}/{maxLife}");
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
        
        Debug.Log($"Velocidad aumentada: {_originalSpeed} → {speed} por {duration}s");
    }
    
    public void IncreaseFireRate(int amount)
    {
        // Aplicar el aumento pero con límite
        bulletsPerSecond += amount;
        
        // Clampear al máximo
        if (bulletsPerSecond > maxBulletsPerSecond)
        {
            bulletsPerSecond = maxBulletsPerSecond;
            Debug.Log($"Fire rate al máximo: {maxBulletsPerSecond} balas/segundo");
        }
        else
        {
            Debug.Log($"Fire rate aumentado: {bulletsPerSecond}/{maxBulletsPerSecond} balas/segundo");
        }
    }

    public void TakeDamage(int amount)
    {
        life -= amount;
        if (life < 0) life = 0;
        
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