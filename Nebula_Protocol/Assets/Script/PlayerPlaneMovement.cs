using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;


public class PlayerPlaneMovement : MonoBehaviour
{
    private Transform enemyCheckpointTarget = null;

    public PlaneControlUI planeControlUI;

    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;
    public GameObject checkpointMarkerPrefab; // Prefab untuk marker checkpoint
    public LayerMask checkpointLayer; // Layer mask untuk checkpoint yang dituju
    public float checkpointReachDistance = 0.5f; // Jarak untuk anggap sampai di checkpoint

    [Header("Line Renderer Customization")]
    public float startWidth = 0.1f; // Lebar awal garis
    public float endWidth = 0.1f; // Lebar akhir garis
    public Color startColor = Color.yellow; // Warna awal garis
    public Color endColor = Color.yellow; // Warna akhir garis
    public Material lineMaterial; // Material untuk garis (opsional)

    private Vector2? targetPosition = null;
    private GameObject currentCheckpointMarker;
    private bool isFollowingCheckpoint = false;
    private Vector2 lastDirection; // Arah terakhir untuk gerakan lurus
    private bool isControlActive = false; // Status kontrol aktif/tidak
    private LineRenderer lineRenderer; // Referensi ke LineRenderer

    public static PlayerPlaneMovement selectedPlane;

    [Header("Auto Shoot Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float bulletLifetime = 3f; // jarak peluru hidup (dihancurkan setelah waktu)
    public float scanRange = 10f;
    public float scanAngle = 30f;
    public LayerMask enemyLayer;
    public float shootCooldown = 0.5f;

    private float shootTimer = 0f;
    //
    private Transform currentLockTarget;
    private float lockTimer = 0f;
    private GameObject currentLockUI;

    [Header("Missile Lock System")]
    public float scanRangeMissile = 8f;
    public float scanAngleMissile = 25f;
    public float timeToLock = 2f;

    public GameObject lockIndicatorPrefab;       // Prefab merah (sudah terkunci)
    public GameObject lockingIndicatorPrefab;    // Prefab oranye (dalam proses mengunci)

    [Header("Missile Settings")]
    public GameObject missilePrefab;
    public float missileCooldown = 3f;

    private float missileTimer = 0f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    public Image healthBarImage; // Drag UI Image (Filled) dari Canvas

    [Header("Elimination UI")]
    public GameObject eliminationUI; // Drag GameObject UI yang muncul saat pesawat hancur
    public PlaneControlUI controlUI; // Drag komponen PlaneControlUI yang terkait

    [Header("Flare Settings")]
    public GameObject flarePrefab;
    public int flareCount = 6;
    public float flareDuration = 3f;
    public float flareCooldown = 10f;

    private bool isFlareActive = false;
    private float flareCooldownTimer = 0f;
    public bool IsFlareActive => isFlareActive; // Untuk diakses dari luar

    [Header("Flare Advanced Settings")]
    public float flareFireRate = 0.2f;  // jeda antar flare
    public float flareForwardForce = 3f; // gaya dorong ke depan

    [Header("Missile Warning UI")]
    public GameObject missileAlertUI; // Drag UI missile warning image ke sini dari Canvas

    [Header("Audio Settings")]
    public AudioSource audioSource; // AudioSource untuk suara
    public AudioClip laserShootSound; // Suara laser
    public AudioClip missileShootSound; // Suara tembak missile
    public AudioClip flareShootSound;   // Suara tembak flare
    public AudioClip playerDestroySound; // Suara ledakan player
    public AudioClip checkpointPlacedSound; // Suara saat checkpoint dipasang
    public AudioClip missileLockLoopSound; // Suara lock looping
    private AudioSource lockLoopAudioSource; // AudioSource khusus looping


    [Header("Explosion Effect")]
    public GameObject explosionPrefab; // Prefab ledakan (misalnya animasi atau particle)
    public float explosionDelayBeforeDestroy = 1.5f; // Waktu tunggu sebelum hancur



    void Start()
    {
        // Ambil komponen LineRenderer dari GameObject ini
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        // Inisialisasi LineRenderer dengan kostumisasi
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        lineRenderer.useWorldSpace = true;

        currentHealth = maxHealth;

        lockLoopAudioSource = gameObject.AddComponent<AudioSource>();
        lockLoopAudioSource.loop = true;
        lockLoopAudioSource.playOnAwake = false;
        lockLoopAudioSource.clip = missileLockLoopSound;

        UpdateHealthBar();

    }

    void Update()
    {
        // Handle input jika pesawat dipilih dan kontrol aktif
        if (selectedPlane == this && isControlActive)
        {
            HandleInput();
        }

        // Gerak dan rotasi hanya jika tidak pause
        if (!GameManager.IsPaused)
        {
            MoveAndRotate();
        }

        // Update garis setiap frame, termasuk saat pause
        UpdateLineRenderer();
        //
        if (!GameManager.IsPaused)
        {
            shootTimer -= Time.deltaTime;
            TryAutoShoot();
        }

        //
        TryMissileLockSystem();

        if (!GameManager.IsPaused)
        {
            missileTimer -= Time.deltaTime;
        }

        if (!GameManager.IsPaused)
        {
            if (isFlareActive)
            {
                flareDuration -= Time.deltaTime;
                if (flareDuration <= 0f)
                {
                    isFlareActive = false;
                    flareDuration = 3f; // Reset agar bisa dipakai lagi nanti
                }
            }

            if (flareCooldownTimer > 0f)
                flareCooldownTimer -= Time.deltaTime;
        }

        if (!GameManager.IsPaused)
        {
            CheckMissileThreat();
        }


    }


    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = currentHealth / maxHealth;
        }
    }
    //
    void Die()
    {
        Debug.Log($"{gameObject.name} has been destroyed.");

        if (selectedPlane == this)
        {
            selectedPlane = null;
        }

        if (controlUI != null)
        {
            controlUI.DisableUI();
        }

        if (eliminationUI != null)
        {
            eliminationUI.SetActive(true);
        }

        if (currentLockUI != null)
        {
            Destroy(currentLockUI);
            currentLockUI = null;
        }

        StopMissileLockAudio();

        

        currentLockTarget = null;
        lockTimer = 0f;

        // Nonaktifkan gerakan & collider agar diam
        isControlActive = false;
        isFollowingCheckpoint = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Tampilkan ledakan di posisi pesawat
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Mainkan suara ledakan
        if (audioSource != null && playerDestroySound != null)
        {
            audioSource.PlayOneShot(playerDestroySound);
        }

        // Delay sebelum dihancurkan
        StartCoroutine(DestroyAfterDelay());

        


    }

    //
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelayBeforeDestroy);
        Destroy(gameObject);
    }


    //
    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(clickPos, Vector2.zero, Mathf.Infinity, enemyLayer);

            if (hit.collider != null)
            {
                // Klik mengenai musuh
                enemyCheckpointTarget = hit.transform;
                isFollowingCheckpoint = true;

                if (checkpointMarkerPrefab != null)
                {
                    if (currentCheckpointMarker != null)
                        Destroy(currentCheckpointMarker);

                    currentCheckpointMarker = Instantiate(
                        checkpointMarkerPrefab,
                        enemyCheckpointTarget.position,
                        Quaternion.identity
                    );

                    if (audioSource != null && checkpointPlacedSound != null)
                    {
                        audioSource.PlayOneShot(checkpointPlacedSound);
                    }

                    currentCheckpointMarker.layer = LayerMaskToLayer(checkpointLayer);
                }

                return;
            }

            // Jika klik kosong dan marker sedang follow musuh, maka hentikan follow
            if (enemyCheckpointTarget != null)
            {
                enemyCheckpointTarget = null;
                isFollowingCheckpoint = false;

                if (currentCheckpointMarker != null)
                    Destroy(currentCheckpointMarker);
                currentCheckpointMarker = null;

                return;
            }

            // Jika bukan klik enemy dan marker sedang tidak follow musuh, buat checkpoint manual
            targetPosition = clickPos;
            isFollowingCheckpoint = true;

            if (checkpointMarkerPrefab != null)
            {
                if (currentCheckpointMarker != null)
                    Destroy(currentCheckpointMarker);

                currentCheckpointMarker = Instantiate(
                    checkpointMarkerPrefab,
                    new Vector3(clickPos.x, clickPos.y, 0f),
                    Quaternion.identity
                );
                //
                if (audioSource != null && checkpointPlacedSound != null)
                {
                    audioSource.PlayOneShot(checkpointPlacedSound);
                }

                currentCheckpointMarker.layer = LayerMaskToLayer(checkpointLayer);
            }
        }
    }


    void MoveAndRotate()
    {
        Vector2? followTarget = null;

        if (enemyCheckpointTarget != null)
        {
            followTarget = enemyCheckpointTarget.position;
        }
        else if (isFollowingCheckpoint && targetPosition.HasValue)
        {
            followTarget = targetPosition;
        }

        if (followTarget.HasValue)
        {
            Vector2 dir = ((Vector2)followTarget.Value - (Vector2)transform.position).normalized;
            float distance = Vector2.Distance(transform.position, followTarget.Value);

            if (distance <= checkpointReachDistance && enemyCheckpointTarget == null)
            {
                isFollowingCheckpoint = false;
                targetPosition = null;
                lastDirection = transform.up;

                if (currentCheckpointMarker != null)
                {
                    Destroy(currentCheckpointMarker);
                    currentCheckpointMarker = null;
                }
            }
            else
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                float step = rotateSpeed * Time.deltaTime;
                Quaternion targetRot = Quaternion.Euler(0, 0, angle);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, step);
            }
        }

        transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);

        // Update posisi marker jika follow musuh
        if (enemyCheckpointTarget != null && currentCheckpointMarker != null)
        {
            currentCheckpointMarker.transform.position = enemyCheckpointTarget.position;
        }

        lineRenderer.startColor = (enemyCheckpointTarget != null) ? Color.red : startColor;


    }

    //
    void UpdateLineRenderer()
    {
        if (lineRenderer == null)
            return;

        if (isFollowingCheckpoint)
        {
            Vector3 endPoint;

            if (enemyCheckpointTarget != null)
            {
                endPoint = enemyCheckpointTarget.position;
            }
            else if (targetPosition.HasValue)
            {
                endPoint = new Vector3(targetPosition.Value.x, targetPosition.Value.y, transform.position.z);
            }
            else
            {
                lineRenderer.positionCount = 0;
                return;
            }

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endPoint);
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }



    // Fungsi untuk mengaktifkan kontrol
    public void ActivateControl()
    {
        isControlActive = true;
        selectedPlane = this;
        Debug.Log($"{gameObject.name} control activated");
    }

    // Fungsi untuk menonaktifkan kontrol
    public void DeactivateControl()
    {
        isControlActive = false;
        if (selectedPlane == this)
        {
            selectedPlane = null;
        }
        // Tidak menghancurkan checkpoint marker saat deactivate
        Debug.Log($"{gameObject.name} control deactivated");

        StopMissileLockAudio();

    }

    // Helper untuk mengubah LayerMask ke layer index
    private int LayerMaskToLayer(LayerMask layerMask)
    {
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 0)
        {
            layer >>= 1;
            layerNumber++;
        }
        return layerNumber - 1;
    }

    void TryAutoShoot()
    {
        // Scan musuh di sekitar
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Vector2 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angleToTarget = Vector2.Angle(transform.up, dirToTarget); // hanya depan (berbasis transform.up)

            if (angleToTarget <= scanAngle / 2f)
            {
                if (shootTimer <= 0f)
                {
                    Shoot();
                    shootTimer = shootCooldown;
                }
                return; // hanya tembak 1 target terdekat
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = transform.up * bulletSpeed;
            }

            Destroy(bullet, bulletLifetime);
        }

        if (audioSource != null && laserShootSound != null)
        {
            audioSource.PlayOneShot(laserShootSound);
        }

    }

    void TryMissileLockSystem()
    {
        if (GameManager.IsPaused) return;

        if (currentLockTarget == null)
        {
            // Cari target baru
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRangeMissile, enemyLayer);
            foreach (var hit in hits)
            {
                Vector2 dirToTarget = (hit.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(transform.up, dirToTarget);
                if (angle <= scanAngleMissile / 2f)
                {
                    currentLockTarget = hit.transform;
                    lockTimer = 0f;

                    // Buat prefab "dalam proses lock"
                    if (lockingIndicatorPrefab != null)
                    {
                        currentLockUI = Instantiate(lockingIndicatorPrefab, currentLockTarget.position, Quaternion.identity, currentLockTarget);
                    }

                    // 🔊 Mulai suara lock looping
                    if (!lockLoopAudioSource.isPlaying && missileLockLoopSound != null)
                    {
                        lockLoopAudioSource.Play();
                    }

                    break;
                }
            }
        }
        else
        {
            // Target masih valid
            Vector2 dirToTarget = (currentLockTarget.position - transform.position).normalized;
            float angle = Vector2.Angle(transform.up, dirToTarget);
            float distance = Vector2.Distance(transform.position, currentLockTarget.position);

            if (angle <= scanAngleMissile / 2f && distance <= scanRangeMissile)
            {
                lockTimer += Time.deltaTime;

                if (lockTimer >= timeToLock)
                {
                    if (currentLockUI != null) Destroy(currentLockUI);
                    if (lockIndicatorPrefab != null)
                    {
                        currentLockUI = Instantiate(lockIndicatorPrefab, currentLockTarget.position, Quaternion.identity, currentLockTarget);
                    }
                }

                // 🔁 Jangan ulangi Play saat sudah main
                if (!lockLoopAudioSource.isPlaying && missileLockLoopSound != null)
                {
                    lockLoopAudioSource.Play();
                }
            }
            else
            {
                // Musuh keluar area
                if (currentLockUI != null) Destroy(currentLockUI);
                currentLockTarget = null;
                lockTimer = 0f;

                // 🔇 Stop suara lock
                if (lockLoopAudioSource.isPlaying)
                {
                    lockLoopAudioSource.Stop();
                }
            }
        }
    }


    void FireMissile(Transform target)
    {
        if (missilePrefab != null && target != null)
        {
            GameObject missile = Instantiate(missilePrefab, transform.position, transform.rotation);
            PlayerMissile missileScript = missile.GetComponent<PlayerMissile>();
            if (missileScript != null)
            {
                missileScript.SetTarget(target);
            }
        }

        if (audioSource != null && missileShootSound != null)
        {
            audioSource.PlayOneShot(missileShootSound);
        }

    }

    public float GetMissileCooldown()
    {
        return missileTimer;
    }

    public void TryManualFireMissile()
    {
        if (currentLockTarget != null && lockTimer >= timeToLock && missileTimer <= 0f)
        {
            FireMissile(currentLockTarget);
            missileTimer = missileCooldown;
        }
    }

    public void DeployFlare()
    {
        if (flareCooldownTimer > 0f || isFlareActive || flarePrefab == null)
            return;

        isFlareActive = true;
        flareCooldownTimer = flareCooldown;

        StartCoroutine(DeployFlareRoutine());
    }

    private IEnumerator DeployFlareRoutine()
    {
        for (int i = 0; i < flareCount; i++)
        {
            Vector2 spawnOffset;
            Vector2 flareDirection = transform.up;

            // Ganti sisi kiri-kanan setiap flare
            if (i % 2 == 0)
            {
                spawnOffset = transform.right * 0.6f; // kanan
            }
            else
            {
                spawnOffset = -transform.right * 0.6f; // kiri
            }

            Vector2 spawnPos = (Vector2)transform.position + spawnOffset;

            GameObject flare = Instantiate(flarePrefab, spawnPos, Quaternion.identity);
            if (audioSource != null && flareShootSound != null)
            {
                audioSource.PlayOneShot(flareShootSound);
            }

            Rigidbody2D rb = flare.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(flareDirection * flareForwardForce, ForceMode2D.Impulse);
            }

            Destroy(flare, flareDuration);

            yield return new WaitForSeconds(flareFireRate);
        }

        isFlareActive = false;
    }


    public float GetFlareCooldownTime()
    {
        return Mathf.Max(0f, flareCooldownTimer);
    }

    void CheckMissileThreat()
    {
        EnemyMissile[] missiles = FindObjectsOfType<EnemyMissile>();
        bool underThreat = false;

        foreach (var missile in missiles)
        {
            if (missile.IsLockedOnPlayer() && missile.Target == transform)
            {
                underThreat = true;
                break;
            }
        }

        if (missileAlertUI != null)
        {
            missileAlertUI.SetActive(underThreat);

            if (underThreat && planeControlUI != null)
            {
                planeControlUI.PlayMissileWarning();
            }
        }



    }

    void StopMissileLockAudio()
    {
        if (lockLoopAudioSource != null && lockLoopAudioSource.isPlaying)
        {
            lockLoopAudioSource.Stop();
        }
    }

    private void OnDisable()
    {
        StopMissileLockAudio();
    }


}