using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; 

public class LoadingCallback : MonoBehaviour
{
    [Header("UI References")]
    public Slider progressBar;
    public TMP_Text progressText;
    
    [Header("Settings")]
    public float minLoadTime = 2f; // Thoi gian cho bat buoc

    private IEnumerator Start()
    {
        string sceneToLoad = Loader.GetTargetScene();
        
        if (string.IsNullOrEmpty(sceneToLoad)) yield break;

        yield return null; 

        StartCoroutine(LoadAsync(sceneToLoad));
    }

    IEnumerator LoadAsync(string sceneName)
    {
        // Bắt đầu load ngầm (Async)
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        // Ngăn không cho Unity tự chuyển cảnh khi xong (để mình tự control thanh loading)
        operation.allowSceneActivation = false;

        float timer = 0f;

        // Vòng lặp chạy cho đến khi load xong
        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            // operation.progress chỉ chạy từ 0 đến 0.9 (0.9 là xong phần data)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Logic giả vờ load (để thanh chạy mượt hơn, không bị giật cái vèo phát 100%)
            // Nếu load thật nhanh quá thì vẫn đợi đủ minLoadTime
            float fakeProgress = Mathf.Clamp01(timer / minLoadTime);
            
            // Lấy giá trị thấp hơn giữa Thực tế và Giả vờ (để đảm bảo không xong quá sớm)
            float displayedProgress = Mathf.Min(progress, fakeProgress);

            // Cập nhật UI
            if (progressBar != null) progressBar.value = displayedProgress;
            if (progressText != null) progressText.text = $"Loading... {Mathf.RoundToInt(displayedProgress * 100)}%";

            // Điều kiện hoàn thành:
            // 1. Load thật đã xong (progress >= 0.9)
            // 2. Thời gian chờ tối thiểu đã hết
            if (operation.progress >= 0.9f && timer >= minLoadTime)
            {
                // Cho phép chuyển cảnh
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}