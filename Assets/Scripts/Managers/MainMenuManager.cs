using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using BayatGames.SaveGameFree;

public class MainMenuManager : MonoBehaviour {

    [Header("Main Menu")]
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button newGameButton;

    [Space(10)]
    [Header("World Configurations Menu")]
    [SerializeField] private GameObject worldConfigsMenuGO;
    [SerializeField] private GameObject worldConfigPrefab;
    [SerializeField] private GameObject worldConfigContent;

    [ContextMenu("Get Combinations")]
    public void GetCombinations() {
        List<int> sample = new List<int> { 1, 2 };
        List<List<int>> result = Ruinarch.Utilities.ItemCombinations(sample, 3, 3);
        for (int i = 0; i < result.Count; i++) {
            string log = "\n{";
            for (int j = 0; j < result[i].Count(); j++) {
                log += " " + result[i][j]+ ",";
            }
            log += " }";
            Debug.Log(log);
        }
    }

    #region Monobehaviours
    public void Awake() {
        
    }
    private void Start() {
        Initialize();
        AudioManager.Instance.PlayFade("Main Menu", 5, () => MainMenuUI.Instance.ShowMenuButtons());
        LevelLoaderManager.SetLoadingState(false);
    }
    #endregion
    private void Initialize() {
        SaveManager.Instance.LoadSaveData();
        newGameButton.interactable = true;
        loadGameButton.interactable = false;
        //loadGameButton.interactable = SaveManager.Instance.currentSave != null;
    }
    public void OnClickPlayGame() {
        //PlayGame();
        //ShowWorldConfigurations();
        //MainMenuUI.Instance.HideMenuButtons();
        SaveManager.Instance.SetCurrentSave(null);
        newGameButton.interactable = false;
        loadGameButton.interactable = false;
        AudioManager.Instance.TransitionTo("Loading", 10, () => OnFinishMusicTransition());
    }
    public void OnClickLoadGame() {
        newGameButton.interactable = false;
        loadGameButton.interactable = false;
        AudioManager.Instance.TransitionTo("Loading", 10, () => OnFinishMusicTransition());
    }

    private void OnFinishMusicTransition() {
        //WorldConfigManager.Instance.SetDataToUse(newGameData); //Remove so that code will randomly generate world.
        LevelLoaderManager.Instance.LoadLevel("Game");
    }

    private void PlayGame() {
        LevelLoaderManager.Instance.LoadLevel("Game");
    }
}
