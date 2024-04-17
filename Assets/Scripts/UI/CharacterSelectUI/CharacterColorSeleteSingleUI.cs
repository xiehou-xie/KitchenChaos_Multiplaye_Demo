using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSeleteSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image colorimage;
    [SerializeField] private GameObject selectedGameObject;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.ChangePlayerColor(colorId);
        });
    }
    void Start()
    {
        KitchenGameMultiplayer.Instance.OnPlayDataNetworkListChanged += KitchenGameMultiplayer_OnPlayDataNetworkListChanged;
        colorimage.color = KitchenGameMultiplayer.Instance.GetPlayerColor(colorId);
        UpdateIsSelected();
    }

    private void KitchenGameMultiplayer_OnPlayDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        if(KitchenGameMultiplayer.Instance.GetPlayerData().colorId == colorId)
        {
            selectedGameObject.SetActive(true);
        }
        else
        {
            selectedGameObject.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnPlayDataNetworkListChanged -= KitchenGameMultiplayer_OnPlayDataNetworkListChanged;
    }
}
