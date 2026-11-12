using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isGameOver { get; private set; }

    [Header("Musuh Settings")]
    [SerializeField] private GameObject kamikazeBotPrefab;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxEnemies = 8;
    [SerializeField] private float spawnMargin = 1.5f;

    private Camera mainCam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        mainCam = Camera.main;

        if (kamikazeBotPrefab != null) InvokeRepeating(nameof(SpawnEnemy), 2f, spawnInterval);
    }

    void Update()
    {
        if (isGameOver && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    void SpawnEnemy()
    {
        if (kamikazeBotPrefab == null || isGameOver) return;
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies) return;

        Instantiate(kamikazeBotPrefab, GetSpawnOutsideScreen(), Quaternion.identity);
    }

    private Vector3 GetSpawnOutsideScreen()
    {
        if (mainCam == null) mainCam = Camera.main;

        float h = mainCam.orthographicSize;
        float w = h * mainCam.aspect;
        Vector3 c = mainCam.transform.position;

        switch (Random.Range(0, 4))
        {
            case 0: return new Vector3(Random.Range(c.x - w, c.x + w), c.y + h + spawnMargin, 0f);
            case 1: return new Vector3(Random.Range(c.x - w, c.x + w), c.y - h - spawnMargin, 0f);
            case 2: return new Vector3(c.x - w - spawnMargin, Random.Range(c.y - h, c.y + h), 0f);
            default: return new Vector3(c.x + w + spawnMargin, Random.Range(c.y - h, c.y + h), 0f);
        }
    }

    public void PlayerKalah()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
