using UnityEngine;

public class AITurret : MonoBehaviour
{
    [Header("Targeting Settings")]
    public LayerMask targetLayer;
    public float detectionRadius = 10f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f; // Waktu antar tembakan (detik)
    public float bulletSpeed = 10f;

    private float fireCooldown = 0f;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        Transform target = FindNearestTarget();
        if (target != null && fireCooldown <= 0f)
        {
            FireAtTarget(target);
            fireCooldown = fireRate;
        }
    }

    Transform FindNearestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);
        float minDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }

        return nearest;
    }

    void FireAtTarget(Transform target)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Arah dari turret ke target
        Vector2 direction = (target.position - firePoint.position).normalized;

        // Rotasi peluru ke arah target
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Dorong peluru
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
