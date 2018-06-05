using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuGO;
    [SerializeField] private UIButton loadGameButton;

    [Space(10)]
    [Header("World Configurations Menu")]
    [SerializeField] private GameObject worldConfigsMenuGO;
    [SerializeField] private GameObject worldConfigPrefab;
    [SerializeField] private UIScrollView worldConfigScrollView;
    [SerializeField] private UIGrid worldConfigGrid;

    private FileInfo[] _saveFiles;

    void Awake() {
        EncryptSaveFiles();
    }
    void Start() {
        _saveFiles = GetSaveFiles();
        EnableDisableLoadButton();
    }
    private void EncryptSaveFiles() {
        //SaveGame.Encode = true;
    }
    private FileInfo[] GetSaveFiles() {
        //FileInfo[] saves = SaveGame.GetFiles();
        //if(saves.Length > 0) {
        //    return saves;
        //}
        return null;
    }
    private void EnableDisableLoadButton() {
        if(_saveFiles != null) {
            loadGameButton.isEnabled = true;
        } else {
            loadGameButton.isEnabled = false;
        }
    }
    public void LoadGame() {
        //Save save = SaveGame.Load<Save>(_saveFiles[0].Name);
        //SaveManager.Instance.currentSave = save;
        //LevelLoaderManager.Instance.LoadLevel("Main");
    }

    public void OnClickPlayGame() {
        ShowWorldConfigurations();
    }

    private void ShowWorldConfigurations() {
        worldConfigsMenuGO.SetActive(true);
        mainMenuGO.SetActive(false);
        LoadWorldConfigurations();
    }

    private void LoadWorldConfigurations() {
        Directory.CreateDirectory(Utilities.worldConfigsSavePath);
        DirectoryInfo info = new DirectoryInfo(Utilities.worldConfigsSavePath);
        FileInfo[] files = info.GetFiles("*.worldConfig");
        for (int i = 0; i < files.Length; i++) {
            FileInfo currFile = files[i];
            GameObject configGO = GameObject.Instantiate(worldConfigPrefab, worldConfigGrid.transform);
            configGO.transform.localScale = Vector3.one;
            WorldConfigItem item = configGO.GetComponent<WorldConfigItem>();
            item.SetFile(currFile);
            worldConfigGrid.Reposition();
            worldConfigScrollView.ResetPosition();
        }
    }

    public void PlayGame() {
        LevelLoaderManager.Instance.LoadLevel("Main");
    }
}
