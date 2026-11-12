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

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        Vector2 arah = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = arah * speed;

        if (arah.x > 0.05f) spriteRenderer.flipX = false;
        else if (arah.x < -0.05f) spriteRenderer.flipX = true;
    }

    private void OnTriggerEnter2D(Collider2D collision) => BeriDamage(collision.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => BeriDamage(collision.gameObject);

    private void BeriDamage(GameObject target)
    {
        if (!target.CompareTag("Player")) return;
        PlayerHealth ph = target.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(contactDamage);
            Destroy(gameObject);
        }
    }
}
