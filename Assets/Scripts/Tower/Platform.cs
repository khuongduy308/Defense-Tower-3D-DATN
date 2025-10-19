using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Platform : MonoBehaviour
{
    public static event System.Action<Platform> OnPlatformClicked;
    [SerializeField] private LayerMask platformlayerMask;

    void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D raycastHit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, platformlayerMask);

            if(raycastHit.collider != null)
            {
                Platform platform = raycastHit.collider.GetComponent<Platform>();
                if(platform != null)
                {
                    // FindObjectOfType<UIController>().ShowTowerPanel();
                    // Debug.Log("Platform clicked");
                    OnPlatformClicked?.Invoke(platform);
                }
            }
        }
    }
}
