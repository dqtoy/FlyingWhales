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

    private float _progress;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void LoadLevel(string sceneName) {
        StartCoroutine(LoadLevelAsynchronously(sceneName));
    }

    IEnumerator LoadLevelAsynchronously(string sceneName) {
        _progress = 0f;
        loaderGO.SetActive(true);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
        UpdateLoadingInfo("Loading Scene...");
        while (asyncOperation.progress < 0.9f) {
            _progress = 0.5f * asyncOperation.progress / 0.9f;
            UpdateLoadingBar(_progress);
        }

        //asyncOperation.allowSceneActivation = true;
        float newProg = _progress;
        
        while (_progress < 1f) {
            yield return null;
            _progress += 0.05f;
            if(_progress > 1f) {
                _progress = 1f;
            }
            UpdateLoadingBar(_progress);
        }
        UpdateLoadingInfo("Loading Complete...");
        loaderGO.SetActive(false);
        asyncOperation.allowSceneActivation = true;
    }

    public void UpdateLoadingInfo(string info) {
        loaderInfoText.text = info;
    }

    public void UpdateLoadingBar(float amount) {
        loaderText.text = (100f * amount).ToString("F0") + " %";
        loaderProgressBar.value = amount;
    }
}
