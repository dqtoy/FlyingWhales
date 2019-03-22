using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LocationStructure {
    public string name { get; private set; }
    public STRUCTURE_TYPE structureType { get; private set; }
    public bool isInside { get; private set; }
    public List<Character> charactersHere { get; private set; }
    [System.NonSerialized]
    private Area _location;
    private List<SpecialToken> _itemsHere;
    public List<IPointOfInterest> pointsOfInterest { get; private set; }
    public List<StructureTrait> traits { get; private set; }
    public List<Corpse> corpses { get; private set; }
    public List<LocationGridTile> tiles { get; private set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }
    public POI_STATE state { get; private set; }

    #region getters
    public Area location {
        get { return _location; }
    }
    public List<SpecialToken> itemsInStructure {
        get { return _itemsHere; }
    }
    public List<LocationGridTile> unoccupiedTiles {
        get { return tiles.Where(x => !x.isOccupied).ToList(); }
    }
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.STRUCTURE; }
    }
    #endregion

    public LocationStructure(STRUCTURE_TYPE structureType, Area location, bool isInside) {
        this.structureType = structureType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString());
        this.isInside = isInside;
        _location = location;
        charactersHere = new List<Character>();
        _itemsHere = new List<SpecialToken>();
        pointsOfInterest = new List<IPointOfInterest>();
        traits = new List<StructureTrait>();
        corpses = new List<Corpse>();
        tiles = new List<LocationGridTile>();
        AddListeners();
        //if (structureType == STRUCTURE_TYPE.DUNGEON || structureType == STRUCTURE_TYPE.WAREHOUSE) {
        //    AddPOI(new SupplyPile(this));
        //}
    }

    #region Residents
    public virtual bool IsOccupied() {
        return false; //will only ever use this in dwellings, to prevent need for casting
    }
    #endregion

    #region Characters
    public void AddCharacterAtLocation(Character character, LocationGridTile tile = null) {
        if (!charactersHere.Contains(character)) {
            charactersHere.Add(character);
            character.SetCurrentStructureLocation(this);
            if(AddPOI(character, tile)){
                character.ScanForAwareness();
            }
            OnCharacterAddedToLocation(character);
        }
    }
    public void RemoveCharacterAtLocation(Character character) {
        if (charactersHere.Remove(character)) {
            character.SetCurrentStructureLocation(null);
            //LocationGridTile tile = character.currentStructureTile;
            //character.SetCurrentStructureTileLocation(null);
            RemovePOI(character);
        }
    }
    private void OnCharacterAddedToLocation(Character character) {
        for (int i = 0; i < traits.Count; i++) {
            StructureTrait trait = traits[i];
            trait.OnCharacterEnteredStructure(character);
        }
    }
    #endregion

    #region Items/Special Tokens
    public void AddItem(SpecialToken token) {
        if (!_itemsHere.Contains(token)) {
            _itemsHere.Add(token);
            token.SetStructureLocation(this);
            AddPOI(token);
        }
    }
    public void RemoveItem(SpecialToken token) {
        if (_itemsHere.Remove(token)) {
            token.SetStructureLocation(null);
            RemovePOI(token);
        }
    }
    public void OwnItemsInLocation(Faction owner) {
        for (int i = 0; i < _itemsHere.Count; i++) {
            _itemsHere[i].SetOwner(owner);
        }
    }
    #endregion

    #region Points Of Interest
    public bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null) {
        if (!pointsOfInterest.Contains(poi)) {
#if !WORLD_CREATION_TOOL
            if (!PlacePOIAtAppropriateTile(poi, tileLocation)) { return false; }
#endif
            pointsOfInterest.Add(poi);
            return true;
        }
        return false;
    }
    public void RemovePOI(IPointOfInterest poi) {
        if (pointsOfInterest.Remove(poi)) {
#if !WORLD_CREATION_TOOL
            if (poi.gridTileLocation != null) {
                Debug.Log("Removed " + poi.ToString() + " from " + poi.gridTileLocation.ToString() + " at " + this.ToString());
                if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                    location.areaMap.RemoveCharacter(poi.gridTileLocation, poi as Character);
                } else {
                    location.areaMap.RemoveObject(poi.gridTileLocation);
                }
                //throw new System.Exception("Provided tile of " + poi.ToString() + " is null!");
            }
            
#endif  
        }
    }
    public bool HasPOIOfType(POINT_OF_INTEREST_TYPE type) {
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            if (pointsOfInterest[i].poiType == type) {
                return true;
            }
        }
        return false;
    }
    public SupplyPile GetSupplyPile() {
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            IPointOfInterest poi = pointsOfInterest[i];
            if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT && (poi as TileObject).tileObjectType == TILE_OBJECT_TYPE.SUPPLY_PILE) {
                return poi as SupplyPile;
            }
        }
        return null;
    }
    public IPointOfInterest GetRandomPOI() {
        if (pointsOfInterest.Count <= 0) {
            return null;
        }
        return pointsOfInterest[Random.Range(0, pointsOfInterest.Count)];
    }
    private bool PlacePOIAtAppropriateTile(IPointOfInterest poi, LocationGridTile tile) {
        if (tile != null) {
            location.areaMap.PlaceObject(poi, tile);
            return true;
        } else {
            List<LocationGridTile> tilesToUse;
            if (location.areaType == AREA_TYPE.DEMONIC_INTRUSION) { //player area
                tilesToUse = tiles;
            } else {
                tilesToUse = GetValidTilesToPlace(poi);
            }
            if (tilesToUse.Count > 0) {
                LocationGridTile chosenTile = tilesToUse[Random.Range(0, tilesToUse.Count)];
                location.areaMap.PlaceObject(poi, chosenTile);
                return true;
            } else {
                Debug.LogWarning("There are no tiles at " + structureType.ToString() + " at " + location.name + " for " + poi.ToString());
            }
        }
        return false;
    }
    //public void SpawnFoodOnStartDay() {
    //    if(structureType == STRUCTURE_TYPE.WILDERNESS) {
    //        LocationGridTile[] berryRabbitSpawnPoints = GetSpawnPointsForFood(_location.SPAWN_BERRY_COUNT + _location.SPAWN_RABBIT_COUNT);
    //        for (int i = 0; i < berryRabbitSpawnPoints.Length; i++) {
    //            if(berryRabbitSpawnPoints[i] != null) {
    //                if (UnityEngine.Random.Range(0, 2) == 0) {
    //                    if(foodCount[FOOD.BERRY] < _location.MAX_EDIBLE_PLANT) {
    //                        AddPOI(CreateFood(FOOD.BERRY), berryRabbitSpawnPoints[i]);
    //                    } else if (foodCount[FOOD.RABBIT] < _location.MAX_SMALL_ANIMAL) {
    //                        AddPOI(CreateFood(FOOD.RABBIT), berryRabbitSpawnPoints[i]);
    //                    }
    //                } else {
    //                    if (foodCount[FOOD.RABBIT] < _location.MAX_SMALL_ANIMAL) {
    //                        AddPOI(CreateFood(FOOD.RABBIT), berryRabbitSpawnPoints[i]);
    //                    } else if (foodCount[FOOD.BERRY] < _location.MAX_EDIBLE_PLANT) {
    //                        AddPOI(CreateFood(FOOD.BERRY), berryRabbitSpawnPoints[i]);
    //                    }
    //                }
    //            }
    //        }
    //    }else if (structureType == STRUCTURE_TYPE.DUNGEON) {
    //        LocationGridTile[] mushroomRatSpawnPoints = GetSpawnPointsForFood(_location.SPAWN_MUSHROOM_COUNT + _location.SPAWN_RAT_COUNT);
    //        for (int i = 0; i < mushroomRatSpawnPoints.Length; i++) {
    //            if (mushroomRatSpawnPoints[i] != null) {
    //                if (UnityEngine.Random.Range(0, 2) == 0) {
    //                    if (foodCount[FOOD.MUSHROOM] < _location.MAX_MUSHROOM) {
    //                        AddPOI(CreateFood(FOOD.MUSHROOM), mushroomRatSpawnPoints[i]);
    //                    } else if (foodCount[FOOD.RAT] < _location.MAX_RAT) {
    //                        AddPOI(CreateFood(FOOD.RAT), mushroomRatSpawnPoints[i]);
    //                    }
    //                } else {
    //                    if (foodCount[FOOD.RAT] < _location.MAX_RAT) {
    //                        AddPOI(CreateFood(FOOD.RAT), mushroomRatSpawnPoints[i]);
    //                    } else if (foodCount[FOOD.MUSHROOM] < _location.MAX_MUSHROOM) {
    //                        AddPOI(CreateFood(FOOD.MUSHROOM), mushroomRatSpawnPoints[i]);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
    //private LocationGridTile[] GetSpawnPointsForFood(int spawnCount) {
    //    LocationGridTile[] spawnPoints = new LocationGridTile[spawnCount];
    //    List<LocationGridTile> unoccupied = unoccupiedTiles;
    //    for (int i = 0; i < spawnPoints.Length; i++) {
    //        if(unoccupied.Count > 0) {
    //            int index = UnityEngine.Random.Range(0, unoccupied.Count);
    //            spawnPoints[i] = unoccupied[index];
    //            unoccupied.RemoveAt(index);
    //        } else {
    //            break;
    //        }
    //    }
    //    return spawnPoints;
    //}
    //public void AdjustFoodCount(FOOD food, int amount) {
    //    foodCount[food] += amount;
    //}
    //private IPointOfInterest CreateFood(FOOD foodType) {
    //    switch (foodType) {
    //        case FOOD.BERRY:
    //        case FOOD.MUSHROOM:
    //            return new EdiblePlant(this);
    //        case FOOD.RABBIT:
    //        case FOOD.RAT:
    //            return new SmallAnimal(this);
    //        default:
    //            return null;
    //    }
    //}
    private List<LocationGridTile> GetValidTilesToPlace(IPointOfInterest poi) {
        switch (poi.poiType) {
            case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                if (poi is MagicCircle) {
                    return unoccupiedTiles.Where(x => !x.HasOccupiedNeighbour() && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall)).ToList();
                } else if (poi is Guitar || poi is Bed || poi is Table) {
                    return GetOuterTiles().Where(x => unoccupiedTiles.Contains(x)).ToList();
                } else {
                    return unoccupiedTiles;
                }
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                return unoccupiedTiles;
            default:
                return unoccupiedTiles.Where(x => !x.IsAdjacentTo(typeof(MagicCircle))).ToList();
        }
    }
    #endregion

    #region Traits
    public void AddTrait(string traitName) {
        StructureTrait createdTrait = null;
        switch (traitName) {
            case "Booby Trapped":
                createdTrait = new BoobyTrapped(this);
                break;
            case "Poisoned Food":
                createdTrait = new PoisonedFood(this);
                break;
            default:
                break;
        }
        if (createdTrait != null) {
            traits.Add(createdTrait);
        }
    }
    public void RemoveTrait(StructureTrait trait) {
        traits.Remove(trait);
    }
    public void RemoveTrait(string traitName) {
        RemoveTrait(GetTrait(traitName));
    }
    public StructureTrait GetTrait(string traitName) {
        for (int i = 0; i < traits.Count; i++) {
            StructureTrait currTrait = traits[i];
            if (currTrait.name == traitName) {
                return currTrait;
            }
        }
        return null;
    }
    #endregion

    #region Corpses
    public void AddCorpse(Character character, LocationGridTile tile) {
        if (!HasCorpseOf(character)) {
            Corpse corpse = new Corpse(character, this);
            corpses.Add(corpse);
            AddPOI(corpse, tile);
        }
    }
    public bool RemoveCorpse(Character character) {
        Corpse corpse = GetCorpseOf(character);
        RemovePOI(corpse);
        return corpses.Remove(corpse);
    }
    public bool HasCorpseOf(Character character) {
        for (int i = 0; i < corpses.Count; i++) {
            Corpse currCorpse = corpses[i];
            if (currCorpse.character.id == character.id) {
                return true;
            }
        }
        return false;
    }
    private Corpse GetCorpseOf(Character character) {
        for (int i = 0; i < corpses.Count; i++) {
            Corpse currCorpse = corpses[i];
            if (currCorpse.character.id == character.id) {
                return currCorpse;
            }
        }
        return null;
    }
    #endregion

    #region Tiles
    public void AddTile(LocationGridTile tile) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
        }
    }
    public void RemoveTile(LocationGridTile tile) {
        tiles.Remove(tile);
    }
    public bool IsFull() {
        return unoccupiedTiles.Count <= 0;
    }
    public LocationGridTile GetNearestTileTo(LocationGridTile tile) {
        LocationGridTile nearestTile = null;
        float nearestDist = 99999f;
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            float dist = currTile.GetDistanceTo(tile);
            if (dist < nearestDist) {
                nearestTile = currTile;
                nearestDist = dist;
            }
        }
        return nearestTile;
    }
    public float GetNearestDistanceTo(LocationGridTile tile) {
        LocationGridTile nearestTile = null;
        float nearestDist = 99999f;
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            float dist = currTile.GetDistanceTo(tile);
            if (dist < nearestDist) {
                nearestTile = currTile;
                nearestDist = dist;
            }
        }
        return nearestDist;
    }
    public bool HasRoadTo(LocationGridTile tile) {
        for (int i = 0; i < tiles.Count; i++) {
            if (PathGenerator.Instance.GetPath(tiles[i], tile, GRID_PATHFINDING_MODE.ROADS_ONLY, true) != null) {
                return true;
            }
        }
        return false;
    }
    public LocationGridTile GetRandomUnoccupiedTile() {
        if (unoccupiedTiles.Count <= 0) {
            return null;
        }
        return unoccupiedTiles[Random.Range(0, unoccupiedTiles.Count)];
    }
    #endregion

    #region Utilities
    public void SetInsideState(bool isInside) {
        this.isInside = isInside;
    }
    public void DestroyStructure() {
        _location.RemoveStructure(this);
        RemoveListeners();
    }
    private void AddListeners() {
        //Messenger.AddListener(Signals.DAY_STARTED, SpawnFoodOnStartDay);
    }
    private void RemoveListeners() {
        //Messenger.RemoveListener(Signals.DAY_STARTED, SpawnFoodOnStartDay);
    }
    /// <summary>
    /// Get the structure's name based on specified rules.
    /// Rules are at - https://trello.com/c/mRzzH9BE/1432-location-naming-convention
    /// </summary>
    /// <param name="character">The character requesting the name</param>
    public virtual string GetNameRelativeTo(Character character) {
        switch (structureType) {
            case STRUCTURE_TYPE.INN:
                return "at the inn";
            case STRUCTURE_TYPE.WAREHOUSE:
                return "at the " + location.name + " warehouse";
            case STRUCTURE_TYPE.WILDERNESS:
                return "outside of " + location.name;
            case STRUCTURE_TYPE.DUNGEON:
            case STRUCTURE_TYPE.WORK_AREA:
            case STRUCTURE_TYPE.EXPLORE_AREA:
                return "in " + location.name;
            default:
                return ToString();
        }
    }
    public List<LocationGridTile> GetOuterTiles() {
        List<LocationGridTile> outerTiles = new List<LocationGridTile>();
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            if (currTile.HasDifferentDwellingOrOutsideNeighbour()) {
                outerTiles.Add(currTile);
            }
        }
        return outerTiles;
    }
    #endregion

    public override string ToString() {
        return structureType.ToString() + " " + location.structures[structureType].IndexOf(this).ToString() + " at " + location.name;
    }

    //#region Point Of Interest
    //public List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
    //    if (poiGoapActions != null && poiGoapActions.Count > 0) {
    //        List<GoapAction> usableActions = new List<GoapAction>();
    //        for (int i = 0; i < poiGoapActions.Count; i++) {
    //            if (actorAllowedInteractions.Contains(poiGoapActions[i])) {
    //                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(poiGoapActions[i], actor, this);
    //                if (goapAction.CanSatisfyRequirements()) {
    //                    usableActions.Add(goapAction);
    //                }
    //            }
    //        }
    //        return usableActions;
    //    }
    //    return null;
    //}
    //#endregion
}
