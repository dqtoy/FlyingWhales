using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Region {
    private HexTile _centerOfMass;
    private List<HexTile> _tilesInRegion; //This also includes the center of mass
    private Color regionColor;
    private List<Region> _adjacentRegions;
    private City _occupant;

    //Resources
    private RESOURCE _specialResource;
    private HexTile _tileWithSpecialResource;
    private Dictionary<RACE, int> _naturalResourceLevel;
    private int _cityLevelCap;

    private List<HexTile> _outerTiles;
    private List<SpriteRenderer> regionBorderLines;

    #region getters/sertters
    internal HexTile centerOfMass {
        get { return _centerOfMass; }
    }
    internal List<HexTile> tilesInRegion {
        get { return _tilesInRegion; }
    }
    internal RESOURCE specialResource {
        get { return _specialResource; }
    }
    internal HexTile tileWithSpecialResource {
        get { return _tileWithSpecialResource; }
    }
    internal Dictionary<RACE, int> naturalResourceLevel {
        get { return _naturalResourceLevel; }
    }
    internal int cityLevelCap {
        get { return _cityLevelCap; }
    }
    #endregion

    public Region(HexTile centerOfMass) {
        SetCenterOfMass(centerOfMass);
        _tilesInRegion = new List<HexTile>();
        AddTile(_centerOfMass);
        regionColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);
        SetSpecialResource(RESOURCE.NONE);
    }

    #region Center Of Mass Functions
    internal void ReComputeCenterOfMass() {
        int maxXCoordinate = _tilesInRegion.Max(x => x.xCoordinate);
        int minXCoordinate = _tilesInRegion.Min(x => x.xCoordinate);
        int maxYCoordinate = _tilesInRegion.Max(x => x.yCoordinate);
        int minYCoordinate = _tilesInRegion.Min(x => x.yCoordinate);

        int midPointX = (minXCoordinate + maxXCoordinate) / 2;
        int midPointY = (minYCoordinate + maxYCoordinate) / 2;

        SetCenterOfMass(GridMap.Instance.map[midPointX, midPointY]);
    }
    internal void RevalidateCenterOfMass() {
        if (_centerOfMass.elevationType != ELEVATION.PLAIN || _centerOfMass.specialResource != RESOURCE.NONE) {
            SetCenterOfMass(_tilesInRegion.Where(x => x.elevationType == ELEVATION.PLAIN && x.specialResource == RESOURCE.NONE)
                .OrderBy(x => x.GetDistanceTo(_centerOfMass)).FirstOrDefault());
            if (_centerOfMass == null) {
                throw new System.Exception("center of mass is null!");
            }
        }
    }
    internal void SetCenterOfMass(HexTile newCenter) {
        if(_centerOfMass != null) {
            _centerOfMass.isHabitable = false;
        }
        _centerOfMass = newCenter;
        _centerOfMass.isHabitable = true;
    }
    #endregion

    #region Adjacency Functions
    /*
     * <summary>
     * Check For Adjacent regions, this will populate the
     * _outerTiles and _adjacentRegions Lists. This is only called at the
     * start of the game, after all the regions have been determined. This will
     * also populate regionBorderLines.
     * </summary>
     * */
    internal void CheckForAdjacency() {
        _outerTiles = new List<HexTile>();
        _adjacentRegions = new List<Region>();
        regionBorderLines = new List<SpriteRenderer>();
        for (int i = 0; i < _tilesInRegion.Count; i++) {
            HexTile currTile = _tilesInRegion[i];
            for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
                HexTile currNeighbour = currTile.AllNeighbours[j];
                if (currNeighbour.region != currTile.region) {
                    //Load Border For currTile
                    HEXTILE_DIRECTION borderTileToActivate = currTile.GetNeighbourDirection(currNeighbour);
                    SpriteRenderer border = currTile.ActivateBorder(borderTileToActivate);
                    if (!regionBorderLines.Contains(border)) {
                        regionBorderLines.Add(border);
                    }

                    if (!_outerTiles.Contains(currTile)) {
                        //currTile has a neighbour that is part of a different region, this means it is an outer tile.
                        _outerTiles.Add(currTile);
                    }
                    if (!_adjacentRegions.Contains(currNeighbour.region)) {
                        _adjacentRegions.Add(currNeighbour.region);
                    }
                }
            } 
        }
    }
    #endregion

    #region Tile Functions
    internal void AddTile(HexTile tile) {
        _tilesInRegion.Add(tile);
        tile.SetRegion(this);
    }
    internal void ResetTilesInRegion() {
        for (int i = 0; i < _tilesInRegion.Count; i++) {
            _tilesInRegion[i].SetRegion(null);
        }
        _tilesInRegion.Clear();
    }
    internal void SetOccupant(City occupant) {
        _occupant = occupant;
        _cityLevelCap = _naturalResourceLevel[occupant.kingdom.race];
        SetAdjacentRegionsAsSeenForOccupant();
        ReColorBorderTiles(_occupant.kingdom.kingdomColor);
        if(_specialResource != RESOURCE.NONE) {
            _tileWithSpecialResource.Occupy(occupant);
            CreateStructureOnSpecialResourceTile();
        }
        
    }
    private void SetAdjacentRegionsAsSeenForOccupant() {
        for (int i = 0; i < _adjacentRegions.Count; i++) {
            Region currRegion = _adjacentRegions[i];
            List<HexTile> tilesInCurrRegion = currRegion.tilesInRegion;
            for (int j = 0; j < tilesInCurrRegion.Count; j++) {
                _occupant.kingdom.SetFogOfWarStateForTile(tilesInCurrRegion[j], FOG_OF_WAR_STATE.SEEN);
            }
        }
    }
    private void ReColorBorderTiles(Color color) {
        for (int i = 0; i < regionBorderLines.Count; i++) {
            regionBorderLines[i].color = color;
        }
    }
    /*
     * <summary>
     * Create a structure on the tile with special resource.
     * This is for visuals only, this does not increase the city's(occupant) level.
     * </sumary>
     * */
    private void CreateStructureOnSpecialResourceTile() {
        if(_specialResource != RESOURCE.NONE) {
            tileWithSpecialResource
                .CreateStructureOnTile(Utilities.GetStructureTypeForResource(_occupant.kingdom.race, _specialResource));
        }
    }
    #endregion

    #region Resource Functions
    internal void SetSpecialResource(RESOURCE resource) {
        _specialResource = resource;
        if(_specialResource != RESOURCE.NONE) {
            List<HexTile> elligibleTiles = _tilesInRegion.Where(x => x.elevationType == ELEVATION.PLAIN && x != centerOfMass).ToList();
            _tileWithSpecialResource = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
            _tileWithSpecialResource.AssignSpecialResource(_specialResource);
        }
    }
    /*
     * <summary>
     * Compute the natural resource level for each race.
     * NOTE: Only Call this once special resource is determined, to compute
     * the correct value.
     * </summary>
     * */
    internal void ComputeNaturalResourceLevel() {
        int humanTilePoints = 0;
        int elvenTilePoints = 0;
        _naturalResourceLevel = new Dictionary<RACE, int>() {
            {RACE.HUMANS, 0},
            {RACE.ELVES, 0},
            {RACE.MINGONS, 0},
            {RACE.CROMADS, 0}
        };
        for (int i = 0; i < _tilesInRegion.Count; i++) {
            HexTile currTile = _tilesInRegion[i];
            if(currTile.elevationType == ELEVATION.MOUNTAIN) {
                //if current tile is mountain continue with other additions
                elvenTilePoints += 1;
            } else if (currTile.elevationType == ELEVATION.WATER) {
                //if current tile is water disregard any other additions
                humanTilePoints += 3;
                elvenTilePoints += 3;
                continue;
            }
            switch (currTile.biomeType) {
                case BIOMES.SNOW:
                    humanTilePoints += 1;
                    elvenTilePoints += 1;
                    break;
                case BIOMES.TUNDRA:
                    humanTilePoints += 2;
                    elvenTilePoints += 2;
                    break;
                case BIOMES.DESERT:
                    humanTilePoints += 3;
                    elvenTilePoints += 1;
                    break;
                case BIOMES.GRASSLAND:
                    humanTilePoints += 6;
                    elvenTilePoints += 3;
                    break;
                case BIOMES.WOODLAND:
                    humanTilePoints += 4;
                    elvenTilePoints += 5;
                    break;
                case BIOMES.FOREST:
                    humanTilePoints += 2;
                    elvenTilePoints += 6;
                    break;
                default:
                    break;
            }
        }

        int increaseFromSpecialResource = 0;
        if(_specialResource != RESOURCE.NONE) {
            increaseFromSpecialResource = 3;
        }

        _naturalResourceLevel[RACE.HUMANS] = (humanTilePoints / 10) + increaseFromSpecialResource;
        _naturalResourceLevel[RACE.ELVES] = (elvenTilePoints / 10) + increaseFromSpecialResource;

        //_centerOfMass.SetTileText(specialResource.ToString() + "\n" +
        //    naturalResourceLevel[RACE.HUMANS].ToString() + "\n" +
        //    naturalResourceLevel[RACE.ELVES].ToString(), 5, Color.white, "Minimap");
    }
    #endregion



}
