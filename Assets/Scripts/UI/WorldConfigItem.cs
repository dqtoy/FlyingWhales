using BayatGames.SaveGameFree;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldConfigItem : MonoBehaviour {
    [SerializeField] private Image worldScreenshot;
    [SerializeField] private TextMeshProUGUI worldConfigName;

    private FileInfo file;
    private string fileName;

    public void SetFile(FileInfo file) {
        this.file = file;
        fileName = file.Name.Replace(Utilities.worldConfigFileExt, "");
        worldConfigName.text = fileName;
        Texture2D worldConfigTexture = IMG2Sprite.LoadTexture(file.DirectoryName + "\\" + fileName + ".png");
        Sprite newSprite = Sprite.Create(worldConfigTexture, new Rect(0, 0, worldConfigTexture.width, worldConfigTexture.height), new Vector2(0.5f, 0.5f));
        worldScreenshot.sprite = newSprite;
        //worldScreenshot.MakePixelPerfect();
    }

    public void OnChooseFile() {
        if (file != null) {
            WorldSaveData data = SaveGame.Load<WorldSaveData>(file.FullName);
            Utilities.ValidateSaveData(data);
            WorldConfigManager.Instance.SetDataToUse(data);
        } else {
            WorldConfigManager.Instance.SetDataToUse(null);
        }
        LevelLoaderManager.Instance.LoadLevel("Game");
    }
}
