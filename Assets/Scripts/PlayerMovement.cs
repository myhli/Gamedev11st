using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float kecepatan = 5f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;

    private Vector2 arahGerak;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        arahGerak = BacaInput();
        if (anim != null) anim.SetFloat("Speed", arahGerak.sqrMagnitude);

        if (arahGerak.x > 0.01f) Hadapkan(1f);
        else if (arahGerak.x < -0.01f) Hadapkan(-1f);

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) MulaiSerang();
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) MulaiSerang();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = arahGerak * kecepatan;
    }

    private Vector2 BacaInput()
    {
        if (Keyboard.current == null) return Vector2.zero;

        Vector2 raw = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) raw.y += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) raw.y -= 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) raw.x -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) raw.x += 1;

        return raw.sqrMagnitude > 0.01f ? raw.normalized : Vector2.zero;
    }

    private void Hadapkan(float arahX)
    {
        Vector3 skala = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(skala.x) * arahX, skala.y, skala.z);
    }

    void MulaiSerang()
    {
        if (anim != null) anim.SetTrigger("Attack");
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable target = enemy.GetComponent<IDamageable>();
            if (target != null) target.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
