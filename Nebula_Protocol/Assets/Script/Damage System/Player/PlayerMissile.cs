using UnityEngine;

public class PlayerMissile : MonoBehaviour
{
    public float startSpeed = 5f;
    public float maxSpeed = 15f;
    public float acceleration = 10f;
    public float travelTimeToDestroy = 5f;

    private Transform target;
    private float currentSpeed;
    private bool isLocked = false;
    private bool isDistracted = false;

    [Header("Flare Detection")]
    public LayerMask flareLayer;
    public float flareScanRange = 5f;
    public float flareScanAngle = 60f; // derajat
    public int flareCountToDeactivate = 3;

    public Transform Target => target;

    void Start()
    {
        currentSpeed = startSpeed;
        Destroy(gameObject, travelTimeToDestroy);
    }

    public void SetTarget(Transform lockTarget)
    {
        target = lockTarget;
        isLocked = true;
    }

    void Update()
    {
        if (isDistracted || !isLocked || target == null)
        {
            // Jalan lurus
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
            transform.Translate(Vector2.up * currentSpeed * Time.deltaTime);
            return;
        }

        // Cek gangguan flare
        if (IsDistractedByFlare())
        {
            isLocked = false;
            isDistracted = true;
            target = null;
            return;
        }

        // Gerak homing ke target
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle), 300f * Time.deltaTime);

        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        transform.Translate(Vector2.up * currentSpeed * Time.deltaTime);
    }

    private bool IsDistractedByFlare()
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform == target)
        {
            // Tambahkan ledakan, damage, dll. di sini
            Destroy(gameObject);
        }
    }
}
