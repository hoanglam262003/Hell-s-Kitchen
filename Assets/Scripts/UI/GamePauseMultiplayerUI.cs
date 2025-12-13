using System;
using UnityEngine;

public class GamePauseMultiplayerUI : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnMultiplayerGamePaused += GameManager_OnMultiplayerGamePaused;
        GameManager.Instance.OnMultiplayerGameUnpaused += GameManager_OnMultiplayerGameUnpaused;
        Hide();
    }

    private void GameManager_OnMultiplayerGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnMultiplayerGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
