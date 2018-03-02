using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;

public class Region {
    private int _id;
    private HexTile _centerOfMass;
    private List<HexTile> _tilesInRegion; //This also includes the center of mass
    private List<HexTile> _outerGridTilesInRegion;
    private Color regionColor;
    private List<Region> _adjacentRegions;
    private List<Region> _adjacentRegionsViaMajorRoad;
    private City _occupant;
    private List<HexTile> _tilesWithMaterials; //The tiles inside the region that have materials

    private Color defaultBorderColor = new Color(94f / 255f, 94f / 255f, 94f / 255f, 255f / 255f);

    //Landmarks
    private RESOURCE _specialResource;
    [System.Obsolete] private HexTile _tileWithSpecialResource;
    [System.Obsolete] private HexTile _tileWithSummoningShrine;
    [System.Obsolete] private HexTile _tileWithHabitat;
    private List<BaseLandmark> _landmarks; //This contains all the landmarks in the region, except for it's city
	private List<BaseLandmark> _allLandmarks; //This contains all the landmarks in the region

    private Dictionary<RACE, int> _naturalResourceLevel;
    private int _cityLevelCap;

    //Population
//    private int _populationGrowth;

	private List<HexTile> _corpseMoundTiles;

    private List<HexTile> _outerTiles;
    private List<SpriteRenderer> regionBorderLines;

    //Pathfinding
    private GraphUpdateObject _guo;

    //Roads
    private List<object> _connections;
    private List<HexTile> _roadTilesInRegion;

	internal int foodMultiplierCapacity;
	internal int materialMultiplierCapacity;
	internal int oreMultiplierCapacity;

	private bool isOtherDay;

	private float _populationGrowth;

    #region getters/sertters
	internal int id {
		get { return this._id; }
	}
    internal HexTile centerOfMass {
        get { return _centerOfMass; }
    }
    internal List<HexTile> tilesInRegion {
        get { return _tilesInRegion; }
    }
    internal List<HexTile> outerGridTilesInRegion {
        get { return _outerGridTilesInRegion; }
    }
    internal List<Region> adjacentRegions {
        get { return _adjacentRegions; }
    }
    internal List<Region> adjacentRegionsViaMajorRoad {
        get { return _adjacentRegionsViaMajorRoad; }
    }
    internal City occupant {
        get { return _occupant; }
    }
 //   internal RESOURCE specialResource {
 //       get { return _specialResource; }
 //   }
 //   internal HexTile tileWithSpecialResource {
 //       get { return _tileWithSpecialResource; }
 //   }
	//internal HexTile tileWithSummoningShrine {
	//	get { return this._tileWithSummoningShrine; }
	//}
	//internal HexTile tileWithHabitat {
	//	get { return this._tileWithHabitat; }
	//}
    internal Dictionary<RACE, int> naturalResourceLevel {
        get { return _naturalResourceLevel; }
    }
    internal int cityLevelCap {
        get { return _cityLevelCap; }
    }
    internal float populationGrowth {
        get { return _populationGrowth; }
    }
	internal List<HexTile> corpseMoundTiles {
		get { return this._corpseMoundTiles; }
	}
	internal List<HexTile> outerTiles {
		get { return this._outerTiles; }
	}
    internal List<BaseLandmark> landmarks {
        get { return _landmarks; }
    }
	internal List<BaseLandmark> allLandmarks {
		get { return _allLandmarks; }
	}
    internal List<object> connections {
        get { return _connections; }
    }
    internal List<HexTile> roadTilesInRegion {
        get { return _roadTilesInRegion; }
    }
    internal Faction owner {
        get { return _centerOfMass.landmarkOnTile.owner; } //The faction that owns this region
    }
    internal BaseLandmark mainLandmark {
        get { return _centerOfMass.landmarkOnTile; }
    }
    internal List<HexTile> tilesWithMaterials {
        get { return _tilesWithMaterials; }
    }
    internal List<ECS.Character> charactersInRegion {
        get { return GetCharactersInRegion(); }
    }
    #endregion

    public Region(HexTile centerOfMass) {
        _id = Utilities.SetID(this);
        SetCenterOfMass(centerOfMass);
        _tilesInRegion = new List<HexTile>();
        _outerGridTilesInRegion = new List<HexTile>();
		this._corpseMoundTiles = new List<HexTile> ();
        _adjacentRegionsViaMajorRoad = new List<Region>();
        _connections = new List<object>();
        _roadTilesInRegion = new List<HexTile>();
        _landmarks = new List<BaseLandmark>();
		_allLandmarks = new List<BaseLandmark> ();
        _tilesWithMaterials = new List<HexTile>();
        AddTile(_centerOfMass);
        regionColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);
		this.foodMultiplierCapacity = 2;
		this.materialMultiplierCapacity = 2;
		this.oreMultiplierCapacity = 2;
		this.isOtherDay = false;

        //Generate population growth
//        int[] possiblePopulationGrowths = new int[] { 4, 5, 6, 7, 8, 9 };
		_populationGrowth = UnityEngine.Random.Range(0.3f, 1.5f) / 100f;
    }

    #region Center Of Mass Functions
    internal void ReComputeCenterOfMass() {
        int maxXCoordinate = _tilesInRegion.Max(x => x.xCoordinate);
        int minXCoordinate = _tilesInRegion.Min(x => x.xCoordinate);
        int maxYCoordinate = _tilesInRegion.Max(x => x.yCoordinate);
        int minYCoordinate = _tilesInRegion.Min(x => x.yCoordinate);

        int midPointX = (minXCoordinate + maxXCoordinate) / 2;
        int midPointY = (minYCoordinate + maxYCoordinate) / 2;

        if(GridMap.Instance.width - 2 >= midPointX) {
            midPointX -= 2;
        }
        if (GridMap.Instance.height - 2 >= midPointY) {
            midPointY -= 2;
        }
        if(midPointX >= 2) {
            midPointX += 2;
        }
        if (midPointY >= 2) {
            midPointY += 2;
        }

        HexTile newCenterOfMass = GridMap.Instance.map[midPointX, midPointY];
        SetCenterOfMass(newCenterOfMass);
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
            _centerOfMass.RemoveLandmarkOnTile();
            _centerOfMass.isHabitable = false;
            _centerOfMass.emptyCityGO.SetActive(false);
        }
        _centerOfMass = newCenter;
        _centerOfMass.isHabitable = true;
		_centerOfMass.emptyCityGO.SetActive (true);
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(LANDMARK_TYPE.CITY);
        _centerOfMass.CreateLandmarkOfType(BASE_LANDMARK_TYPE.SETTLEMENT, LANDMARK_TYPE.CITY, landmarkData.possibleMaterials[Random.Range(0, landmarkData.possibleMaterials.Length)]);
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
                    AddRegionBorderLineSprite(border);

                    if (!_outerTiles.Contains(currTile)) {
                        //currTile has a neighbour that is part of a different region, this means it is an outer tile.
                        _outerTiles.Add(currTile);
                    }
                    if (!_adjacentRegions.Contains(currNeighbour.region)) {
                        if(currNeighbour.region == null) {
                            throw new System.Exception("REGION IS NULL!");
                        } else {
                            _adjacentRegions.Add(currNeighbour.region);
                        }
                        
                    }
                }
            } 
        }
    }
    internal bool IsAdjacentToKingdom(Kingdom kingdom) {
        for (int i = 0; i < _adjacentRegions.Count; i++) {
            Region currRegion = _adjacentRegions[i];
            if(currRegion.occupant != null && currRegion.occupant.kingdom == kingdom) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Tile Functions
    internal List<HexTile> GetTilesAdjacentOnlyTo(Region otherRegion) {
        List<HexTile> adjacentTiles = new List<HexTile>();
        for (int i = 0; i < _outerTiles.Count; i++) {
            HexTile currTile = _outerTiles[i];
            if(currTile.roadType != ROAD_TYPE.MAJOR && currTile.MajorRoadTiles.Count <= 0 && currTile.AllNeighbours.Where(x => x.region.id == otherRegion.id).Any()) {
                bool isOnlyAdjacentToOtherRegion = true;
                for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
                    HexTile currNeighbour = currTile.AllNeighbours[j];
                    if (currNeighbour.region.id != otherRegion.id && currNeighbour.region.id != this.id) {
                        isOnlyAdjacentToOtherRegion = false;
                        break;
                    }
                }
                if (isOnlyAdjacentToOtherRegion) {
                    adjacentTiles.Add(currTile);
                }
            }
        }
        return adjacentTiles;
    }
    internal void AddTile(HexTile tile) {
        if (!_tilesInRegion.Contains(tile)) {
            _tilesInRegion.Add(tile);
            tile.SetRegion(this);
        }
    }
    internal void AddOuterGridTile(HexTile tile) {
        if (!_outerGridTilesInRegion.Contains(tile)) {
            _outerGridTilesInRegion.Add(tile);
            tile.SetRegion(this);
        }
    }
    internal void ResetTilesInRegion() {
        for (int i = 0; i < _tilesInRegion.Count; i++) {
            _tilesInRegion[i].SetRegion(null);
        }
        _tilesInRegion.Clear();
    }
    internal void SetOccupant(City occupant) {
        _occupant = occupant;
        //_occupant.kingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.VISIBLE);
//        _cityLevelCap = _naturalResourceLevel[occupant.kingdom.race];
		_cityLevelCap = 12;
        //SetAdjacentRegionsAsVisibleForOccupant();
        //SetRegionPathfindingTag(occupant.kingdom.kingdomTagIndex);
        Color occupantColor = Color.clear;
        if (_occupant.kingdom != null) {
            occupantColor = _occupant.kingdom.kingdomColor;
        } else {
            occupantColor = _occupant.faction.factionColor;
        }
        occupantColor.a = 255f / 255f;
        ReColorBorderTiles(occupantColor);
        occupant.HighlightAllOwnedTiles(69f / 255f);
        //      if(_specialResource != RESOURCE.NONE) {
        //          _tileWithSpecialResource.Occupy(occupant);
        //          CreateStructureOnSpecialResourceTile();
        //      }
        //if(this._tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.FOOD){
        //	this._occupant.kingdom.AdjustFoodCityCapacity (this._tileWithSpecialResource.cityCapacity);
        //}else if(this._tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.MATERIAL){
        //	if(this._tileWithSpecialResource.specialResource == RESOURCE.SLATE || this._tileWithSpecialResource.specialResource == RESOURCE.GRANITE){
        //		this._occupant.kingdom.AdjustMaterialCityCapacityForHumans (this._tileWithSpecialResource.cityCapacity);
        //	}else if(this._tileWithSpecialResource.specialResource == RESOURCE.OAK || this._tileWithSpecialResource.specialResource == RESOURCE.EBONY){
        //		this._occupant.kingdom.AdjustMaterialCityCapacityForElves (this._tileWithSpecialResource.cityCapacity);
        //	}
        //}else if(this._tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.ORE){
        //	this._occupant.kingdom.AdjustOreCityCapacity (this._tileWithSpecialResource.cityCapacity);
        //}
        //StartProducing ();
    }
    internal void RemoveOccupant() {
        City previousOccupant = _occupant;
        _occupant = null;

		////Remove Resource City Estimation Capacity for Surplus / Deficit
		//if(this._tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.FOOD){
		//	previousOccupant.kingdom.AdjustFoodCityCapacity (-this._tileWithSpecialResource.cityCapacity);
		//}else if(this._tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.MATERIAL){
		//	if(this._tileWithSpecialResource.specialResource == RESOURCE.SLATE || this._tileWithSpecialResource.specialResource == RESOURCE.GRANITE){
		//		previousOccupant.kingdom.AdjustMaterialCityCapacityForHumans (-this._tileWithSpecialResource.cityCapacity);
		//	}else if(this._tileWithSpecialResource.specialResource == RESOURCE.OAK || this._tileWithSpecialResource.specialResource == RESOURCE.EBONY){
		//		previousOccupant.kingdom.AdjustMaterialCityCapacityForElves (-this._tileWithSpecialResource.cityCapacity);
		//	}
		//}else if(this._tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.ORE){
		//	previousOccupant.kingdom.AdjustOreCityCapacity (-this._tileWithSpecialResource.cityCapacity);
		//}

        //Check if this region has adjacent regions that has the same occupant as this one, if so set region as visible
        if (IsAdjacentToKingdom(previousOccupant.kingdom)) {
            previousOccupant.kingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.VISIBLE);
        } else {
            previousOccupant.kingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.SEEN);
        }

        //Change fog of war of region for discovered kingdoms
        for (int i = 0; i < previousOccupant.kingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = previousOccupant.kingdom.discoveredKingdoms[i];
            if (IsAdjacentToKingdom(otherKingdom)) {
                otherKingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.VISIBLE);
            } else {
                otherKingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.HIDDEN);
            }
        }

        //Check adjacent regions
        for (int i = 0; i < adjacentRegions.Count; i++) {
            Region adjacentRegion = adjacentRegions[i];
            if (adjacentRegion.IsAdjacentToKingdom(previousOccupant.kingdom)) {
                previousOccupant.kingdom.SetFogOfWarStateForRegion(adjacentRegion, FOG_OF_WAR_STATE.VISIBLE);
                continue;
            }

            if (adjacentRegion.occupant == null) {
                previousOccupant.kingdom.SetFogOfWarStateForRegion(adjacentRegion, FOG_OF_WAR_STATE.HIDDEN);
            } else {
                Kingdom occupantOfAdjacentRegion = adjacentRegion.occupant.kingdom;
                if (previousOccupant.kingdom.discoveredKingdoms.Contains(occupantOfAdjacentRegion)) {
                    previousOccupant.kingdom.SetFogOfWarStateForRegion(adjacentRegion, FOG_OF_WAR_STATE.SEEN);
                } else if(occupantOfAdjacentRegion == previousOccupant.kingdom) {
                    previousOccupant.kingdom.SetFogOfWarStateForRegion(adjacentRegion, FOG_OF_WAR_STATE.VISIBLE);
                } else {
                    previousOccupant.kingdom.SetFogOfWarStateForRegion(adjacentRegion, FOG_OF_WAR_STATE.HIDDEN);
                }
            }
        }
        SetRegionPathfindingTag(PathfindingManager.unoccupiedTagIndex);

        ReColorBorderTiles(defaultBorderColor);
        
        //if (_specialResource != RESOURCE.NONE) {
        //    _tileWithSpecialResource.Unoccupy();
        //}
		//StopProducing ();
    }
    private void SetAdjacentRegionsAsVisibleForOccupant() {
        for (int i = 0; i < _adjacentRegions.Count; i++) {
            Region currRegion = _adjacentRegions[i];
            if(currRegion._occupant == null || currRegion._occupant.kingdom != _occupant.kingdom) {
                _occupant.kingdom.SetFogOfWarStateForRegion(currRegion, FOG_OF_WAR_STATE.VISIBLE);
            }
        }
    }
    /*
     Highlight all tiles in the region.
         */
    internal void HighlightRegionTiles(Color highlightColor, float highlightAlpha) {
        Color color = highlightColor;
        color.a = highlightAlpha;
        Color fullColor = highlightColor;
        fullColor.a = 255f/255f;
        for (int i = 0; i < this.tilesInRegion.Count; i++) {
            HexTile currentTile = this.tilesInRegion[i];
            //currentTile.kingdomColorSprite.color = color;
            //currentTile.kingdomColorSprite.gameObject.SetActive(true);
            currentTile.SetMinimapTileColor(fullColor);
        }
        for (int i = 0; i < this.outerGridTilesInRegion.Count; i++) {
            HexTile currentTile = this.outerGridTilesInRegion[i];
            //currentTile.kingdomColorSprite.color = color;
            //currentTile.kingdomColorSprite.gameObject.SetActive(true);
            currentTile.SetMinimapTileColor(fullColor);
        }
    }
    internal void ReColorBorderTiles(Color color) {
        Color fullColor = color;
        fullColor.a = 255f / 255f;
        for (int i = 0; i < regionBorderLines.Count; i++) {
            regionBorderLines[i].color = fullColor;
        }
    }
    internal void AddRegionBorderLineSprite(SpriteRenderer sprite) {
        if (!regionBorderLines.Contains(sprite)) {
            regionBorderLines.Add(sprite);
        }
    }
    #endregion

    #region Resources
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
            if (currTile.elevationType == ELEVATION.MOUNTAIN) {
                //if current tile is mountain continue with other additions
                elvenTilePoints += 1;
            } else if (currTile.elevationType == ELEVATION.WATER) {
                //if current tile is water disregard any other additions
                humanTilePoints += 2;
                elvenTilePoints += 2;
                continue;
            }
            switch (currTile.biomeType) {
                case BIOMES.SNOW:
                    humanTilePoints += 1;
                    elvenTilePoints += 1;
                    break;
                case BIOMES.TUNDRA:
                    humanTilePoints += 3;
                    elvenTilePoints += 3;
                    break;
                case BIOMES.DESERT:
                    humanTilePoints += 2;
                    elvenTilePoints += 1;
                    break;
                case BIOMES.GRASSLAND:
                    humanTilePoints += 8;
                    elvenTilePoints += 5;
                    break;
                case BIOMES.WOODLAND:
                    humanTilePoints += 4;
                    elvenTilePoints += 6;
                    break;
                case BIOMES.FOREST:
                    humanTilePoints += 3;
                    elvenTilePoints += 8;
                    break;
                default:
                    break;
            }
        }

        //int increaseFromSpecialResource = 0;
        //if(_specialResource != RESOURCE.NONE) {
        //    increaseFromSpecialResource = 3;
        //}

        //_naturalResourceLevel[RACE.HUMANS] = (humanTilePoints / 15) + increaseFromSpecialResource;
        //_naturalResourceLevel[RACE.ELVES] = (elvenTilePoints / 15) + increaseFromSpecialResource;

        _naturalResourceLevel[RACE.HUMANS] = (humanTilePoints / 15);
        _naturalResourceLevel[RACE.ELVES] = (elvenTilePoints / 15);

        //_centerOfMass.SetTileText(specialResource.ToString() + "\n" +
        //    naturalResourceLevel[RACE.HUMANS].ToString() + "\n" +
        //    naturalResourceLevel[RACE.ELVES].ToString(), 5, Color.white, "Minimap");
    }
    internal void ShowNaturalResourceLevelForRace(RACE race) {
        int maxXCoordinate = _tilesInRegion.Max(x => x.xCoordinate);
        int minXCoordinate = _tilesInRegion.Min(x => x.xCoordinate);
        int maxYCoordinate = _tilesInRegion.Max(x => x.yCoordinate);
        int minYCoordinate = _tilesInRegion.Min(x => x.yCoordinate);

        int midPointX = (minXCoordinate + maxXCoordinate) / 2;
        int midPointY = (minYCoordinate + maxYCoordinate) / 2;

        HexTile midPoint = GridMap.Instance.map[midPointX, midPointY];

        string text = "0";
        if (_occupant != null) {
            text = _occupant.ownedTiles.Count.ToString();
        }
        text += "/" + _naturalResourceLevel[race].ToString();
        midPoint.SetTileText(text, 6, Color.white, "Minimap");
    }
    internal int GetActivelyHarvestedMaterialsOfType(MATERIAL material) {
        int count = 0;
        for (int i = 0; i < _landmarks.Count; i++) {
            BaseLandmark currLandmark = _landmarks[i];
            if(currLandmark is ResourceLandmark) {
                ResourceLandmark resourceLandmark = currLandmark as ResourceLandmark;
                //check if the landmark has the material specified, and already has a structure built on it.
                if (resourceLandmark.materialOnLandmark == material && resourceLandmark.location.HasStructure()) {
                    count++;
                }
            }
        }
        return count;
    }
    #endregion

    #region Materials
    public void AddTileWithMaterial(HexTile tile) {
        if (!_tilesWithMaterials.Contains(tile)) {
            _tilesWithMaterials.Add(tile);
        }
    }
    public void RemoveTileWithMaterial(HexTile tile) {
        _tilesWithMaterials.Remove(tile);
    }
    #endregion

    #region Landmark Functions
    internal void AddLandmarkToRegion(BaseLandmark landmark) {
        if (!_landmarks.Contains(landmark)) {
            _landmarks.Add(landmark);
			_allLandmarks.Add (landmark);
        }
    }
    #endregion

    #region Kingdom Discovery Functions
    internal void CheckForDiscoveredKingdoms() {
        //List<Region> adjacentRegionsOfOtherRegions = new List<Region>();
        //List<Kingdom> adjacentKingdoms = new List<Kingdom>();

        //NEW: Kingdoms only discover kingdoms they are adjacent to!
        for (int i = 0; i < _adjacentRegionsViaMajorRoad.Count; i++) {
            Region adjacentRegion = _adjacentRegionsViaMajorRoad[i];

            if (adjacentRegion.occupant != null) {
                Kingdom otherKingdom = adjacentRegion.occupant.kingdom;
                if (otherKingdom.id != occupant.kingdom.id) {
                    //if (!adjacentKingdoms.Contains(otherKingdom)) {
                    //    adjacentKingdoms.Add(otherKingdom);
                    //}
                    if (!_occupant.kingdom.discoveredKingdoms.Contains(otherKingdom)) {
                        KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, otherKingdom);
                    }
                    _occupant.kingdom.GetRelationshipWithKingdom(otherKingdom).ChangeAdjacency(true);
                }

                //for (int j = 0; j < adjacentRegion.adjacentRegionsViaMajorRoad.Count; j++) {
                //    Region otherAdjacentRegion = adjacentRegion.adjacentRegionsViaMajorRoad[j];
                //    if (!_adjacentRegions.Contains(otherAdjacentRegion) && !adjacentRegionsOfOtherRegions.Contains(otherAdjacentRegion) && otherAdjacentRegion != this) {
                //        adjacentRegionsOfOtherRegions.Add(otherAdjacentRegion);
                //    }
                //}
            }
        }

        ////When you discover another kingdom via adjacency, you discover all other kingdoms that it has discovered
        //for (int i = 0; i < adjacentKingdoms.Count; i++) {
        //    Kingdom otherKingdom = adjacentKingdoms[i];
        //    List<Kingdom> discoveredKingdomsOfOtherKingdom = otherKingdom.adjacentKingdoms;
        //    for (int j = 0; j < discoveredKingdomsOfOtherKingdom.Count; j++) {
        //        Kingdom kingdomToDiscover = discoveredKingdomsOfOtherKingdom[j];
        //        if (kingdomToDiscover != _occupant.kingdom && !_occupant.kingdom.discoveredKingdoms.Contains(kingdomToDiscover)) {
        //            KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, kingdomToDiscover);
        //        }
        //    }
        //}

        ////When you discover another kingdom via adjacency, you also discover all other regions it is adjacent to.
        //for (int i = 0; i < adjacentRegionsOfOtherRegions.Count; i++) {
        //    Region otherAdjacentRegion = adjacentRegionsOfOtherRegions[i];
        //    if (otherAdjacentRegion.occupant != null) {
        //        Kingdom adjacentKingdomOfOtherKingdom = otherAdjacentRegion.occupant.kingdom;
        //        if (!adjacentKingdomOfOtherKingdom.isDead && adjacentKingdomOfOtherKingdom != _occupant.kingdom && !_occupant.kingdom.discoveredKingdoms.Contains(adjacentKingdomOfOtherKingdom)) {
        //            KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, adjacentKingdomOfOtherKingdom);
        //        }
        //    }
        //}

        ////When you discover another kingdom via adjacency, you also discover all other kingdoms it is adjacent to.
        //for (int i = 0; i < adjacentKingdoms.Count; i++) {
        //    Kingdom otherKingdom = adjacentKingdoms[i];
        //    List<Kingdom> adjacentKingdomsOfOtherKingdom = otherKingdom.adjacentKingdoms;
        //    for (int j = 0; j < adjacentKingdomsOfOtherKingdom.Count; j++) {
        //        Kingdom kingdomToDiscover = adjacentKingdomsOfOtherKingdom[j];
        //        if (kingdomToDiscover != _occupant.kingdom && !_occupant.kingdom.discoveredKingdoms.Contains(kingdomToDiscover)) {
        //            KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, kingdomToDiscover);
        //        }
        //    }
        //}

        ////When a kingdom expands kingdoms it is adjacent to should discover each other
        //if (_occupant.kingdom.adjacentKingdoms.Count > 1) {
        //    for (int i = 0; i < _occupant.kingdom.adjacentKingdoms.Count; i++) {
        //        Kingdom currentKingdom = _occupant.kingdom.adjacentKingdoms[i];
        //        for (int j = 0; j < _occupant.kingdom.adjacentKingdoms.Count; j++) {
        //            Kingdom otherKingdom = _occupant.kingdom.adjacentKingdoms[j];
        //            if (currentKingdom.id != otherKingdom.id && !currentKingdom.discoveredKingdoms.Contains(otherKingdom)) {
        //                KingdomManager.Instance.DiscoverKingdom(currentKingdom, otherKingdom);
        //            }
        //        }
        //    }
        //}
    }
    #endregion

    #region Pathfinding
    private void SetRegionPathfindingTag(int pathfindingTag) {
        for (int i = 0; i < tilesInRegion.Count; i++) {
            tilesInRegion[i].SetPathfindingTag(pathfindingTag);
        }

        //PathfindingManager.Instance.RescanSpecificPortion(_guo);
    }
    #endregion

    #region Road Functions
    internal void AddConnection(Region otherRegion) {
        if (!_connections.Contains(otherRegion)) {
            _connections.Add(otherRegion);
            if (!_adjacentRegionsViaMajorRoad.Contains(otherRegion)) {
                _adjacentRegionsViaMajorRoad.Add(otherRegion);
            }
        }
    }
    internal void AddConnection(BaseLandmark otherObject) {
        if (!_connections.Contains(otherObject)) {
            _connections.Add(otherObject);
            if (otherObject is Settlement) {
                if (!_adjacentRegionsViaMajorRoad.Contains(otherObject.location.region)) {
                    _adjacentRegionsViaMajorRoad.Add(otherObject.location.region);
                }
            }
        }
    }
    internal void AddTileAsRoad(HexTile tile) {
        if (!_roadTilesInRegion.Contains(tile)) {
            _roadTilesInRegion.Add(tile);
        }
    }
    internal void RemoveTileAsRoad(HexTile tile) {
        _roadTilesInRegion.Remove(tile);
    }
    #endregion

    internal void AddCorpseMoundTile(HexTile hexTile){
		this._corpseMoundTiles.Add (hexTile);
	}
	internal void RemoveCorpseMoundTile(HexTile hexTile){
		this._corpseMoundTiles.Remove (hexTile);
	}

    #region Utilities
    private List<ECS.Character> GetCharactersInRegion() {
        List<ECS.Character> characters = new List<ECS.Character>();
        for (int i = 0; i < tilesInRegion.Count; i++) {
            HexTile currTile = tilesInRegion[i];
            characters.AddRange(currTile.charactersAtLocation.Select(x => x.mainCharacter));
            if (currTile.landmarkOnTile != null) {
                characters.AddRange(currTile.landmarkOnTile.charactersAtLocation.Select(x => x.mainCharacter));
            }
        }
        return characters;
    }
    #endregion

    //internal void StartProducing(){
    //	Messenger.AddListener("OnDayEnd", ProduceResource);
    //}
    //internal void StopProducing(){
    //	Messenger.RemoveListener("OnDayEnd", ProduceResource);
    //}
    //private void ProduceResource(){
    //	if(!isOtherDay){
    //		isOtherDay = true;
    //		return;
    //	}else{
    //		isOtherDay = false;
    //	}
    //	this._tileWithSpecialResource.ProduceResource();
    //}
}
