using UnityEngine;

public class EnemyPlaneAI : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;
    public LayerMask playerLayer;
    public float detectionRange = 15f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float bulletLifetime = 3f;
    public float fireCooldown = 0.7f;
    public float fireRange = 10f;
    public float fireAngle = 30f;

    [Header("Run Away Settings")]
    public float stuckThresholdTime = 2.5f;
    public float runDuration = 3f;
    public float runSpeed = 10f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    private Transform currentTarget;
    private float fireTimer;
    private float rotationStuckTimer = 0f;
    private bool isRunning = false;
    private float runTimer = 0f;
    private float lastRotationZ = 0f;

    [Header("Missile Settings")]
    public GameObject missilePrefab;
    public Transform missileLaunchPoint;
    public float missileCooldown = 6f;
    public float missileLockTime = 1.5f;
    public float missileRange = 12f;
    public float missileScanAngle = 45f;

    private float missileTimer = 0f;
    private float missileLockTimer = 0f;
    private Transform lockedMissileTarget;

    [Header("Missile Ammo")]
    public int maxMissileAmmo = 3;
    private int currentMissileAmmo;

    [Header("Flare Defense Settings")]
    public LayerMask playerMissileLayer;
    public float flareDetectionRange = 10f;
    [Range(0f, 1f)] public float chanceToFlare = 0.5f;
    public int flareUseCount = 1;

    [Header("Flare Settings")]
    public GameObject flarePrefab;
    public int flareCount = 6;
    public float flareDuration = 3f;
    public float flareCooldown = 10f;
    public float flareFireRate = 0.2f;
    public float flareForwardForce = 3f;

    private bool isFlareActive = false;
    private float flareCooldownTimer = 0f;
    private int flaresUsed = 0;

    [Header("Audio Settings")]
    public AudioSource audioSource;         // Drag AudioSource ke sini
    public AudioClip laserShootSound;       // Drag suara laser ke sini
    public AudioClip missileShootSound; // Suara tembak missile
    public AudioClip flareShootSound;   // Suara tembak flare

    [Header("Explosion FX")]
    public GameObject explosionPrefab;
    public float explosionDelay = 0.1f;
    public AudioClip explosionSound;


    void Start()
    {
        currentHealth = maxHealth;

        currentMissileAmmo = maxMissileAmmo;

    }

    void Update()
    {
        if (!GameManager.IsPaused)
        {
            if (!isRunning)
            {
                FindTarget();
            }

            MoveAndRotate();
            TryShoot();
            fireTimer -= Time.deltaTime;
        }

        missileTimer -= Time.deltaTime;
        ScanAndLockMissileTarget();
        TryLaunchMissile();

        flareCooldownTimer -= Time.deltaTime;
        CheckAndTriggerFlareDefense();


    }

    void FindTarget()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, detectionRange, playerLayer);

        if (targets.Length == 0)
        {
            currentTarget = null;
            return;
        }

        float minDist = Mathf.Infinity;
        Collider2D selected = null;

        foreach (Collider2D t in targets)
        {
            float dist = Vector2.Distance(transform.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                selected = t;
            }
        }

        currentTarget = selected.transform;
    }

    void MoveAndRotate()
    {
        if (isRunning)
        {
            transform.Translate(Vector2.up * runSpeed * Time.deltaTime);
            runTimer -= Time.deltaTime;

            if (runTimer <= 0f)
            {
                isRunning = false;
                rotationStuckTimer = 0f;
            }

            return;
        }

        if (currentTarget == null)
        {
            transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
            return;
        }

        Vector2 dir = (currentTarget.position - transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        float deltaRotation = Mathf.Abs(Mathf.DeltaAngle(lastRotationZ, transform.eulerAngles.z));
        if (deltaRotation < 1f)
        {
            rotationStuckTimer += Time.deltaTime;

            if (rotationStuckTimer >= stuckThresholdTime)
            {
                isRunning = true;
                runTimer = runDuration;
                rotationStuckTimer = 0f;
                return;
            }
        }
        else
        {
            rotationStuckTimer = 0f;
        }

        lastRotationZ = transform.eulerAngles.z;

        transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
    }

    void TryShoot()
    {
        if (currentTarget == null || fireTimer > 0f || isRunning) return;

        Vector2 dirToTarget = (currentTarget.position - transform.position).normalized;
        float angle = Vector2.Angle(transform.up, dirToTarget);
        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (angle <= fireAngle / 2f && dist <= fireRange)
        {
            Fire();
            fireTimer = fireCooldown;
        }
    }

    void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = transform.up * bulletSpeed;
        }

        Destroy(bullet, bulletLifetime);

        if (audioSource != null && laserShootSound != null)
        {
            audioSource.PlayOneShot(laserShootSound);
        }


    }

    // FUNGSI TAMBAHAN UNTUK DAMAGE
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // Ledakan visual
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Suara ledakan
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Delay sebelum hilang (biar suara dan efek sempat tampil)
        Destroy(gameObject, explosionDelay);

        // Notify ke manager
        SurvivalGameManager gm = FindObjectOfType<SurvivalGameManager>();
        if (gm != null)
        {
            gm.OnEnemyDestroyed();
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fireRange);
    }

    void ScanAndLockMissileTarget()
    {
        if (lockedMissileTarget != null)
        {
            if (!lockedMissileTarget.gameObject.activeInHierarchy || Vector2.Distance(transform.position, lockedMissileTarget.position) > missileRange)
            {
                lockedMissileTarget = null;
                missileLockTimer = 0f;
            }
            else
            {
                missileLockTimer += Time.deltaTime;
                return;
            }
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, missileRange, playerLayer);
        Transform selected = null;
        float minAngle = Mathf.Infinity;

        foreach (var hit in hits)
        {
            Vector2 toTarget = (hit.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(transform.up, toTarget);

            if (angle <= missileScanAngle / 2f && angle < minAngle)
            {
                minAngle = angle;
                selected = hit.transform;
            }
        }

        if (selected != null)
        {
            lockedMissileTarget = selected;
            missileLockTimer = 0f;
        }
    }

    void TryLaunchMissile()
    {
        if (lockedMissileTarget == null || missileTimer > 0f || currentMissileAmmo <= 0) return;

        if (missileLockTimer >= missileLockTime)
        {
            FireMissile(lockedMissileTarget);
            missileTimer = missileCooldown;
            missileLockTimer = 0f;
            lockedMissileTarget = null;
            currentMissileAmmo--;
        }
    }


    void FireMissile(Transform target)
    {
        if (missilePrefab == null || missileLaunchPoint == null) return;

        GameObject missile = Instantiate(missilePrefab, missileLaunchPoint.position, transform.rotation);
        EnemyMissile missileScript = missile.GetComponent<EnemyMissile>();
        if (missileScript != null)
        {
            missileScript.LockOn(target);
        }

        if (audioSource != null && missileShootSound != null)
        {
            audioSource.PlayOneShot(missileShootSound);
        }

    }

    void CheckAndTriggerFlareDefense()
    {
        if (isFlareActive || flareCooldownTimer > 0f || flaresUsed >= flareUseCount) return;

        Collider2D[] missiles = Physics2D.OverlapCircleAll(transform.position, flareDetectionRange, playerMissileLayer);

        if (missiles.Length > 0)
        {
            if (Random.value <= chanceToFlare)
            {
                StartCoroutine(DeployFlare());
                flareCooldownTimer = flareCooldown;
                flaresUsed++;
            }
        }
    }

    System.Collections.IEnumerator DeployFlare()
    {
        isFlareActive = true;

        for (int i = 0; i < flareCount; i++)
        {
            GameObject flare = Instantiate(flarePrefab, transform.position, Quaternion.identity);
            if (audioSource != null && flareShootSound != null)
            {
                audioSource.PlayOneShot(flareShootSound);
            }

            Rigidbody2D rb = flare.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 forceDir = (new Vector2(transform.up.x, transform.up.y) + Random.insideUnitCircle * 0.3f).normalized;
                rb.AddForce(forceDir * flareForwardForce, ForceMode2D.Impulse);
            }

            Destroy(flare, flareDuration);
            yield return new WaitForSeconds(flareFireRate);
        }

        isFlareActive = false;
    }


}
