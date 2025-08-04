using UnityEngine;

public class DamageToPlayer : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 20f; // Jumlah damage ke player

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Cek apakah yang terkena adalah player
        PlayerPlaneMovement player = collision.GetComponent<PlayerPlaneMovement>();
        if (player != null)
        {
            player.TakeDamage(damageAmount);
            Destroy(gameObject); // Hancurkan peluru setelah kena
        }
    }
}
