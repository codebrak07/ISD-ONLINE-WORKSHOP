using UnityEngine;

public class bomb : MonoBehaviour
{
    public float fallSpeed = 4f;
    public int scorePenalty = 3;

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
                GameManager.Instance.AddScore(-scorePenalty);
            Destroy(gameObject);
        }
    }
}