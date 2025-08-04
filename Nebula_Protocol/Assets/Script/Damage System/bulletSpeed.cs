using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float bulletSpeed = 10f;     // Kecepatan peluru
    public float lifetime = 3f;         // Waktu hidup peluru (detik)

    private void Start()
    {
        Destroy(gameObject, lifetime); // Hancurkan peluru setelah waktu tertentu
    }

    private void Update()
    {
        // Gerakkan peluru terus ke arah depan (local up)
        transform.Translate(Vector3.up * bulletSpeed * Time.deltaTime);
    }
}
