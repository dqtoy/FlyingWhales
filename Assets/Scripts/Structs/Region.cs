using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;

public class Region {
    private int _id;
    private HexTile _centerOfMass;
    private List<HexTile> _tilesInRegion; //This also includes the center of mass
    private Color regionColor;
    private List<Region> _adjacentRegions;
    private City _occupant;

    private Color defaultBorderColor = new Color(94f / 255f, 94f / 255f, 94f / 255f, 255f / 255f);

    //Landmarks
    private RESOURCE _specialResource;
    private HexTile _tileWithSpecialResource;
	private HexTile _tileWithSummoningShrine;
	private HexTile _tileWithHabitat;
    private List<Landmark> _landmarks;
	private int _landmarkCount;

    private Dictionary<RACE, int> _naturalResourceLevel;
    private int _cityLevelCap;

    //Population
    private int _populationGrowth;

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
    internal List<Region> adjacentRegions {
        get { return _adjacentRegions; }
    }
    internal City occupant {
        get { return _occupant; }
    }
    internal RESOURCE specialResource {
        get { return _specialResource; }
    }
    internal HexTile tileWithSpecialResource {
        get { return _tileWithSpecialResource; }
    }
	internal HexTile tileWithSummoningShrine {
		get { return this._tileWithSummoningShrine; }
	}
	internal HexTile tileWithHabitat {
		get { return this._tileWithHabitat; }
	}
    internal Dictionary<RACE, int> naturalResourceLevel {
        get { return _naturalResourceLevel; }
    }
    internal int cityLevelCap {
        get { return _cityLevelCap; }
    }
    internal int populationGrowth {
        get { return _populationGrowth; }
    }
	internal List<HexTile> corpseMoundTiles {
		get { return this._corpseMoundTiles; }
	}
	internal List<HexTile> outerTiles {
		get { return this._outerTiles; }
	}
    internal List<Landmark> landmarks {
        get { return _landmarks; }
    }
	internal int landmarkCount {
		get { return this._landmarkCount; }
	}
    internal List<object> connections {
        get { return _connections; }
    }
    internal List<HexTile> roadTilesInRegion {
        get { return _roadTilesInRegion; }
    }
    #endregion

    public Region(HexTile centerOfMass) {
        _id = Utilities.SetID(this);
        SetCenterOfMass(centerOfMass);
        _tilesInRegion = new List<HexTile>();
		this._corpseMoundTiles = new List<HexTile> ();
        _connections = new List<object>();
        _roadTilesInRegion = new List<HexTile>();
        _landmarks = new List<Landmark>();
        AddTile(_centerOfMass);
        regionColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);
        SetSpecialResource(RESOURCE.NONE);
		this.foodMultiplierCapacity = 2;
		this.materialMultiplierCapacity = 2;
		this.oreMultiplierCapacity = 2;

        //Generate population growth
        int[] possiblePopulationGrowths = new int[] { 4, 5, 6, 7, 8, 9 };
        _populationGrowth = possiblePopulationGrowths[Random.Range(0, possiblePopulationGrowths.Length)];
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
			_centerOfMass.emptyCityGO.SetActive (false);
            _centerOfMass.isHabitable = false;
        }
        _centerOfMass = newCenter;
        _centerOfMass.isHabitable = true;
		_centerOfMass.emptyCityGO.SetActive (true);
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
    internal void AddTile(HexTile tile) {
        if (!_tilesInRegion.Contains(tile)) {
            _tilesInRegion.Add(tile);
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
        _occupant.kingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.VISIBLE);
        _cityLevelCap = _naturalResourceLevel[occupant.kingdom.race];
        SetAdjacentRegionsAsVisibleForOccupant();
        SetRegionPathfindingTag(occupant.kingdom.kingdomTagIndex);
        Color solidKingdomColor = _occupant.kingdom.kingdomColor;
        solidKingdomColor.a = 255f / 255f;
        ReColorBorderTiles(solidKingdomColor);
        if(_specialResource != RESOURCE.NONE) {
            _tileWithSpecialResource.Occupy(occupant);
            CreateStructureOnSpecialResourceTile();
        }
		StartProducing ();
    }
    internal void RemoveOccupant() {
        City previousOccupant = _occupant;
        _occupant = null;
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
        
        if (_specialResource != RESOURCE.NONE) {
            _tileWithSpecialResource.Unoccupy();
        }
		StopProducing ();
    }
    private void SetAdjacentRegionsAsVisibleForOccupant() {
        for (int i = 0; i < _adjacentRegions.Count; i++) {
            Region currRegion = _adjacentRegions[i];
            if(currRegion._occupant == null || currRegion._occupant.kingdom != _occupant.kingdom) {
                _occupant.kingdom.SetFogOfWarStateForRegion(currRegion, FOG_OF_WAR_STATE.VISIBLE);
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

    #region Landmark Functions
    internal void AddLandmarkToRegion(Landmark landmark) {
        if (!_landmarks.Contains(landmark)) {
            _landmarks.Add(landmark);
        }
    }
    internal void SetSpecialResource(RESOURCE resource) {
        if (resource != RESOURCE.NONE) {
            AssignSpecialResourceToTile(resource);
            //if _tileWithSpecialResource is still null, this means there was no tile found for a special resource
            if (_tileWithSpecialResource != null) {
                this._landmarkCount += 1;
                _specialResource = resource;
				if(this._tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.FOOD){
					this.foodMultiplierCapacity = 3;
				}else if(this._tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.MATERIAL){
					this.materialMultiplierCapacity = 3;
				}else if(this._tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.ORE){
					this.oreMultiplierCapacity = 3;
				}
            }

        } else {
            _specialResource = resource;
        }        
    }
    /*
     * <summary>
     * Once special resource has been generated for this region,
     * this decides on which tile it should be placed.
     * </summary>
     * */
    private void AssignSpecialResourceToTile(RESOURCE specialResource) {
        //Get tiles in region that are plains, not the center of mass and does not have a landmark
        List<HexTile> elligibleTiles = _tilesInRegion.Where(x => x.elevationType == ELEVATION.PLAIN && x.id != centerOfMass.id && !x.hasLandmark && !x.HasNeighbourThatIsLandmark()).ToList();

        //Remove neighbours of the center of mass from the choices, since resource tiles are not supposed to be near the city
        for (int i = 0; i < centerOfMass.AllNeighbours.Count; i++) {
            elligibleTiles.Remove(centerOfMass.AllNeighbours[i]);
        }

        //Remove road tiles from the choices and also their neighbours
        for (int i = 0; i < _roadTilesInRegion.Count; i++) {
            HexTile currRoadTile = _roadTilesInRegion[i];
            elligibleTiles.Remove(currRoadTile);
            //for (int j = 0; j < currRoadTile.AllNeighbours.Count; j++) {
            //    elligibleTiles.Remove(currRoadTile.AllNeighbours[j]);
            //}
        }
        if (elligibleTiles.Count <= 0) {
            return;
            //throw new System.Exception("No elligible tiles for special resource in region " + centerOfMass.name);
        }

        Dictionary<HexTile, List<HexTile>> tilesToChooseFrom = new Dictionary<HexTile, List<HexTile>>();

        //Check minor roads in region and check if they have a path towards the center of mass, if so, connect to the nearest minor road instead
        List<HexTile> minorRoads = new List<HexTile>(_roadTilesInRegion.Where(x => x.roadType == ROAD_TYPE.MINOR));
        List<HexTile> elligibleRoadTiles = new List<HexTile>();
        for (int i = 0; i < minorRoads.Count; i++) {
            HexTile currRoadTile = minorRoads[i];
            if (PathGenerator.Instance.GetPath(currRoadTile, centerOfMass, PATHFINDING_MODE.USE_ROADS) != null) {
                elligibleRoadTiles.Add(currRoadTile);
            }
        }

        if (elligibleRoadTiles.Count > 0) {
            for (int i = 0; i < elligibleTiles.Count; i++) {
                HexTile currElligibleTile = elligibleTiles[i];
                float shortestDistance = Vector2.Distance(currElligibleTile.transform.position, centerOfMass.transform.position);
                elligibleRoadTiles = elligibleRoadTiles.OrderBy(x => Vector2.Distance(currElligibleTile.transform.position, x.transform.position)).ToList();
                for (int j = 0; j < elligibleRoadTiles.Count; j++) {
                    HexTile currRoadTile = elligibleRoadTiles[j];
                    float distance = Vector2.Distance(currElligibleTile.transform.position, currRoadTile.transform.position);
                    if (distance < shortestDistance) {
                        //Get path from current elligible tile to the road tile, use LANDMARK_CONNECTION, to limit the paths to within the region.
                        List<HexTile> path = PathGenerator.Instance.GetPath(currElligibleTile, currRoadTile, PATHFINDING_MODE.LANDMARK_CONNECTION);
                        if (path != null) { //Check if the path that was calculated is not null
                            if (!tilesToChooseFrom.ContainsKey(currElligibleTile)) {
                                tilesToChooseFrom.Add(currElligibleTile, path);
                            }
                            break; //once a valid road tile has been found, break
                        }
                    }
                }
            }
        }

        if (tilesToChooseFrom.Count <= 0) {
            //Check which of the remaining elligible tiles have a path towards the center of mass. 
            //The path must not cross water and, as much as possible, not converge with any main roads
            for (int i = 0; i < elligibleTiles.Count; i++) {
                HexTile currElligibleTile = elligibleTiles[i];
                //Get path from current elligible tile to the center of mass, use LANDMARK_CONNECTION, to limit the paths to within the region.
                List<HexTile> path = PathGenerator.Instance.GetPath(currElligibleTile, centerOfMass, PATHFINDING_MODE.LANDMARK_CONNECTION);
                if (path != null) { //Check if the path that was calculated is not null
                    if (!tilesToChooseFrom.ContainsKey(currElligibleTile)) {
                        tilesToChooseFrom.Add(currElligibleTile, path);
                    }
                }
            }
        }
                

        if(tilesToChooseFrom.Count > 0) {
            HexTile chosenTile = tilesToChooseFrom.Keys.ElementAt(Random.Range(0, tilesToChooseFrom.Keys.Count));
            _tileWithSpecialResource = chosenTile;
            _tileWithSpecialResource.AssignSpecialResource(specialResource);
            RoadManager.Instance.ConnectLandmarkToRegion(_tileWithSpecialResource, this);
            RoadManager.Instance.CreateRoad(tilesToChooseFrom[_tileWithSpecialResource], ROAD_TYPE.MINOR);

            //if (RoadManager.Instance.SmartCreateRoad(chosenTile, centerOfMass, pathfindingModeToUse, ROAD_TYPE.MINOR)) {
            //    _tileWithSpecialResource = chosenTile;
            //    _tileWithSpecialResource.AssignSpecialResource(specialResource);
            //    RoadManager.Instance.ConnectLandmarkToRegion(_tileWithSpecialResource, this);
            //}
        } else {
            Debug.LogWarning("Could not assign resource tile to region on " + centerOfMass.name);
        }
    }
	internal void SetSummoningShrine(){
        if (AssignSummoningShrineToTile()) {
            this._landmarkCount += 1;
        }
	}
    /*
     * <summary>
     * Assign a summoning shrine to a tile.
     * This will return a true/false depending if a summoning shrine was created
     * </summary>
     * */
    private bool AssignSummoningShrineToTile() {
        //Get tiles in region that are plains, not the center of mass and does not have a landmark
        List<HexTile> elligibleTiles = _tilesInRegion.Where(x => x.elevationType == ELEVATION.PLAIN && x.id != centerOfMass.id && !x.hasLandmark && !x.HasNeighbourThatIsLandmark()).ToList();

        //Remove neighbours of the center of mass from the choices, since shrines are not supposed to be near the city
        for (int i = 0; i < centerOfMass.AllNeighbours.Count; i++) {
            elligibleTiles.Remove(centerOfMass.AllNeighbours[i]);
        }

        //Remove neighbours of tile with resource
        if(_tileWithSpecialResource != null) {
            for (int i = 0; i < _tileWithSpecialResource.AllNeighbours.Count; i++) {
                elligibleTiles.Remove(_tileWithSpecialResource.AllNeighbours[i]);
            }
        }
        
        //Remove road tiles from the choices and also their neighbours
        for (int i = 0; i < _roadTilesInRegion.Count; i++) {
            HexTile currRoadTile = _roadTilesInRegion[i];
            elligibleTiles.Remove(currRoadTile);
            for (int j = 0; j < currRoadTile.AllNeighbours.Count; j++) {
                elligibleTiles.Remove(currRoadTile.AllNeighbours[j]);
            }
        }
        if (elligibleTiles.Count <= 0) {
            return false;
            //throw new System.Exception("No elligible tiles for special resource in region " + centerOfMass.name);
        }


        Dictionary<HexTile, List<HexTile>> tilesToChooseFrom = new Dictionary<HexTile, List<HexTile>>();
        //Check minor roads in region and check if they have a path towards the center of mass, if so, connect to the nearest minor road instead
        List<HexTile> minorRoads = new List<HexTile>(_roadTilesInRegion.Where(x => x.roadType == ROAD_TYPE.MINOR));
        List<HexTile> elligibleRoadTiles = new List<HexTile>();
        for (int i = 0; i < minorRoads.Count; i++) {
            HexTile currRoadTile = minorRoads[i];
            if (PathGenerator.Instance.GetPath(currRoadTile, centerOfMass, PATHFINDING_MODE.USE_ROADS) != null) {
                elligibleRoadTiles.Add(currRoadTile);
            }
        }
        
        if(elligibleRoadTiles.Count > 0) {
            for (int i = 0; i < elligibleTiles.Count; i++) {
                HexTile currElligibleTile = elligibleTiles[i];
                float shortestDistance = Vector2.Distance(currElligibleTile.transform.position, centerOfMass.transform.position);
                elligibleRoadTiles = elligibleRoadTiles.OrderBy(x => Vector2.Distance(currElligibleTile.transform.position, x.transform.position)).ToList();
                for (int j = 0; j < elligibleRoadTiles.Count; j++) {
                    HexTile currRoadTile = elligibleRoadTiles[j];
                    float distance = Vector2.Distance(currElligibleTile.transform.position, currRoadTile.transform.position);
                    if(distance < shortestDistance) {
                        //Get path from current elligible tile to the road tile, use LANDMARK_CONNECTION, to limit the paths to within the region.
                        List<HexTile> path = PathGenerator.Instance.GetPath(currElligibleTile, currRoadTile, PATHFINDING_MODE.LANDMARK_CONNECTION);
                        if (path != null) { //Check if the path that was calculated is not null
                            if (!tilesToChooseFrom.ContainsKey(currElligibleTile)) {
                                tilesToChooseFrom.Add(currElligibleTile, path);
                            }
                            break; //once a valid road tile has been found, break
                        }
                    }
                }
            }
        }

        if (tilesToChooseFrom.Count <= 0) {
            //Check which of the remaining elligible tiles have a path towards the center of mass. 
            //The path must not cross water and not converge with any main roads
            for (int i = 0; i < elligibleTiles.Count; i++) {
                HexTile currElligibleTile = elligibleTiles[i];
                //Get path from current elligible tile to the center of mass, use LANDMARK_CONNECTION, to limit the paths to within the region.
                List<HexTile> path = PathGenerator.Instance.GetPath(currElligibleTile, centerOfMass, PATHFINDING_MODE.LANDMARK_CONNECTION);
                if (path != null) { //Check if the path that was calculated is not null
                    if (!tilesToChooseFrom.ContainsKey(currElligibleTile)) {
                        tilesToChooseFrom.Add(currElligibleTile, path);
                    }
                }
            }
        }

        if(tilesToChooseFrom.Count > 0) {
            HexTile chosenTile = tilesToChooseFrom.Keys.ElementAt(Random.Range(0, tilesToChooseFrom.Keys.Count));
            this._tileWithSummoningShrine = chosenTile;
            this._tileWithSummoningShrine.CreateSummoningShrine();
            RoadManager.Instance.ConnectLandmarkToRegion(_tileWithSummoningShrine, this);
            RoadManager.Instance.CreateRoad(tilesToChooseFrom[_tileWithSummoningShrine], ROAD_TYPE.MINOR);
            return true;
        } else {
            Debug.LogWarning("Could not assign summoning shrine to region on " + centerOfMass.name);
            return false;
        }
    }
	internal void SetHabitat(){
        if (AssignHabitatToTile()) {
            this._landmarkCount += 1;
        }
		
		//List<HexTile> elligibleTiles = _tilesInRegion.Where(x => x.elevationType != ELEVATION.WATER && x.id != centerOfMass.id && !x.hasLandmark).ToList();
		//this._tileWithHabitat = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
		//this._tileWithHabitat.CreateHabitat();
	}
    /*
     * <summary>
     * Assign a summoning shrine to a tile.
     * This will return a true/false depending if a summoning shrine was created
     * </summary>
     * */
    private bool AssignHabitatToTile() {
        //Get tiles in region that are plains, not the center of mass and does not have a landmark
        List<HexTile> elligibleTiles = _tilesInRegion.Where(x => x.elevationType == ELEVATION.PLAIN && x.id != centerOfMass.id && !x.hasLandmark && !x.HasNeighbourThatIsLandmark()).ToList();

        //Remove neighbours of the center of mass from the choices, since shrines are not supposed to be near the city
        for (int i = 0; i < centerOfMass.AllNeighbours.Count; i++) {
            elligibleTiles.Remove(centerOfMass.AllNeighbours[i]);
        }

        //Remove neighbours of tile with resource
        if (_tileWithSpecialResource != null) {
            for (int i = 0; i < _tileWithSpecialResource.AllNeighbours.Count; i++) {
                elligibleTiles.Remove(_tileWithSpecialResource.AllNeighbours[i]);
            }
        }

        //Remove road tiles from the choices and also their neighbours
        for (int i = 0; i < _roadTilesInRegion.Count; i++) {
            HexTile currRoadTile = _roadTilesInRegion[i];
            elligibleTiles.Remove(currRoadTile);
            for (int j = 0; j < currRoadTile.AllNeighbours.Count; j++) {
                elligibleTiles.Remove(currRoadTile.AllNeighbours[j]);
            }
        }
        if (elligibleTiles.Count <= 0) {
            return false;
            //throw new System.Exception("No elligible tiles for special resource in region " + centerOfMass.name);
        }

        Dictionary<HexTile, List<HexTile>> tilesToChooseFrom = new Dictionary<HexTile, List<HexTile>>();
        //Check minor roads in region and check if they have a path towards the center of mass, if so, connect to the nearest minor road instead
        List<HexTile> minorRoads = new List<HexTile>(_roadTilesInRegion.Where(x => x.roadType == ROAD_TYPE.MINOR));
        List<HexTile> elligibleRoadTiles = new List<HexTile>();
        for (int i = 0; i < minorRoads.Count; i++) {
            HexTile currRoadTile = minorRoads[i];
            if (PathGenerator.Instance.GetPath(currRoadTile, centerOfMass, PATHFINDING_MODE.USE_ROADS) != null) {
                elligibleRoadTiles.Add(currRoadTile);
            }
        }

        if (elligibleRoadTiles.Count > 0) {
            for (int i = 0; i < elligibleTiles.Count; i++) {
                HexTile currElligibleTile = elligibleTiles[i];
                float shortestDistance = Vector2.Distance(currElligibleTile.transform.position, centerOfMass.transform.position);
                elligibleRoadTiles = elligibleRoadTiles.OrderBy(x => Vector2.Distance(currElligibleTile.transform.position, x.transform.position)).ToList();
                for (int j = 0; j < elligibleRoadTiles.Count; j++) {
                    HexTile currRoadTile = elligibleRoadTiles[j];
                    float distance = Vector2.Distance(currElligibleTile.transform.position, currRoadTile.transform.position);
                    if (distance < shortestDistance) {
                        //Get path from current elligible tile to the road tile, use LANDMARK_CONNECTION, to limit the paths to within the region.
                        List<HexTile> path = PathGenerator.Instance.GetPath(currElligibleTile, currRoadTile, PATHFINDING_MODE.LANDMARK_CONNECTION);
                        if (path != null) { //Check if the path that was calculated is not null
                            if (!tilesToChooseFrom.ContainsKey(currElligibleTile)) {
                                tilesToChooseFrom.Add(currElligibleTile, path);
                            }
                            break; //once a valid road tile has been found, break
                        }
                    }
                }
            }
        }

        if (tilesToChooseFrom.Count <= 0) {
            //Check which of the remaining elligible tiles have a path towards the center of mass. 
            //The path must not cross water and, as much as possible, not converge with any main roads
            for (int i = 0; i < elligibleTiles.Count; i++) {
                HexTile currElligibleTile = elligibleTiles[i];
                //Get path from current elligible tile to the center of mass, use LANDMARK_CONNECTION, to limit the paths to within the region.
                List<HexTile> path = PathGenerator.Instance.GetPath(currElligibleTile, centerOfMass, PATHFINDING_MODE.LANDMARK_CONNECTION);
                if (path != null) { //Check if the path that was calculated is not null
                    if (!tilesToChooseFrom.ContainsKey(currElligibleTile)) {
                        tilesToChooseFrom.Add(currElligibleTile, path);
                    }
                }
            }
        }

        if (tilesToChooseFrom.Count > 0) {
            HexTile chosenTile = tilesToChooseFrom.Keys.ElementAt(Random.Range(0, tilesToChooseFrom.Keys.Count));
            this._tileWithHabitat = chosenTile;
            this._tileWithHabitat.CreateHabitat();
            RoadManager.Instance.ConnectLandmarkToRegion(_tileWithHabitat, this);
            RoadManager.Instance.CreateRoad(tilesToChooseFrom[_tileWithHabitat], ROAD_TYPE.MINOR);
            return true;
        } else {
            Debug.LogWarning("Could not assign habitat tile to region on " + centerOfMass.name);
            return false;
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

        int increaseFromSpecialResource = 0;
        //if(_specialResource != RESOURCE.NONE) {
        //    increaseFromSpecialResource = 3;
        //}

        _naturalResourceLevel[RACE.HUMANS] = (humanTilePoints / 15) + increaseFromSpecialResource;
        _naturalResourceLevel[RACE.ELVES] = (elvenTilePoints / 15) + increaseFromSpecialResource;

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
        if(_occupant != null) {
            text = _occupant.ownedTiles.Count.ToString();
        }
        text += "/" + _naturalResourceLevel[race].ToString();
        midPoint.SetTileText(text, 6, Color.white, "Minimap");
    }
    #endregion

    #region Kingdom Discovery Functions
    internal void CheckForDiscoveredKingdoms() {
        List<Region> adjacentRegionsOfOtherRegions = new List<Region>();
        List<Kingdom> adjacentKingdoms = new List<Kingdom>();

        for (int i = 0; i < _adjacentRegions.Count; i++) {
            Region adjacentRegion = _adjacentRegions[i];
            if(adjacentRegion.occupant != null) {
                Kingdom otherKingdom = adjacentRegion.occupant.kingdom;
                if (otherKingdom != occupant.kingdom) {
                    if (!adjacentKingdoms.Contains(otherKingdom)) {
                        adjacentKingdoms.Add(otherKingdom);
                    }
                    if (!_occupant.kingdom.discoveredKingdoms.Contains(otherKingdom)) {
                        KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, otherKingdom);
                    }
                    _occupant.kingdom.GetRelationshipWithKingdom(otherKingdom).ChangeAdjacency(true);
                }
                
                for (int j = 0; j < adjacentRegion.adjacentRegions.Count; j++) {
                    Region otherAdjacentRegion = adjacentRegion.adjacentRegions[j];
                    if (!_adjacentRegions.Contains(otherAdjacentRegion) && !adjacentRegionsOfOtherRegions.Contains(otherAdjacentRegion) && otherAdjacentRegion != this) {
                        adjacentRegionsOfOtherRegions.Add(otherAdjacentRegion);
                    }
                }
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

        //When you discover another kingdom via adjacency, you also discover all other regions it is adjacent to.
        for (int i = 0; i < adjacentRegionsOfOtherRegions.Count; i++) {
            Region otherAdjacentRegion = adjacentRegionsOfOtherRegions[i];
            if (otherAdjacentRegion.occupant != null) {
                Kingdom adjacentKingdomOfOtherKingdom = otherAdjacentRegion.occupant.kingdom;
                if (!adjacentKingdomOfOtherKingdom.isDead && adjacentKingdomOfOtherKingdom != _occupant.kingdom && !_occupant.kingdom.discoveredKingdoms.Contains(adjacentKingdomOfOtherKingdom)) {
                    KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, adjacentKingdomOfOtherKingdom);
                }
            }
        }

        //When you discover another kingdom via adjacency, you also discover all other kingdoms it is adjacent to.
        for (int i = 0; i < adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = adjacentKingdoms[i];
            List<Kingdom> adjacentKingdomsOfOtherKingdom = otherKingdom.adjacentKingdoms;
            for (int j = 0; j < adjacentKingdomsOfOtherKingdom.Count; j++) {
                Kingdom kingdomToDiscover = adjacentKingdomsOfOtherKingdom[j];
                if (kingdomToDiscover != _occupant.kingdom && !_occupant.kingdom.discoveredKingdoms.Contains(kingdomToDiscover)) {
                    KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, kingdomToDiscover);
                }
            }
        }

        //When a kingdom expands kingdoms it is adjacent to should discover each other
        if (_occupant.kingdom.adjacentKingdoms.Count > 1) {
            for (int i = 0; i < _occupant.kingdom.adjacentKingdoms.Count; i++) {
                Kingdom currentKingdom = _occupant.kingdom.adjacentKingdoms[i];
                for (int j = 0; j < _occupant.kingdom.adjacentKingdoms.Count; j++) {
                    Kingdom otherKingdom = _occupant.kingdom.adjacentKingdoms[j];
                    if (currentKingdom.id != otherKingdom.id && !currentKingdom.discoveredKingdoms.Contains(otherKingdom)) {
                        KingdomManager.Instance.DiscoverKingdom(currentKingdom, otherKingdom);
                    }
                }
            }
        }
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
    internal void AddConnection(object otherObject) {
        if (!_connections.Contains(otherObject)) {
            _connections.Add(otherObject);
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

	internal void StartProducing(){
		Messenger.AddListener("OnDayEnd", ProduceResource);
	}
	internal void StopProducing(){
		Messenger.RemoveListener("OnDayEnd", ProduceResource);
	}
	private void ProduceResource(){
		this._tileWithSpecialResource.ProduceResource();
	}
}
