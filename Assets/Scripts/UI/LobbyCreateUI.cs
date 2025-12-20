using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button createPublicLobbyButton;
    [SerializeField] private Button createPrivateLobbyButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
        createPublicLobbyButton.onClick.AddListener(() =>
        {
            string lobbyName = lobbyNameInputField.text;
            KitchenGameLobby.Instance.CreateLobby(lobbyName, false);
        });
        createPrivateLobbyButton.onClick.AddListener(() =>
        {
            string lobbyName = lobbyNameInputField.text;
            KitchenGameLobby.Instance.CreateLobby(lobbyName, true);
        });
    }

    private void Start()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
