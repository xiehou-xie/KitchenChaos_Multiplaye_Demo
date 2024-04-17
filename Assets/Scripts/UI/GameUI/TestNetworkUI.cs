using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestNetworkUI : MonoBehaviour
{
    [SerializeField] private Transform uiParent;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clentButton;
    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartHost();
            Hide();
        });
        clentButton.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartClient();
            Hide();
        });
    }
    private void Hide()
    {
        uiParent.gameObject.SetActive(false);
    }
}
