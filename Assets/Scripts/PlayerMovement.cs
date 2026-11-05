using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float kecepatan = 5f;
    private Vector2 arahGerak;
    public int skor = 0;

    [Header("Combat Settings")]
    [SerializeField] private Transform attackPoint; // Titik ujung pedang
    [SerializeField] private float attackRange = 0.5f; // Jarak jangkauan tebasan
    [SerializeField] private LayerMask enemyLayers; // Layer khusus musuh
    [SerializeField] private int attackDamage = 1;

    // Referensi Komponen
    private Rigidbody2D rb; // <-- Variabel ini yang sebelumnya belum ada
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMove(InputValue value)
    {
        arahGerak = value.Get<Vector2>();
    }

    void OnAttack()
    {
        MulaiSerang();
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            Vector2 rawInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) rawInput.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) rawInput.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) rawInput.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) rawInput.x += 1;
            if (rawInput != Vector2.zero) arahGerak = rawInput.normalized;
        }

        // Parameter Animator
        if (anim != null)
        {
            anim.SetFloat("Speed", arahGerak.sqrMagnitude);
        }

        // Balik hadap karakter (kiri / kanan)
        if (arahGerak.x > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (arahGerak.x < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // Cadangan input langsung (Spasi atau Klik Kiri)
        if ((Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
        {
            MulaiSerang();
        }
    }

    void FixedUpdate()
    {
        // Pergerakan murni fisika: otomatis mengatasi tabrakan liar & player mundur sendiri
        rb.linearVelocity = arahGerak.normalized * kecepatan;
    }

    void MulaiSerang()
    {
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // Deteksi musuh di depan Player
        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            SpriteRenderer coinSprite = other.GetComponent<SpriteRenderer>();
            Sprite icon = (coinSprite != null) ? coinSprite.sprite : null;

            Destroy(other.gameObject);
            
            skor++;
            Debug.Log("Skor Kamu Saat Ini: " + skor);

            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.AmbilKoin(icon);
            }
        }
    }
}