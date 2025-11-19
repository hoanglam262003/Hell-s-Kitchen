using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour
{
    [SerializeField]
    private Image timerImage;

    private void Update()
    {
        float normalizedTime = GameManager.Instance.GetGamePlayingTimerNormalized();
        timerImage.fillAmount = normalizedTime;
    }
}
