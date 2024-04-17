using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        KitchenGameMultiplayer.Instance.OnFailedJoinGame += KitchenGameMultiplayer_OnFailedJoinGame;
        KitchenGameMultiplayer.Instance.OnTtyingToJoinGame += KitchenGameMultiplayer_OnTtyingToJoinGame;
        Hide();
    }

    private void KitchenGameMultiplayer_OnTtyingToJoinGame(object sender, System.EventArgs e)
    {
        Show();
    }

    private void KitchenGameMultiplayer_OnFailedJoinGame(object sender, System.EventArgs e)
    {
        Hide();
    }
    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnFailedJoinGame -= KitchenGameMultiplayer_OnFailedJoinGame;
        KitchenGameMultiplayer.Instance.OnTtyingToJoinGame -= KitchenGameMultiplayer_OnTtyingToJoinGame;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
