using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;           // Tên để gọi (VD: "Shoot", "Explosion")
    public AudioClip clip;        // File âm thanh
    
    [Range(0f, 1f)]
    public float volume = 1f;     // Âm lượng riêng cho từng file
    
    [Range(0.1f, 3f)]
    public float pitch = 1f;      // Độ cao (1 là bình thường)

    [HideInInspector]
    public AudioSource source;    // Nguồn phát (tự động gán)
}
