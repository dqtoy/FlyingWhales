using BayatGames.SaveGameFree;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WorldConfigItem : MonoBehaviour {
    [SerializeField] private UITexture worldScreenshot;
    [SerializeField] private UILabel worldConfigName;

    private FileInfo file;
    private string fileName;

    public void SetFile(FileInfo file) {
        this.file = file;
        fileName = file.Name.Replace(Utilities.worldConfigFileExt, "");
        worldConfigName.text = fileName;
        Texture2D worldConfigTexture = IMG2Sprite.LoadTexture(Utilities.worldConfigsSavePath + fileName + ".png");
        worldScreenshot.mainTexture = worldConfigTexture;
        //worldScreenshot.MakePixelPerfect();
    }

    public void OnChooseFile() {
        if (file != null) {
            WorldSaveData data = SaveGame.Load<WorldSaveData>(file.FullName);
            WorldConfigManager.Instance.SetDataToUse(data);
        } else {
            WorldConfigManager.Instance.SetDataToUse(null);
        }
        LevelLoaderManager.Instance.LoadLevel("Main");
    }
}
