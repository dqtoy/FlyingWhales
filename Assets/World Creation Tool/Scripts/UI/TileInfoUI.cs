using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInfoUI : MonoBehaviour {

    [SerializeField] private Text tileInfoText;
    [SerializeField] private EnvelopContentUnityUI envelopContent;

    public void Initialize() {
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OVER, ShowTileInfo);
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OUT, HideTileInfo);
    }

    private void ShowTileInfo(HexTile tile) {
        string info = string.Empty;
        info += GetTileBasicInfo(tile);
        if (tile.landmarkOnTile != null) {
            info += "\n";
            info += GetLandmarkInfo(tile.landmarkOnTile);
        }
        if (tile.areaOfTile != null) {
            info += "\n";
            info += GetAreaInfo(tile.areaOfTile);
        }
        tileInfoText.text = info;
        this.gameObject.SetActive(true);
        envelopContent.Execute();
    }
    private void HideTileInfo(HexTile tile) {
        this.gameObject.SetActive(false);
    }

    private string GetTileBasicInfo(HexTile tile) {
        string info = string.Empty;
        info = "<b>Basic Info</b>";
        info += "\nTile Name: [" + tile.id.ToString() + "] " + tile.ToString();
        info += "\nBiome: " + tile.biomeType.ToString();
        info += "\nElevation Type: " + tile.elevationType.ToString();
        return info;
    }

    private string GetLandmarkInfo(BaseLandmark landmark) {
        string info = string.Empty;
        info = "\n<b>Landmark Info</b>";
        info += "\nLandmark Name: [" + landmark.id + "] " + landmark.landmarkName;
        info += "\nLandmark Type: " + landmark.specificLandmarkType.ToString();
        info += "\nCivilians: " + landmark.civilianCount.ToString();
        //info += "\nCharacters with homes here: ";
        //for (int i = 0; i < landmark.charactersWithHomeOnLandmark.Count; i++) {
        //    ECS.Character currChar = landmark.charactersWithHomeOnLandmark[i];
        //    info += "\n   - " + currChar.name;
        //}
        info += "\nCharacters located here: ";
        for (int i = 0; i < landmark.charactersAtLocation.Count; i++) {
            ICharacter currChar = landmark.charactersAtLocation[i];
            info += "\n   - " + currChar.name;
        }
        return info;
    }

    private string GetAreaInfo(Area area) {
        string info = string.Empty;
        info = "\n<b>Area Info</b>";
        info += "\nArea Name: [" + area.id + "] " + area.name;
        info += "\nArea Type: " + area.areaType.ToString();
        info += "\nCore Tile: " + area.coreTile.tileName;
        info += "\nTile Count: " + area.tiles.Count.ToString();
        info += "\nLandmarks: " + area.landmarks.Count.ToString();
        info += "\nTotal Civilian Count: " + area.totalCivilians.ToString();

        return info;
    }
}
