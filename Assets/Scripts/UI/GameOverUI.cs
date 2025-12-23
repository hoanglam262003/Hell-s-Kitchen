using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI recipesDeliveredText;
    [SerializeField]
    private Button mainMenuButton;
    [SerializeField]
    private Button playAgainButton;
    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            GameInput.Instance.enabled = false;
            if (!KitchenGameMultiplayer.playMultiplayer)
            {
                Loader.LoadScene(Loader.Scene.MainMenuScene);
                return;
            }
            NetworkManager.Singleton.Shutdown();
            Loader.LoadScene(Loader.Scene.MainMenuScene);
        });
        playAgainButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ResetState();
            GameInput.Instance.enabled = false;
            if (!KitchenGameMultiplayer.playMultiplayer)
            {
                Loader.LoadScene(Loader.Scene.MainMenuScene);
                return;
            }
            NetworkManager.Singleton.Shutdown();
            Loader.LoadScene(Loader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGameOver())
        {
            GameInput.Instance.enabled = false;
            Show();
            recipesDeliveredText.text = DeliveryManager.Instance.GetSuccessfulRecipesDelivered().ToString();
        }
        else
        {
            Hide();
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}
