using System;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnTryToJoinGame += KitchenGameMultiplayer_OnTryToJoinGame;
        KitchenGameMultiplayer.Instance.OnFailedToJoinGame += KitchenGameMultiplayer_OnFailedToJoinGame;
        Hide();
    }

    private void KitchenGameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Hide();
    }

    private void KitchenGameMultiplayer_OnTryToJoinGame(object sender, EventArgs e)
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

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnTryToJoinGame -= KitchenGameMultiplayer_OnTryToJoinGame;
        KitchenGameMultiplayer.Instance.OnFailedToJoinGame -= KitchenGameMultiplayer_OnFailedToJoinGame;
    }
}
