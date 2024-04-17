using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameManager : NetworkBehaviour
{


    public static KitchenGameManager Instance { get; private set; }



    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnMultiPlayerPauseChanged;
    public event EventHandler OnMultiPlayerUnPauseChanged;


    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }


    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private bool isLocalPlayerReady = false;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 90f;
    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePause = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> playReadyDictionary;
    private Dictionary<ulong, bool> playPauseDictionary;
    private bool autoTestGamePauseState = false;
    [SerializeField] private Transform playerPrefab;


    private void Awake()
    {
        Instance = this;

        playReadyDictionary = new Dictionary<ulong, bool>();
        playPauseDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }
    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        isGamePause.OnValueChanged += IsGamePause_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = GameObject.Instantiate(playerPrefab);
            playerTransform.transform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
            SetPlayerReadyServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playReadyDictionary.ContainsKey(clientId) || !playReadyDictionary[clientId])
            {
                allReady = false;
                break;
            }
        }
        if (allReady)
        {
            state.Value = State.CountdownToStart;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientid)
    {
        autoTestGamePauseState = true;
    }

    private void IsGamePause_OnValueChanged(bool previousValue, bool newValue)
    {
        if (isGamePause.Value)
        {
            Time.timeScale = 0f;
            OnMultiPlayerPauseChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiPlayerUnPauseChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        if (!IsServer) { return; }
        switch (state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value < 0f)
                {
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value < 0f)
                {
                    state.Value = State.GameOver;
                }
                break;
            case State.GameOver:
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

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }

    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer.Value;
    }

    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (gamePlayingTimer.Value / gamePlayingTimerMax);
    }

    public void TogglePauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused)
        {
            PauseGameServerRpc();
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            UnPauseGameServerRpc();
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playPauseDictionary[serverRpcParams.Receive.SenderClientId] = true;
        TestGamePauseState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnPauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playPauseDictionary[serverRpcParams.Receive.SenderClientId] = false;
        TestGamePauseState();
    }
    private void TestGamePauseState()
    {
        foreach (ulong clientid in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playPauseDictionary.ContainsKey(clientid) && playPauseDictionary[clientid])
            {
                isGamePause.Value = true;
                break;
            }
        }
        isGamePause.Value = false;
    }
}