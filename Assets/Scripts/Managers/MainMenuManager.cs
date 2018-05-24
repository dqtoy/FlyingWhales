using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {

    public UIButton loadGameButton;
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

    public void PlayGame() {
        LevelLoaderManager.Instance.LoadLevel("Main");
    }
}
