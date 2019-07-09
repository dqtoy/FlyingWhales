using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldSaveData {
    public int width;
    public int height;
    public int borderThickness;
    public List<HexTileData> tilesData;
    public List<HexTileData> outerGridTilesData;
    public List<FactionSaveData> factionsData;
    public List<LandmarkSaveData> landmarksData;
    public List<CharacterSaveData> charactersData;
    //public List<MonsterSaveData> monstersData;
    public List<AreaSaveData> areaData;
    public byte[] pathfindingSettings;

    private Dictionary<int, HexTileData> tileDictionary;
    private Dictionary<int, HexTileData> outerTileDictionary;

    public WorldSaveData(int width, int height, int borderThickness) {
        this.width = width;
        this.height = height;
        this.borderThickness = borderThickness;
    }

    public void OccupyTileData(List<HexTile> tiles) {
        tilesData = new List<HexTileData>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            tilesData.Add(currTile.data);
        }
    }
    public void OccupyOuterTileData(List<HexTile> outerTiles) {
        outerGridTilesData = new List<HexTileData>();
        for (int i = 0; i < outerTiles.Count; i++) {
            HexTile currTile = outerTiles[i];
            outerGridTilesData.Add(currTile.data);
        }
    }
    public void OccupyFactionData(List<Faction> factions) {
        factionsData = new List<FactionSaveData>();
        for (int i = 0; i < factions.Count; i++) {
            Faction currFaction = factions[i];
            FactionSaveData factionData = new FactionSaveData(currFaction);
            factionsData.Add(factionData);
        }
    }
    public void OccupyLandmarksData(List<BaseLandmark> landmarks) {
        landmarksData = new List<LandmarkSaveData>();
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            LandmarkSaveData landmarkData = new LandmarkSaveData(currLandmark);
            landmarksData.Add(landmarkData);
        }
    }
    public void OccupyCharactersData(List<Character> characters) {
        charactersData = new List<CharacterSaveData>();
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            CharacterSaveData characterData = new CharacterSaveData(currCharacter);
            charactersData.Add(characterData);
        }
    }
    public void OccupyAreaData(List<Area> areas) {
        areaData = new List<AreaSaveData>();
        for (int i = 0; i < areas.Count; i++) {
            Area currArea = areas[i];
            AreaSaveData currData = new AreaSaveData(currArea);
            areaData.Add(currData);
        }
    }
    //public void OccupySquadData(List<Squad> squads) {
    //    squadData = new List<SquadSaveData>();
    //    for (int i = 0; i < squads.Count; i++) {
    //        Squad currSquad = squads[i];
    //        SquadSaveData data = new SquadSaveData(currSquad);
    //        squadData.Add(data);
    //    }
    //}
    //public void OccupyMonstersData(List<MonsterParty> monsters) {
    //    monstersData = new List<MonsterSaveData>();
    //    for (int i = 0; i < monsters.Count; i++) {
    //        MonsterParty currMonster = monsters[i];
    //        MonsterSaveData monsterData = new MonsterSaveData(currMonster);
    //        monstersData.Add(monsterData);
    //    }
    //}
    public void OccupyPathfindingSettings(HexTile[,] map, int width, int height) {
        //PathfindingManager.Instance.ClearGraphs();
        //PathfindingManager.Instance.CreateGrid(map, width, height);
        Pathfinding.Serialization.SerializeSettings settings = new Pathfinding.Serialization.SerializeSettings();
        //Save node info, and output nice JSON
        settings.nodes = true;
        pathfindingSettings = AstarPath.active.data.SerializeGraphs(settings);
    }
    public bool HasFactionlessCharacter() {
        for (int i = 0; i < charactersData.Count; i++) {
            CharacterSaveData data = charactersData[i];
            if (data.factionID == -1) {
                return true;
            }
        }
        return false;
    }
    public void ConstructTileDictionary() {
        tileDictionary = new Dictionary<int, HexTileData>();
        for (int i = 0; i < tilesData.Count; i++) {
            tileDictionary.Add(tilesData[i].id, tilesData[i]);
        }
    }

    public HexTileData GetTileData(int tileID) {
        if (tileDictionary == null) {
            for (int i = 0; i < tilesData.Count; i++) {
                HexTileData currData = tilesData[i];
                if (currData.id == tileID) {
                    return currData;
                }
            }
        } else {
            return tileDictionary[tileID];
        }
        return null;
    }

    public HexTileData GetOuterTileData(int tileID) {
        if (outerTileDictionary == null) {
            for (int i = 0; i < outerGridTilesData.Count; i++) {
                HexTileData currData = outerGridTilesData[i];
                if (currData.id == tileID) {
                    return currData;
                }
            }
        } else {
            return outerTileDictionary[tileID];
        }
        return null;
    }
    public FactionSaveData GetFactionData(int factionID) {
        if (factionsData != null) {
            for (int i = 0; i < factionsData.Count; i++) {
                FactionSaveData data = factionsData[i];
                if (data.factionID == factionID) {
                    return data;
                }
            }
        }
        
        return null;
    }

    public CharacterSaveData GetCharacterSaveData(int characterID) {
        if (charactersData != null) {
            for (int i = 0; i < charactersData.Count; i++) {
                CharacterSaveData data = charactersData[i];
                if (data.id == characterID) {
                    return data;
                }
            }
        }
        return null;
    }

    
}
