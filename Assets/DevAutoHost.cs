using UnityEngine;

public class DevAutoHost : MonoBehaviour
{
    private void Start()
    {
#if UNITY_EDITOR
        Invoke(nameof(AutoHost), 0.5f);
#endif
    }

    private void AutoHost()
    {
        MainMenuUI menuUI = FindObjectOfType<MainMenuUI>();
        if (menuUI != null)
        {
            menuUI.OnCreateHostedGameClicked();
        }
        else
        {
            Debug.LogWarning("DevAutoHost: Could not find MainMenuUI in scene.");
        }
    }
}