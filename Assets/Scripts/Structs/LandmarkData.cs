using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct LandmarkData {
    [Header("General Data")]
    public string landmarkTypeString;
    public LANDMARK_TYPE landmarkType;
    public int minimumTileCount; //how many tiles does this landmark need
    public int buildDuration; //how many ticks to build this landmark
    public string description;
    public HEXTILE_DIRECTION connectedTileDirection;
    public List<LANDMARK_TAG> uniqueTags;
    public Sprite landmarkObjectSprite;
    public Sprite landmarkTypeIcon;
    public Sprite landmarkPortrait;
    public BiomeLandmarkSpriteListDictionary biomeTileSprites;
    public List<LandmarkStructureSprite> neutralTileSprites; //These are the sprites that will be used if landmark is not owned by a race
    public List<LandmarkStructureSprite> humansLandmarkTileSprites;
    public List<LandmarkStructureSprite> elvenLandmarkTileSprites;

    public void ConstructData() { }
}
