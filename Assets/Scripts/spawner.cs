using UnityEngine;

public class spawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public GameObject bombPrefab;
    public float spawnInterval = 0.8f;
    public float xRange = 8f;
    public float spawnY = 6f;
    [Range(0f, 1f)] public float bombChance = 0.25f;

    float timer;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            GameObject prefab = Random.value < bombChance ? bombPrefab : coinPrefab;
            Vector3 pos = new Vector3(Random.Range(-xRange, xRange), spawnY, 0f);
            Instantiate(prefab, pos, Quaternion.identity);
        }
    }
}