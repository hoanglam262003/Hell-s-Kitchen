using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;

    [SerializeField] private Transform playerPrefab;
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    private NetworkVariable<State> networkState = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<float> networkCountdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> networkGamePlayingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<bool> networkGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPauseDictionary;
    private bool autoTestGamePauseState;

    private State singleState;
    private float singleCountdownTimer;
    private float singleGameTimer;
    private bool singleGamePaused;

    private bool isLocalPlayerReady;
    private float gamePlayingTimerMax = 300f;
    private bool localGamePaused = false;
    public bool IsMultiplayer => KitchenGameMultiplayer.playMultiplayer;

    private State CurrentState
    {
        get => IsMultiplayer ? networkState.Value : singleState;
        set
        {
            if (IsMultiplayer)
                networkState.Value = value;
            else
                singleState = value;

            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private float CountdownTimer
    {
        get => IsMultiplayer ? networkCountdownToStartTimer.Value : singleCountdownTimer;
        set
        {
            if (IsMultiplayer)
                networkCountdownToStartTimer.Value = value;
            else
                singleCountdownTimer = value;
        }
    }

    private float GameTimer
    {
        get => IsMultiplayer ? networkGamePlayingTimer.Value : singleGameTimer;
        set
        {
            if (IsMultiplayer)
                networkGamePlayingTimer.Value = value;
            else
                singleGameTimer = value;
        }
    }

    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPauseDictionary = new Dictionary<ulong, bool>();

        singleState = State.WaitingToStart;
        singleCountdownTimer = 3f;
        singleGameTimer = gamePlayingTimerMax;
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        if (!IsMultiplayer)
        {
            Instantiate(playerPrefab);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsMultiplayer) return;
        networkState.OnValueChanged += State_OnValueChanged;
        networkGamePaused.OnValueChanged += GamePaused_OnValueChanged;
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        autoTestGamePauseState = true;
    }

    private void GamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if (networkGamePaused.Value)
        {
            Time.timeScale = 0f;
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (CurrentState == State.WaitingToStart)
        {
            isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
            if (IsMultiplayer)
            {
                if (NetworkManager.Singleton != null &&
                    NetworkManager.Singleton.IsListening)
                {
                    SetPlayerReadyServerRpc();
                }
            }
            else
            {
                CurrentState = State.CountdownToStart;
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetPlayerReadyServerRpc(RpcParams rpcParams = default)
    {
        playerReadyDictionary[rpcParams.Receive.SenderClientId] = true;

        bool allReady = true;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            networkState.Value = State.CountdownToStart;
        }

        Debug.Log($"All players ready: {allReady}");
    }

    private void Update()
    {
        if (IsMultiplayer && !IsServer) return;

        switch (CurrentState)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                CountdownTimer -= Time.deltaTime;
                if (CountdownTimer < 0f)
                {
                    CurrentState = State.GamePlaying;
                    GameTimer = gamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                GameTimer -= Time.deltaTime;
                if (GameTimer < 0f)
                {
                    CurrentState = State.GameOver;
                }
                break;
            case State.GameOver:
                Time.timeScale = 0f;
                break;
        }
    }

    private void LateUpdate()
    {
        if (autoTestGamePauseState)
        {
            autoTestGamePauseState = false;
            TestGamePauseState();
        }
    }

    public bool IsGamePlaying()
    {
        return CurrentState == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return CurrentState == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return CountdownTimer;
    }

    public bool IsGameOver()
    {
        return CurrentState == State.GameOver;
    }

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (GameTimer / gamePlayingTimerMax);
    }

    public bool IsWaitingToStart()
    {
        return CurrentState == State.WaitingToStart;
    }

    public void TogglePauseGame()
    {
        localGamePaused = !localGamePaused;
        if (IsMultiplayer)
        {
            if (localGamePaused)
                PauseGameServerRpc();
            else
                UnpauseGameServerRpc();
        }
        else
        {
            singleGamePaused = localGamePaused;
            Time.timeScale = singleGamePaused ? 0f : 1f;
        }

        if (localGamePaused)
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        else
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
    }
    public void ResetState()
    {
        CurrentState = State.WaitingToStart;
        CountdownTimer = 3f;
        GameTimer = gamePlayingTimerMax;
        Time.timeScale = 1f;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PauseGameServerRpc(RpcParams rpcParams = default)
    {
        playerPauseDictionary[rpcParams.Receive.SenderClientId] = true;
        TestGamePauseState();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void UnpauseGameServerRpc(RpcParams rpcParams = default)
    {
        playerPauseDictionary[rpcParams.Receive.SenderClientId] = false;
        TestGamePauseState();
    }

    private void TestGamePauseState()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPauseDictionary.ContainsKey(clientId) && playerPauseDictionary[clientId])
            {
                networkGamePaused.Value = true;
                return;
            }
        }
        networkGamePaused.Value = false;
    }
}
