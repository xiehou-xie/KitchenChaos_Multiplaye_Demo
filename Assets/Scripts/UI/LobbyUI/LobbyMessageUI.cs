using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private Button closeButton;
    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }
    // Start is called before the first frame update
    void Start()
    {
        KitchenGameMultiplayer.Instance.OnFailedJoinGame += KitchenGameMultiplayer_OnFailedJoinGame;
        KitchenGameLobby.Instance.OnCreateLobbyFaild += KitchenGameLobby_OnCreateLobbyFaild;
        KitchenGameLobby.Instance.OnCreateLobbyStarted += KitchenGameLobby_OnCreateLobbyStarted;
        KitchenGameLobby.Instance.OnJoinStarted += KitchenGameLobby_OnJoinStarted;
        KitchenGameLobby.Instance.OnJoinFailed += KitchenGameLobby_OnJoinFaild;
        KitchenGameLobby.Instance.OnQuickJoinFaild += KitchenGameLobby_OnQuickJoinFaild;

        Hide();
    }

    private void KitchenGameLobby_OnCreateLobbyFaild(object sender, System.EventArgs e)
    {
        ShowMessage("Faild to create Lobby!");
    }

    private void KitchenGameLobby_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Creating Lobby...");
    }

    private void KitchenGameMultiplayer_OnFailedJoinGame(object sender, System.EventArgs e)
    {
        if(string.IsNullOrEmpty(NetworkManager.Singleton.DisconnectReason))
        {
            ShowMessage("Failed to connect");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void KitchenGameLobby_OnJoinStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Joining lobby ...");
    }

    private void KitchenGameLobby_OnJoinFaild(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to join Lobby");
    }

    private void KitchenGameLobby_OnQuickJoinFaild(object sender, System.EventArgs e)
    {
        ShowMessage("Could not find a Lobby to Quick Join");
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void ShowMessage(string message)
    {
        Show();

        textMeshProUGUI.text = message;
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnFailedJoinGame -= KitchenGameMultiplayer_OnFailedJoinGame;
        KitchenGameLobby.Instance.OnCreateLobbyFaild -= KitchenGameLobby_OnCreateLobbyFaild;
        KitchenGameLobby.Instance.OnCreateLobbyStarted -= KitchenGameLobby_OnCreateLobbyStarted;
        KitchenGameLobby.Instance.OnJoinStarted -= KitchenGameLobby_OnJoinStarted;
        KitchenGameLobby.Instance.OnJoinFailed -= KitchenGameLobby_OnJoinFaild;
        KitchenGameLobby.Instance.OnQuickJoinFaild -= KitchenGameLobby_OnQuickJoinFaild;
    }
}
