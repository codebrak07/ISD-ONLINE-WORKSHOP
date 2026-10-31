using UnityEngine;

public class catcher : MonoBehaviour
{
    public float speed = 10f;
    public float limit = 8f;

    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        Vector3 pos = transform.position;
        pos.x += move * speed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -limit, limit);
        transform.position = pos;
    }
}