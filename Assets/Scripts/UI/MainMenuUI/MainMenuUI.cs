using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private Button playMultipButton;
    [SerializeField] private Button playSingleButton;
    [SerializeField] private Button quitButton;


    private void Awake()
    {
        playMultipButton.onClick.AddListener(() => {
            KitchenGameMultiplayer.playMultiplayer = true;
            Loader.Load(Loader.Scene.LobbyScene);
        });
        playSingleButton.onClick.AddListener(() => {
            KitchenGameMultiplayer.playMultiplayer = false;
            Loader.Load(Loader.Scene.LobbyScene);
        });
        quitButton.onClick.AddListener(() => {
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #else
                    Application.Quit();
            #endif
        });

        Time.timeScale = 1f;
    }

}