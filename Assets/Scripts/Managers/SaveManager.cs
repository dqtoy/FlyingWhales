using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree;

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;
    public Save currentSave { get; private set; }

    private const string saveFileName = "CURRENT_SAVE_FILE";

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }

    public void SetCurrentSave(Save save) {
        currentSave = save;
    }
    public void SaveCurrentStateOfWorld() {
        Save save = new Save((int)GridMap.Instance.width, (int)GridMap.Instance.height, GridMap.Instance._borderThickness);
        save.SaveHextiles(GridMap.Instance.hexTiles);
        save.SaveOuterHextiles(GridMap.Instance.outerGridList);
        save.SavePlayerArea(PlayerManager.Instance.player.playerArea);
        save.SaveNonPlayerAreas(LandmarkManager.Instance.allNonPlayerAreas);
        save.SaveFactions(FactionManager.Instance.allFactions);

        SaveGame.Save<Save>(Utilities.gameSavePath + saveFileName, save);
    }
    public void LoadSaveData() {
        if(Utilities.DoesFileExist(Utilities.gameSavePath + saveFileName)) {
            SetCurrentSave(SaveGame.Load<Save>(Utilities.gameSavePath + saveFileName));
        }
    }
}
