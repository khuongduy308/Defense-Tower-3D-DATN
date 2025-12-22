using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [Header("Kéo file nhạc của màn này vào đây")]
    public AudioClip musicForThisScene;

    private void Start()
    {
        // Gọi ông trùm AudioManager bật bài này lên
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(musicForThisScene);
        }
    }
}