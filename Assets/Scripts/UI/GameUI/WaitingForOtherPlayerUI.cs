using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForOtherPlayerUI : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        KitchenGameManager.Instance.OnLocalPlayerReadyChanged += KitchenGameManager_OnLocalPlayerReadyChanged;
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
        Hide();
    }

    private void KitchenGameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsLocalPlayerReady())
        {
            Show();
        }
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsCountdownToStartActive())
        {
            Hide();
        }
    }

    private void Show()
    {
        this.transform.gameObject.SetActive(true);
    }
    private void Hide()
    {
        this.transform.gameObject.SetActive(false);
    }
}
