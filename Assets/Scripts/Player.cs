using System.Linq.Expressions;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public Vector3 movement;
    public Animator myAnimator;
    public int life;
    public int maxLife = 10;
    public GameObject gun;
    public GameObject bullet;
    public Transform bulletSpawnOrigin;
    public int bulletsPerSecond = 4;
    public int maxBulletsPerSecond = 20; // Límite máximo para evitar excesivas instancias de balas
    
    public bool canShoot = true;
    
    // variables privadas para el cálculo de la frecuencia de spawneo
    private float _nextFireTime = 0f;
    private float _bulletFireRate;
    private int _lastUpgradeSecond = 0;
    
    // Variables para el speed boost (sin coroutine)
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
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseDirection = mousePosition - transform.position;
        mouseDirection.Normalize();

        float angle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;
        gun.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Quaternion angleAdjustment = Quaternion.Euler(0, 0, -90);
        Instantiate(bullet, bulletSpawnOrigin.transform.position, gun.transform.rotation * angleAdjustment);
        
        AudioManager.Instance.PlaySound("PlayerShoot");
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