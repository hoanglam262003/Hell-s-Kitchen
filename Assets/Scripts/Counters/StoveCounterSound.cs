using System;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;

    private AudioSource audioSource;
    private float warningSoundTimer;
    private bool playWarningSound;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
        stoveCounter.OnProgressChanged += StoveCounter_OnStateChanged;
        SoundManager.Instance.OnVolumeChanged += SoundManager_OnVolumeChanged;
        audioSource.volume = SoundManager.Instance.GetVolume();
    }

    private void StoveCounter_OnStateChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = 0.5f;
        playWarningSound = e.progressNormalized >= burnShowProgressAmount && stoveCounter.IsFried();
    }

    private void OnDestroy()
    {
        SoundManager.Instance.OnVolumeChanged -= SoundManager_OnVolumeChanged;
    }

    private void SoundManager_OnVolumeChanged(object sender, EventArgs e)
    {
        audioSource.volume = SoundManager.Instance.GetVolume();
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.StateChangedEventArgs e)
    {
        bool playSound = e.state == StoveCounter.State.Frying || e.state == StoveCounter.State.Fried;

        if (playSound)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    private void Update()
    {
        if (playWarningSound)
        {
            warningSoundTimer -= Time.deltaTime;
            if (warningSoundTimer <= 0f)
            {
                float warningSoundTimerMax = 0.2f;
                warningSoundTimer = warningSoundTimerMax;
                SoundManager.Instance.PlayWarningSound(stoveCounter.transform.position);
            }
        }
    }
}
