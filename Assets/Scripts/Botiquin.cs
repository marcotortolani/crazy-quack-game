using UnityEngine;

public class Botiquin : MonoBehaviour
{
    public int health;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }
        Player player = collision.GetComponent<Player>();
        if (player.life < player.maxLife)
        {       
            player.AddLife(health);
            Destroy(gameObject);
            Debug.Log("Conseguiste un Botiquin");
        }
    }
}
