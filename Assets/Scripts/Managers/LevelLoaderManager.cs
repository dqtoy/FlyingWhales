using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoaderManager : MonoBehaviour {
    public static LevelLoaderManager Instance;

    [SerializeField] private GameObject loaderGO;
    [SerializeField] private UIProgressBar loaderProgressBar;
    [SerializeField] private UILabel loaderText;
    [SerializeField] private UILabel loaderInfoText;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void LoadLevel(string sceneName) {
        StartCoroutine(LoadLevelAsynchronously(sceneName));
    }

    IEnumerator LoadLevelAsynchronously(string sceneName) {
        loaderGO.SetActive(true);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        UpdateLoadingInfo("Loading Scene...");
        while (asyncOperation.progress < 0.9f) {
            var scaledPerc = 0.5f * asyncOperation.progress / 0.9f;
            loaderText.text = (100f * scaledPerc).ToString("F0") + " %";
        }

        asyncOperation.allowSceneActivation = true;
        float perc = 0.5f;
        while (!asyncOperation.isDone) {
            yield return null;
            perc = Mathf.Lerp(perc, 1f, 0.05f);
            loaderText.text = (100f * perc).ToString("F0") + " %";
        }

        UpdateLoadingInfo("Loading Complete...");
    }

    public void UpdateLoadingInfo(string info) {
        loaderInfoText.text = info;
    }
}
