using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnRadius = 8f;
    [SerializeField] private int maxEnemies = 8;

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (enemyPrefab == null)
        {
            GameObject existingEnemy = GameObject.FindGameObjectWithTag("Enemy");
            if (existingEnemy != null)
            {
                enemyPrefab = existingEnemy;
            }
        }

        InvokeRepeating(nameof(SpawnEnemy), 2f, spawnInterval);
    }

    void SpawnEnemy()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            else return;
        }

        if (enemyPrefab == null)
        {
            GameObject existingEnemy = GameObject.FindGameObjectWithTag("Enemy");
            if (existingEnemy != null)
            {
                enemyPrefab = existingEnemy;
            }
            else return;
        }

        int currentCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (currentCount >= maxEnemies) return;

        Vector2 randomOffset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = player.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        newEnemy.name = "KamikazeBot";
        newEnemy.SetActive(true);
    }
}
