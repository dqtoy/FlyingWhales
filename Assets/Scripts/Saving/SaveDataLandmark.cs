﻿using System.Collections;
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

        
    }
    public void Load(HexTile tile) {
        BaseLandmark landmark = tile.CreateLandmarkOfType(this);
    }
}
