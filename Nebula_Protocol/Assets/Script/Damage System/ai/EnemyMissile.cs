using UnityEngine;

public class EnemyMissile : MonoBehaviour
{
    public float startSpeed = 5f;
    public float maxSpeed = 20f;
    public float acceleration = 15f;
    public float lifetime = 6f;

    private Transform target;
    private float currentSpeed;
    private bool isLocked = false;

    [Header("Flare Detection")]
    public LayerMask flareLayer;
    public float flareScanRange = 5f;
    public float flareScanAngle = 60f; // derajat
    public int flareCountToDeactivate = 3;

    private bool isDistracted = false;

    public Transform Target => target;


    void Start()
    {
        currentSpeed = startSpeed;
        Destroy(gameObject, lifetime);
    }

    public void LockOn(Transform targetTransform)
    {
        target = targetTransform;
        isLocked = true;
    }

    void Update()
    {
        if (isDistracted || !isLocked || target == null)
        {
            // Jalan lurus terus
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
            transform.Translate(Vector2.up * currentSpeed * Time.deltaTime);
            return;
        }

        // Cek apakah flare cukup untuk ganggu lock
        if (IsDistractedByFlare())
        {
            isLocked = false;
            isDistracted = true;
            target = null;
            return;
        }

        // Masih ngelock target
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle), 360f * Time.deltaTime);

        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        transform.Translate(Vector2.up * currentSpeed * Time.deltaTime);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform == target)
        {
            Destroy(gameObject);
            // Tambahkan efek ledakan di sini jika perlu
        }
    }

    bool IsDistractedByFlare()
    {
        Collider2D[] flares = Physics2D.OverlapCircleAll(transform.position, flareScanRange, flareLayer);

        int validFlareCount = 0;
        Vector2 forward = transform.up;

        foreach (Collider2D flare in flares)
        {
            Vector2 toFlare = ((Vector2)flare.transform.position - (Vector2)transform.position).normalized;
            float angle = Vector2.Angle(forward, toFlare);
            if (angle <= flareScanAngle / 2f)
            {
                validFlareCount++;
            }

            if (validFlareCount >= flareCountToDeactivate)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsLockedOnPlayer()
    {
        return isLocked && target != null && target.CompareTag("Player"); // pastikan tag Player digunakan
    }


}