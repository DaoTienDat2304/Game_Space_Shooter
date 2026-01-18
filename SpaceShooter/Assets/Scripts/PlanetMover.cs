using UnityEngine;

/// <summary>
/// Script di chuyển Planet thẳng xuống
/// </summary>
public class PlanetMover : MonoBehaviour
{
    public float speed = 3f;

    private void Update()
    {
        // Di chuyển thẳng xuống
        transform.Translate(Vector2.down * speed * Time.deltaTime);

        // Nếu ra khỏi màn hình dưới, hủy
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }
}
