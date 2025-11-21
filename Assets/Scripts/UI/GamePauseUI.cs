using System;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField]
    private Button continueButton;
    [SerializeField]
    private Button mainMenuButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(() =>
        {
            GameManager.Instance.TogglePauseGame();
        });
        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.LoadScene(Loader.Scene.MainMenuScene);
        });
    }
    private void Start()
    {
        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;
        Hide();
    }

    private void GameManager_OnGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGamePaused(object sender, System.EventArgs e)
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
