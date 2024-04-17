using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField]private Button mainMenuButton;
    [SerializeField]private Button createLobbyButton;
    [SerializeField]private Button quickJoinButton;
    [SerializeField]private Button joinCodeButton;
    [SerializeField]private TMP_InputField joinCodeInputField;
    [SerializeField]private TMP_InputField playerNameInputField;
    [SerializeField]private LobbyCreatedUI lobbyCreatedUI;
    [SerializeField]private Transform lobbyContainer;
    [SerializeField]private Transform lobbyTemplate;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        createLobbyButton.onClick.AddListener(() =>
        {
            lobbyCreatedUI.Show();
        });
        quickJoinButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.QuickJoin();
        });
        joinCodeButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.JoinLobbyByCode(joinCodeInputField.text);
        });
        lobbyTemplate.gameObject.SetActive(false);
    }
    private void Start()
    {
        playerNameInputField.text = KitchenGameMultiplayer.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string inputfield) =>
        {
            KitchenGameMultiplayer.Instance.SetPlayerName(inputfield);
        });
        KitchenGameLobby.Instance.OnLobbyListChanged += KitchenGameLobby_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void KitchenGameLobby_OnLobbyListChanged(object sender, KitchenGameLobby.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }
    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (Lobby lobby in lobbyList)
        {
            Transform transform = Instantiate(lobbyTemplate,lobbyContainer);
            transform.gameObject.SetActive(true);
            transform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }
    private void OnDestroy()
    {
        KitchenGameLobby.Instance.OnLobbyListChanged -= KitchenGameLobby_OnLobbyListChanged;
    }
}
