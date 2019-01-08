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

    [Space(10)]
    [Header("World Configurations Menu")]
    [SerializeField] private GameObject worldConfigsMenuGO;
    [SerializeField] private GameObject worldConfigPrefab;
    [SerializeField] private GameObject worldConfigContent;
    [SerializeField] private ContentSorter worldConfigContentSorter;

    [ContextMenu("Get Combinations")]
    public void GetCombinations() {
        List<int> sample = new List<int> { 1, 2 };
        List<List<int>> result = Utilities.ItemCombinations(sample, 3, 3);
        for (int i = 0; i < result.Count; i++) {
            string log = "\n{";
            for (int j = 0; j < result[i].Count(); j++) {
                log += " " + result[i][j]+ ",";
            }
            log += " }";
            Debug.Log(log);
        }
    }

    private WorldSaveData newGameData;

    #region Monobehaviours
    public void Awake() {
        LoadNewGameData();
    }
    #endregion

    public void OnClickPlayGame() {
        //PlayGame();
        //ShowWorldConfigurations();
        WorldConfigManager.Instance.SetDataToUse(newGameData);
        LevelLoaderManager.Instance.LoadLevel("Main");
    }

    private void ShowWorldConfigurations() {
        worldConfigsMenuGO.SetActive(true);
        LoadWorldConfigurations();
    }

    private void LoadNewGameData() {
        DirectoryInfo templateDirInfo = new DirectoryInfo(Utilities.worldConfigsTemplatesPath);
        FileInfo[] templateFiles = templateDirInfo.GetFiles("*.worldConfig");
        if (templateFiles.Length == 0) {
            throw new System.Exception("There is no new game data");
        }
        newGameData = SaveGame.Load<WorldSaveData>(templateFiles[0].FullName);
        Utilities.ValidateSaveData(newGameData);
    }

    private void LoadWorldConfigurations() {
        //initial templates
        Directory.CreateDirectory(Utilities.worldConfigsTemplatesPath);
        DirectoryInfo templateDirInfo = new DirectoryInfo(Utilities.worldConfigsTemplatesPath);
        FileInfo[] templateFiles = templateDirInfo.GetFiles("*.worldConfig");
        for (int i = 0; i < templateFiles.Length; i++) {
            FileInfo currFile = templateFiles[i];
            GameObject configGO = GameObject.Instantiate(worldConfigPrefab, worldConfigContent.transform);
            configGO.transform.localScale = Vector3.one;
            WorldConfigItem item = configGO.GetComponent<WorldConfigItem>();
            item.SetFile(currFile);
        }

        //custom maps
        Directory.CreateDirectory(Utilities.worldConfigsSavePath);
        DirectoryInfo customMapDirInfo = new DirectoryInfo(Utilities.worldConfigsSavePath);
        FileInfo[] customMapFiles = customMapDirInfo.GetFiles("*.worldConfig");
        for (int i = 0; i < customMapFiles.Length; i++) {
            FileInfo currFile = customMapFiles[i];
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
