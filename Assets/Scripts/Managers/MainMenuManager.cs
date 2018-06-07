using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

    [Header("Main Menu")]
    [SerializeField] private Button loadGameButton;

    [Space(10)]
    [Header("World Configurations Menu")]
    [SerializeField] private GameObject worldConfigsMenuGO;
    [SerializeField] private GameObject worldConfigPrefab;
    [SerializeField] private GameObject worldConfigContent;

    public void OnClickPlayGame() {
        //PlayGame();
        ShowWorldConfigurations();
    }

    private void ShowWorldConfigurations() {
        worldConfigsMenuGO.SetActive(true);
        LoadWorldConfigurations();
    }

    private void LoadWorldConfigurations() {
        Directory.CreateDirectory(Utilities.worldConfigsSavePath);
        DirectoryInfo info = new DirectoryInfo(Utilities.worldConfigsSavePath);
        FileInfo[] files = info.GetFiles("*.worldConfig");
        for (int i = 0; i < files.Length; i++) {
            FileInfo currFile = files[i];
            GameObject configGO = GameObject.Instantiate(worldConfigPrefab, worldConfigContent.transform);
            configGO.transform.localScale = Vector3.one;
            WorldConfigItem item = configGO.GetComponent<WorldConfigItem>();
            item.SetFile(currFile);
        }
    }

    private void PlayGame() {
        LevelLoaderManager.Instance.LoadLevel("Main");
    }
}
