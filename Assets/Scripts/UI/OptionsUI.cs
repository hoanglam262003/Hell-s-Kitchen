using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { get; private set; }

    [SerializeField]
    private Button soundEffectsButton;
    [SerializeField]
    private Button musicButton;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private Button moveUpButton;
    [SerializeField]
    private Button moveDownButton;
    [SerializeField]
    private Button moveLeftButton;
    [SerializeField]
    private Button moveRightButton;
    [SerializeField]
    private Button interactButton;
    [SerializeField]
    private Button cutButton;
    [SerializeField]
    private Button pauseButton;
    [SerializeField]
    private TextMeshProUGUI soundEffectsText;
    [SerializeField]
    private TextMeshProUGUI musicText;
    [SerializeField]
    private TextMeshProUGUI moveUpText;
    [SerializeField]
    private TextMeshProUGUI moveDownText;
    [SerializeField]
    private TextMeshProUGUI moveLeftText;
    [SerializeField]
    private TextMeshProUGUI moveRightText;
    [SerializeField]
    private TextMeshProUGUI interactText;
    [SerializeField]
    private TextMeshProUGUI cutText;
    [SerializeField]
    private TextMeshProUGUI pauseText;
    [SerializeField]
    private Transform pressToRebindKeyTransform;
    [SerializeField]
    private TextMeshProUGUI duplicateKeyNotification;

    private Action onCloseButtonAction;

    private void Awake()
    {
        Instance = this;
        soundEffectsButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        backButton.onClick.AddListener(() =>
        {
            Hide();
            onCloseButtonAction();
        });
        moveUpButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_Up);
        });
        moveDownButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_Down);
        });
        moveLeftButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_Left);
        });
        moveRightButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_Right);
        });
        interactButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Interact);
        });
        cutButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Cut);
        });
        pauseButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Pause);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;
        UpdateVisual();
        Hide();
        HidePressToRebindKey();
    }

    private void GameManager_OnGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void UpdateVisual()
    {
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
        musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);

        moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
        moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
        interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        cutText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Cut);
        pauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);
    }
    public void Show(Action onCloseButtonAction)
    {
        this.onCloseButtonAction = onCloseButtonAction;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        if (GameInput.Instance != null)
        {
            GameInput.Instance.CancelRebindIfRunning();
        }
    }

    private void ShowPressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(true);
    }
    private void HidePressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(false);
    }
    private void RebindBinding(GameInput.Binding binding)
    {
        duplicateKeyNotification.gameObject.SetActive(false);
        ShowPressToRebindKey();

        GameInput.Instance.RebindBinding(
            binding,
            onSuccess: () =>
            {
                duplicateKeyNotification.gameObject.SetActive(false);
                HidePressToRebindKey();
                UpdateVisual();
            },
            onDuplicate: () =>
            {
                duplicateKeyNotification.gameObject.SetActive(true);
            }
        );
    }

}
