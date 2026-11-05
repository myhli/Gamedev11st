using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Diperlukan untuk komponen teks TMP

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 7;
    private int currentHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text hpText;

    [Header("Shield Settings")]
    [SerializeField] private int maxShield = 5;
    private int currentShield;
    [SerializeField] private Slider shieldSlider;
    [SerializeField] private TMP_Text shieldText;
    [SerializeField] private float shieldRegenDelay = 4f; // Jeda sebelum shield mulai mengisi ulang
    [SerializeField] private float shieldRegenRate = 0.5f;  // Jeda per 1 poin pemulihan shield

    [Header("Invulnerability (i-Frames)")]
    [SerializeField] private float iFrameDuration = 1f;
    private bool isInvulnerable = false;

    private SpriteRenderer spriteRenderer;
    private Coroutine shieldRegenCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Inisialisasi batas slider
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
        }

        if (shieldSlider != null)
        {
            shieldSlider.minValue = 0;
            shieldSlider.maxValue = maxShield;
        }

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        int sisaDamage = damage;

        // 1. Serap damage dengan Shield terlebih dahulu
        if (currentShield > 0)
        {
            if (currentShield >= sisaDamage)
            {
                currentShield -= sisaDamage;
                sisaDamage = 0; // Shield menahan seluruh damage, nyawa aman
            }
            else
            {
                sisaDamage -= currentShield; // Kurangi sisa damage dengan sisa shield
                currentShield = 0;          // Shield hancur
            }
        }

        // 2. Jika sisa damage masih ada (shield sudah tembus/habis), potong Health
        if (sisaDamage > 0)
        {
            currentHealth -= sisaDamage;
        }

        Debug.Log($"Damage masuk! Sisa Shield: {currentShield}/{maxShield} | Sisa HP: {currentHealth}/{maxHealth}");

        UpdateUI();

        // Reset hitungan jeda regen shield jika sedang diserang
        if (shieldRegenCoroutine != null)
        {
            StopCoroutine(shieldRegenCoroutine);
        }
        shieldRegenCoroutine = StartCoroutine(RegenShieldRoutine());

        // Cek kematian atau aktifkan efek kebal sementara (i-frame)
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageRoutine());
        }
    }

    private void UpdateUI()
    {
        // Perbarui slider dan teks darah
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (hpText != null) hpText.text = currentHealth + " / " + maxHealth;

        // Perbarui slider dan teks perisai
        if (shieldSlider != null) shieldSlider.value = currentShield;
        if (shieldText != null) shieldText.text = currentShield + " / " + maxShield;
    }

    private IEnumerator RegenShieldRoutine()
    {
        // Tunggu beberapa detik tanpa terkena serangan
        yield return new WaitForSeconds(shieldRegenDelay);

        // Isi ulang shield satu per satu hingga penuh
        while (currentShield < maxShield)
        {
            currentShield++;
            UpdateUI();
            yield return new WaitForSeconds(shieldRegenRate);
        }
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

    void Die()
    {
        Debug.Log("Player Kalah / Game Over!");

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Beritahu GameManager bahwa player telah kalah
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerKalah();
        }
        else
        {
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null) gm.PlayerKalah();
        }
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentShield() => currentShield;
    public int GetMaxShield() => maxShield;
}