using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "MultiplayerName";
    public const int MAX_PLAYER_AMOUNT = 4;
    public static KitchenGameMultiplayer Instance { get; private set; }

    public static bool playMultiplayer = true;
    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] private List<Color> playerColorList;

    private NetworkList<PlayerData> playDataNetworkList;
    private string playerName;

    public event EventHandler OnFailedJoinGame;
    public event EventHandler OnTtyingToJoinGame;
    public event EventHandler OnPlayDataNetworkListChanged;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));

        playDataNetworkList = new NetworkList<PlayerData>();
        playDataNetworkList.OnListChanged += PlayDataNetworkList_OnListChanged;
    }
    private void Start()
    {
        if(!playMultiplayer)
        {
            //单人模式
            StartHost();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }
    public string GetPlayerName()
    {
        return playerName;
    }
    
    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }

    private void PlayDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0;i< playDataNetworkList.Count; i++)
        {
            PlayerData playerData = playDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                playDataNetworkList.RemoveAt(i);
                break;
            }
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playDataNetworkList.Add(new PlayerData { 
            clientId = clientId,
            colorId = GetFirstUnusedColorId(),
            
        });
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            response.Approved = false;
            response.Reason = "Game has already started";
            return;
        }
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            response.Approved = false;
            response.Reason = "Game is full";
            return;
        }
        response.Approved = true;
    }

    public void StartClient()
    {
        OnTtyingToJoinGame?.Invoke(this, EventArgs.Empty);
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playDataNetworkList[playerIndex];
        playerData.playerName = playerName;
        playDataNetworkList[playerIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playDataNetworkList[playerIndex];
        playerData.playerId = playerId;
        playDataNetworkList[playerIndex] = playerData;
    }



    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {

        SpawnKitchenObjectServerRpc(GetKitchenObjectListSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentReference)
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectListSOFromIndex(kitchenObjectSOIndex);

        kitchenObjectParentReference.TryGet(out NetworkObject kitchenObjectParentnetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentnetworkObject.GetComponent<IKitchenObjectParent>();
        if (kitchenObjectParent.HasKitchenObject())
        {
            //父类已经有对象
            return;
        }
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);

        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }

    public int GetKitchenObjectListSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }
    public KitchenObjectSO GetKitchenObjectListSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }

    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }
    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectReference)
    {
        kitchenObjectReference.TryGet(out NetworkObject kitchenObjectnetworkObject);
        if(kitchenObjectnetworkObject == null)
        {
            //已经被销毁
            return;
        }
        KitchenObject kitchenObject = kitchenObjectnetworkObject.GetComponent<KitchenObject>();
        ClearKitchenObjectOnParentClientRpc(kitchenObjectReference);

        kitchenObject.DestroySelf();
    }
    [ClientRpc]
    private void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference kitchenObjectReference)
    {
        kitchenObjectReference.TryGet(out NetworkObject kitchenObjectnetworkObject);
        KitchenObject kitchenObject = kitchenObjectnetworkObject.GetComponent<KitchenObject>();

        kitchenObject.ClearKitchenObjectOnParent();
    }
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playDataNetworkList.Count;
    }
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playDataNetworkList.Count; i++)
        {
            if (playDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playDataNetworkList[playerIndex];
    }

    public Color GetPlayerColor(int colorId)
    {
        return playerColorList[colorId];
    }

    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(colorId))
        {
            //不存在可使用颜色
            return;
        }
        int playerIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playDataNetworkList[playerIndex];
        playerData.colorId = colorId;
        playDataNetworkList[playerIndex] = playerData;
    }

    public bool IsColorAvailable(int colorId)
    {
        foreach(PlayerData playerData in playDataNetworkList)
        {
            if(playerData.colorId == colorId)
            {
                return false;
            }
        }
        return true;
    }

    private int GetFirstUnusedColorId()
    {
        for (int i = 0;i<playerColorList.Count;i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }
    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }
}
