using System.Linq.Expressions;
using UnityEngine;

public class PlayerOld : MonoBehaviour
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
    
    // variables privadas para el cálculo de la frecuencia de spawneo
    private float nextFireTime = 0f;
    private float bulletFireRate;
    private int lastUpgradeSecond = 0;

    private void Start()
    {
        life = maxLife;
    }

    private void Update()
    {
        Movement();
        UpdateFireRate();

        if(!GameManagerOld.Instance) return;
        if (!GameManagerOld.Instance.playerIsDead && !GameManagerOld.Instance.playerIsWin)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + bulletFireRate;
                ShootDirection();
            }
        }
        IncreaseBulletsRate();
    }

    private void UpdateFireRate()
    { 
        if (bulletsPerSecond > 0)
        {
            bulletFireRate = 1f / bulletsPerSecond;
        }
        else
        {
            bulletFireRate = 0.25f; // valor por defecto si es 0 o negativo - 4 bullets por segundo
        }
        
    }

    private void ShootDirection()
    {
        // Dirección del mouse para apuntar
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseDirection = mousePosition - transform.position;
        mouseDirection.Normalize();

        // Angulo de direccion para el objeto Gun
        float angle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;
        gun.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Spawn bullets
        Quaternion angleAdjustment = Quaternion.Euler(0, 0, -90);
        Instantiate(bullet, bulletSpawnOrigin.transform.position, gun.transform.rotation * angleAdjustment);
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
        
        // con ClampMagnitude podemos normalizar el movement con un maximo de 1, sin perder el smooth
        movement = Vector3.ClampMagnitude(movement, 1);

        transform.position += movement * (speed * Time.deltaTime);
    }

    private void IncreaseBulletsRate()
    {
        if (GameManagerOld.Instance.secondsAlive >= lastUpgradeSecond + 10)
        {
            bulletsPerSecond += 1;
            lastUpgradeSecond = GameManagerOld.Instance.secondsAlive;
            Debug.Log("Velocidad de disparo: " + bulletsPerSecond);
        }
    }
    public void AddLife(int amount)
    {
        life += amount;
        if (life > maxLife) life = maxLife;
    }

    public void TakeDamage(int amount)
    {
        life -= amount;
        Debug.Log("Recibiste daño, vida actual: " + life);
        
        if (life <= 0)
        {
            if (GameManagerOld.Instance != null)
            {
                GameManagerOld.Instance.playerIsDead = true;
            }
            Destroy(gameObject);
        }
    }
}