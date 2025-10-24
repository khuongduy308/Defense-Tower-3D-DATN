using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Platform : MonoBehaviour
{
    public static event System.Action<Platform> OnPlatformClicked;
    [SerializeField] private LayerMask platformlayerMask;
    public static bool towerPanelOpen  { get; set; } = false;

    void Update()
    {
        if (towerPanelOpen || Time.timeScale == 0f) return;
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D raycastHit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, platformlayerMask);

            if (raycastHit.collider != null)
            {
                Platform platform = raycastHit.collider.GetComponent<Platform>();
                if (platform != null)
                {
                    // FindObjectOfType<UIController>().ShowTowerPanel();
                    // Debug.Log("Platform clicked");
                    OnPlatformClicked?.Invoke(platform);
                }
            }
        }
    }
    
    public void PlaceTower(TowerData data)
    {
        Instantiate(data.prefab, transform.position, Quaternion.identity, transform);
    }
}
