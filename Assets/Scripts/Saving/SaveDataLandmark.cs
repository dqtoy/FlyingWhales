using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataLandmark {
    public int id;
    public string landmarkName;
    public LANDMARK_TYPE landmarkType;
    public int locationID;
    public int connectedTileID;
    public List<LANDMARK_TAG> landmarkTags;
    public List<int> connectionsTileIDs;
    public List<int> charactersHereIDs;
    //public IWorldObject worldObj;
    //public WorldEvent activeEvent;
    public int eventSpawnedByCharacterID;
    public bool hasEventIconGO;
    //public IWorldEventData eventData;

    public void Save(BaseLandmark landmark) {
        id = landmark.id;
        landmarkName = landmark.landmarkName;
        landmarkType = landmark.specificLandmarkType;
        locationID = landmark.tileLocation.id;
        if(landmark.connectedTile != null) {
            connectedTileID = landmark.connectedTile.id;
        } else {
            connectedTileID = -1;
        }
        landmarkTags = landmark.landmarkTags;

        connectionsTileIDs = new List<int>();
        for (int i = 0; i < landmark.connections.Count; i++) {
            connectionsTileIDs.Add(landmark.connections[i].tileLocation.id);
        }

        charactersHereIDs = new List<int>();
        for (int i = 0; i < landmark.charactersHere.Count; i++) {
            charactersHereIDs.Add(landmark.charactersHere[i].id);
        }

        if (landmark.eventSpawnedBy != null) {
            eventSpawnedByCharacterID = landmark.eventSpawnedBy.id;
        }

        hasEventIconGO = landmark.eventIconGO != null;
    }
    public void Load(HexTile tile) {
        BaseLandmark landmark = tile.CreateLandmarkOfType(this);
        for (int i = 0; i < charactersHereIDs.Count; i++) {
            landmark.AddCharacterHere(CharacterManager.Instance.GetCharacterByID(charactersHereIDs[i]));
        }
        landmark.SetCharacterEventSpawner(CharacterManager.Instance.GetCharacterByID(eventSpawnedByCharacterID));
    }
    public void LoadLandmarkConnections(BaseLandmark landmark) {
        for (int i = 0; i < connectionsTileIDs.Count; i++) {
            BaseLandmark landmarkToConnect = GridMap.Instance.hexTiles[connectionsTileIDs[i]].landmarkOnTile;
            if (!landmark.connections.Contains(landmarkToConnect)) {
                LandmarkManager.Instance.ConnectLandmarks(landmark, landmarkToConnect);
            }
        }
    }
}
