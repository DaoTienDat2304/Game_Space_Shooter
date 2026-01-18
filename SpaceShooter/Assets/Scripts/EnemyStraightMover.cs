using UnityEngine;

/// <summary>
/// Script di chuyển Enemy thẳng xuống (đơn giản)
/// </summary>
public class EnemyStraightMover : MonoBehaviour
{
    public float speed = 3f;

    private void Update()
    {
        // Di chuyển thẳng xuống
        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }
}
