using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PauseMultiPlayerUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        KitchenGameManager.Instance.OnMultiPlayerPauseChanged += KitchenGameManager_OnMultiPlayerPauseChanged;
        KitchenGameManager.Instance.OnMultiPlayerUnPauseChanged += KitchenGameManager_OnMultiPlayerUnPauseChanged;
        Hide();
    }

    private void KitchenGameManager_OnMultiPlayerPauseChanged(object sender, System.EventArgs e)
    {
        Show();
    }

    private void KitchenGameManager_OnMultiPlayerUnPauseChanged(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        this.gameObject.SetActive(true);
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
