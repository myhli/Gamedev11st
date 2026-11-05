using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " terkena damage! Sisa HP: " + currentHealth);

        // Efek visual singkat (opsional: kedip merah saat kena hit)
        StartCoroutine(BlinkRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator BlinkRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " Mati!");

        // Beritahu GameManager bahwa musuh ini kalah
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MusuhKalah(this);
        }
        else
        {
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null) gm.MusuhKalah(this);
        }

        Destroy(gameObject);
    }
}