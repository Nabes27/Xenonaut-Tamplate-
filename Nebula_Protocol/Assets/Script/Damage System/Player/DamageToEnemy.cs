using UnityEngine;

public class DamageToEnemy : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 25f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyPlaneAI enemy = collision.GetComponent<EnemyPlaneAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(damageAmount);
            Destroy(gameObject); // Hancurkan peluru
        }
    }
}
