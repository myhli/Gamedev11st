using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 7;
    [SerializeField] private float iFrameDuration = 1f;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text hpText;

    private int currentHealth;
    private bool isInvulnerable;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
        }

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0) Die();
        else StartCoroutine(DamageRoutine());
    }

    private void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (hpText != null) hpText.text = currentHealth + " / " + maxHealth;
    }

    private IEnumerator DamageRoutine()
    {
        isInvulnerable = true;

        if (spriteRenderer != null)
        {
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 0.5f);
                yield return new WaitForSeconds(iFrameDuration / 6);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(iFrameDuration / 6);
            }
        }

        isInvulnerable = false;
    }

    private void Die()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (GameManager.Instance != null) GameManager.Instance.PlayerKalah();
    }
}
