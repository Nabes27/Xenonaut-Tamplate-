using UnityEngine;

public class AISAMLauncher : MonoBehaviour
{
    [Header("Targeting")]
    public LayerMask targetLayer;
    public float detectionRadius = 15f;

    [Header("Missile Settings")]
    public GameObject missilePrefab;
    public Transform launchPoint;
    public float fireCooldown = 8f;
    public int maxAmmo = 3;
    public float lockTime = 1.5f;

    private float cooldownTimer = 0f;
    private int ammoLeft;
    private float lockTimer = 0f;
    private Transform currentTarget;

    void Start()
    {
        ammoLeft = maxAmmo;
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (ammoLeft <= 0) return;

        if (currentTarget == null)
        {
            currentTarget = FindNearestTarget();
            lockTimer = 0f;
        }

        if (currentTarget != null)
        {
            lockTimer += Time.deltaTime;

            if (lockTimer >= lockTime && cooldownTimer <= 0f)
            {
                LaunchMissile(currentTarget);
                cooldownTimer = fireCooldown;
                ammoLeft--;
                currentTarget = null;
            }
        }
    }

    Transform FindNearestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);
        float minDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (var hit in hits)
        {
            // Cek apakah target sedang flare
            PlayerPlaneMovement targetPlane = hit.GetComponent<PlayerPlaneMovement>();
            if (targetPlane != null && targetPlane.IsFlareActive)
            {
                continue; // Lewati jika flare aktif
            }

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }

        return nearest;
    }


    void LaunchMissile(Transform target)
    {
        if (missilePrefab == null || launchPoint == null || target == null) return;

        GameObject missile = Instantiate(missilePrefab, launchPoint.position, Quaternion.identity);
        EnemyMissile missileScript = missile.GetComponent<EnemyMissile>();
        if (missileScript != null)
        {
            missileScript.LockOn(target);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
