using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Musuh & Kondisi Menang/Kalah")]
    public int totalMusuh;
    private int musuhTerkalahkan = 0;
    public bool isGameOver { get; private set; }
    public bool isGameWon { get; private set; }

    [Header("Koin Settings")]
    public int totalKoin;
    private int koinTerkumpul = 0;
    public ItemDetails coinData;

    [Header("UI References")]
    public TextMeshProUGUI teksSkor;
    public GameObject teksMenang;
    public GameObject panelMenang;
    public GameObject teksKalah;
    public GameObject panelKalah;
    public TextMeshProUGUI teksStatus;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Hitung total koin di scene
        totalKoin = GameObject.FindGameObjectsWithTag("Coin").Length;

        // Hitung total musuh di scene
        HitungTotalMusuh();

        // Sembunyikan panel/teks UI menang dan kalah di awal
        if (teksMenang != null) teksMenang.SetActive(false);
        if (panelMenang != null) panelMenang.SetActive(false);
        if (teksKalah != null) teksKalah.SetActive(false);
        if (panelKalah != null) panelKalah.SetActive(false);
        if (teksStatus != null) teksStatus.gameObject.SetActive(false);

        UpdateSkorUI();
    }

    void Update()
    {
        // Shortcut [R] untuk restart permainan saat game over / menang
        if (isGameOver)
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                RestartGame();
            }
        }
    }

    public void HitungTotalMusuh()
    {
        EnemyHealth[] enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude);
        totalMusuh = enemies.Length;
        Debug.Log($"[GameManager] Total musuh aktif di scene: {totalMusuh}");
    }

    // Dipanggil saat Player mati di PlayerHealth.cs
    public void PlayerKalah()
    {
        if (isGameOver || isGameWon) return;

        isGameOver = true;
        Debug.Log("💀 [GameManager] PLAYER KALAH! Game Over.");

        if (panelKalah != null)
        {
            panelKalah.SetActive(true);
        }
        else if (teksKalah != null)
        {
            teksKalah.SetActive(true);
        }

        if (teksStatus != null)
        {
            teksStatus.text = "GAME OVER!\nKamu Telah Gugur\n(Tekan [R] untuk Coba Lagi)";
            teksStatus.color = Color.red;
            teksStatus.gameObject.SetActive(true);
        }
    }

    // Dipanggil saat musuh mati di EnemyHealth.cs
    public void MusuhKalah(EnemyHealth musuh)
    {
        if (isGameOver && !isGameWon) return;

        musuhTerkalahkan++;
        Debug.Log($"[GameManager] Musuh terkalahkan: {musuhTerkalahkan}/{totalMusuh}");

        // Cek sisa musuh yang masih aktif di scene selain yang sedang proses hancur
        EnemyHealth[] remaining = Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude);
        int activeCount = 0;
        foreach (var e in remaining)
        {
            if (e != null && e != musuh && e.gameObject.activeInHierarchy)
            {
                activeCount++;
            }
        }

        Debug.Log($"[GameManager] Sisa musuh hidup: {activeCount}");

        if (activeCount <= 0 || (totalMusuh > 0 && musuhTerkalahkan >= totalMusuh))
        {
            Menang();
        }
    }

    public void Menang()
    {
        if (isGameOver && !isGameWon) return;

        isGameWon = true;
        isGameOver = true;
        Debug.Log("🏆 [GameManager] KAMU MENANG! Seluruh musuh telah dikalahkan!");

        if (panelMenang != null)
        {
            panelMenang.SetActive(true);
        }
        else if (teksMenang != null)
        {
            teksMenang.SetActive(true);
        }

        if (teksStatus != null)
        {
            teksStatus.text = "VICTORY!\nSeluruh Musuh Telah Dikalahkan!\n(Tekan [R] untuk Main Lagi)";
            teksStatus.color = new Color(1f, 0.85f, 0.2f);
            teksStatus.gameObject.SetActive(true);
        }
    }

    public void AmbilKoin(Sprite icon = null)
    {
        koinTerkumpul++;
        UpdateSkorUI();

        if (InventoryManager.Instance != null && coinData != null)
        {
            InventoryManager.Instance.AddItem(coinData);
        }
    }

    void UpdateSkorUI()
    {
        if (teksSkor != null)
        {
            teksSkor.text = "Skor: " + koinTerkumpul;
        }
    }

    private void OnGUI()
    {
        if (!isGameOver) return;

        // Jika tidak ada panel UI kustom yang terpasang di Inspector, tampilkan banner overlay GUI otomatis
        if (panelMenang == null && teksMenang == null && panelKalah == null && teksKalah == null && teksStatus == null)
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.fontSize = 22;
            boxStyle.alignment = TextAnchor.MiddleCenter;
            boxStyle.fontStyle = FontStyle.Bold;

            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.fontSize = 15;
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.normal.textColor = Color.white;

            float boxWidth = 440;
            float boxHeight = 170;
            float x = (Screen.width - boxWidth) / 2;
            float y = (Screen.height - boxHeight) / 2;

            if (isGameWon)
            {
                GUI.color = new Color(0.2f, 0.9f, 0.35f, 0.95f);
                GUI.Box(new Rect(x, y, boxWidth, boxHeight), "🏆 VICTORY!\nSeluruh Musuh Telah Dikalahkan!", boxStyle);
            }
            else
            {
                GUI.color = new Color(0.95f, 0.2f, 0.2f, 0.95f);
                GUI.Box(new Rect(x, y, boxWidth, boxHeight), "💀 GAME OVER!\nPlayer Telah Gugur!", boxStyle);
            }

            GUI.color = Color.white;
            GUI.Label(new Rect(x, y + 85, boxWidth, 25), "Tekan [R] pada keyboard untuk Coba Lagi", subStyle);

            if (GUI.Button(new Rect(x + 130, y + 115, 180, 35), "🔄 Main Lagi (Restart)"))
            {
                RestartGame();
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
