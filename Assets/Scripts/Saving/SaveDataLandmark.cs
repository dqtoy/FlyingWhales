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
    public SaveDataWorldObject worldObj;
    public bool hasWorldObject;
    public WORLD_EVENT activeEvent;
    public int eventSpawnedByCharacterID;
    public bool hasEventIconGO;
    public SaveDataWorldEventData eventData;

    public virtual void Save(BaseLandmark landmark) {
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

        if(landmark.activeEvent != null) {
            activeEvent = landmark.activeEvent.eventType;
            eventSpawnedByCharacterID = landmark.eventSpawnedBy.id;

            var typeName = "SaveData" + landmark.eventData.GetType().ToString();
            eventData = System.Activator.CreateInstance(System.Type.GetType(typeName)) as SaveDataWorldEventData;
            eventData.Save(landmark.eventData);
        } else {
            activeEvent = WORLD_EVENT.NONE;
        }
        hasEventIconGO = landmark.eventIconGO != null;

        if(landmark.worldObj != null) {
            hasWorldObject = true;

            if (landmark.worldObj is Artifact) {
                worldObj = new SaveDataArtifact();
            } else if (landmark.worldObj is Summon) {
                worldObj = new SaveDataSummon();
            } else {
                var typeName = "SaveData" + landmark.worldObj.GetType().ToString();
                worldObj = System.Activator.CreateInstance(System.Type.GetType(typeName)) as SaveDataWorldObject;
            }
            worldObj.Save(landmark.worldObj);
        } else {
            hasWorldObject = false;
        }
    }
    public void Load(HexTile tile) {
        BaseLandmark landmark = tile.CreateLandmarkOfType(this);
        for (int i = 0; i < charactersHereIDs.Count; i++) {
            landmark.LoadCharacterHere(CharacterManager.Instance.GetCharacterByID(charactersHereIDs[i]));
        }
    }
    public void LoadLandmarkConnections(BaseLandmark landmark) {
        for (int i = 0; i < connectionsTileIDs.Count; i++) {
            BaseLandmark landmarkToConnect = GridMap.Instance.hexTiles[connectionsTileIDs[i]].landmarkOnTile;
            if (!landmark.connections.Contains(landmarkToConnect)) {
                LandmarkManager.Instance.ConnectLandmarks(landmark, landmarkToConnect);
            }
        }
    }
    public void LoadActiveEventAndWorldObject(BaseLandmark landmark) {
        landmark.LoadEventAndWorldObject(this);
    }
}
