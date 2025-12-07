using UnityEngine;

public class AutoDisableEffect : MonoBehaviour
{
    [Header("Cấu hình thời gian")]
    [Tooltip("Thời gian Animation nổ chạy")]
    [SerializeField] private float explosionTime = 1f; 

    [Tooltip("Thời gian giữ lại xác để ngắm (ví dụ 3s)")]
    [SerializeField] private float lingerTime = 2.0f; 

    private void OnEnable()
    {
        // 1. Chỉ tắt Collider sau khi nổ xong (để không chặn click chuột của người chơi)
        Invoke(nameof(DisableCollider), explosionTime);

        // 2. Tắt hoàn toàn vật thể sau một khoảng thời gian dài (để dọn rác bộ nhớ)
        Invoke(nameof(DisableMe), explosionTime + lingerTime);
    }

    private void DisableCollider()
    {
        // Tìm tất cả Collider 2D trên object này và tắt đi
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    private void DisableMe()
    {
        Destroy(gameObject);
    }
}
