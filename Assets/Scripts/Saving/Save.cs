using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Save {
    public int width;
    public int height;
    public int borderThickness;
    public List<SaveDataHextile> hextileSaves;
    public List<SaveDataHextile> outerHextileSaves;
    public List<SaveDataLandmark> landmarkSaves;
    public List<SaveDataArea> nonPlayerAreaSaves;
    public List<SaveDataFaction> factionSaves;

    public SaveDataArea playerAreaSave;

    public Save(int width, int height, int borderThickness) {
        this.width = width;
        this.height = height;
        this.borderThickness = borderThickness;
    }

    public void SaveHextiles(List<HexTile> tiles) {
        hextileSaves = new List<SaveDataHextile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            SaveDataHextile newSaveData = new SaveDataHextile();
            newSaveData.Save(currTile);
            hextileSaves.Add(newSaveData);
            if(currTile.landmarkOnTile != null) {
                SaveLandmark(currTile.landmarkOnTile);
            }
        }
    }
    public void SaveOuterHextiles(List<HexTile> tiles) {
        outerHextileSaves = new List<SaveDataHextile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            SaveDataHextile newSaveData = new SaveDataHextile();
            newSaveData.Save(currTile);
            outerHextileSaves.Add(newSaveData);
        }
    }
    private void SaveLandmark(BaseLandmark landmark) {
        if(landmarkSaves == null) {
            landmarkSaves = new List<SaveDataLandmark>();
        }
        SaveDataLandmark newSaveData = new SaveDataLandmark();
        newSaveData.Save(landmark);
        SortAddSaveDataLandmark(newSaveData);
    }
    public void SaveLandmarks(List<HexTile> tiles) {
        landmarkSaves = new List<SaveDataLandmark>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            if(currTile.landmarkOnTile != null) {
                SaveDataLandmark newSaveData = new SaveDataLandmark();
                newSaveData.Save(currTile.landmarkOnTile);
                SortAddSaveDataLandmark(newSaveData);
            }
        }
    }
    private void SortAddSaveDataLandmark(SaveDataLandmark newSaveData) {
        bool hasBeenInserted = false;
        for (int i = 0; i < landmarkSaves.Count; i++) {
            SaveDataLandmark currSaveData = landmarkSaves[i];
            if (newSaveData.id < currSaveData.id) {
                landmarkSaves.Insert(i, newSaveData);
                hasBeenInserted = true;
                break;
            }
        }
        if (!hasBeenInserted) {
            landmarkSaves.Add(newSaveData);
        }
    }
    public void LoadLandmarks() {
        for (int i = 0; i < hextileSaves.Count; i++) {
            SaveDataHextile saveDataHextile = hextileSaves[i];
            if (saveDataHextile.landmarkID != -1) {
                HexTile currTile = GridMap.Instance.hexTiles[saveDataHextile.id];
                //We get the index for the appropriate landmark of hextile through (landmarkID - 1) because the list of landmarksaves is properly ordered
                //Example, the save data in index 0 of the list has an id of 1 since all ids in game start at 1, that is why to get the index of the landmark of the tile, we get the true landmark id and subtract it by 1
                //This is done so that we will not loop every time we want to get the save data of a landmark and check all the ids if it will match
                landmarkSaves[saveDataHextile.landmarkID - 1].Load(currTile);
            }
        }
    }
    public void SavePlayerArea(Area area) {
        playerAreaSave = new SaveDataArea();
        playerAreaSave.Save(area);
    }
    public void LoadPlayerArea() {
        playerAreaSave.Load();
    }
    public void SaveNonPlayerAreas(List<Area> areas) {
        nonPlayerAreaSaves = new List<SaveDataArea>();
        for (int i = 0; i < areas.Count; i++) {
            Area area = areas[i];
            SaveDataArea saveDataArea = new SaveDataArea();
            saveDataArea.Save(area);
            nonPlayerAreaSaves.Add(saveDataArea);
        }
    }
    public void LoadNonPlayerAreas() {
        for (int i = 0; i < nonPlayerAreaSaves.Count; i++) {
            nonPlayerAreaSaves[i].Load();
        }
    }

    public void SaveFactions(List<Faction> factions) {
        factionSaves = new List<SaveDataFaction>();
        for (int i = 0; i < factions.Count; i++) {
            SaveDataFaction saveDataFaction = new SaveDataFaction();
            saveDataFaction.Save(factions[i]);
            factionSaves.Add(saveDataFaction);
        }
    }
    public void LoadFactions() {
        List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks();
        for (int i = 0; i < factionSaves.Count; i++) {
            factionSaves[i].Load(allLandmarks);
        }
    }


}
