using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private int contactDamage = 1;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Cari objek Player di scene secara otomatis
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        // Hitung arah menuju Player
        Vector2 arah = (playerTransform.position - transform.position).normalized;

        // Gerakkan Orc mendekati Player
        rb.linearVelocity = arah * speed;

        // Balik hadap badan Orc (Flip X) sesuai posisi Player
        if (arah.x > 0.05f)
        {
            spriteRenderer.flipX = false;
        }
        else if (arah.x < -0.05f)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Player"))
    {
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }
    }
}
private void OnCollisionEnter2D(Collision2D collision)
{
    BeriDamageKePlayer(collision.gameObject);
}

// Tambahkan ini agar saat Orc terus menempel pada Player, damage tetap masuk berkala

private void BeriDamageKePlayer(GameObject target)
{
    if (target.CompareTag("Player"))
    {
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }
    }
}
}