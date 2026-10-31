using UnityEngine;

public class coinDrop : MonoBehaviour
{
    public float fallSpeed = 4f;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        if (transform.position.y < -6f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<catcher>() != null)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(1);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCoin();

            Destroy(gameObject);
        }
    }
}