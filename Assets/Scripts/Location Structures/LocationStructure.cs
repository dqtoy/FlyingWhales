using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LocationStructure {

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

    #region getters
    public Area location {
        get { return _location; }
    }
    public List<SpecialToken> itemsInStructure {
        get { return _itemsHere; }
    }
    #endregion

    public LocationStructure(STRUCTURE_TYPE structureType, Area location, bool isInside) {
        this.structureType = structureType;
        this.isInside = isInside;
        _location = location;
        charactersHere = new List<Character>();
        _itemsHere = new List<SpecialToken>();
        pointsOfInterest = new List<IPointOfInterest>();
        traits = new List<StructureTrait>();
        corpses = new List<Corpse>();
        tiles = new List<LocationGridTile>();
        if (structureType == STRUCTURE_TYPE.DUNGEON || structureType == STRUCTURE_TYPE.WAREHOUSE) {
            AddPOI(new SupplyPile(this));
        }
    }

    #region Utilities
    public void SetInsideState(bool isInside) {
        this.isInside = isInside;
    }
    public void DestroyStructure() {
        _location.RemoveStructure(this);
    }
    #endregion

    #region Residents
    public virtual bool IsOccupied() {
        return false; //will only ever use this in dwellings, to prevent need for casting
    }
    #endregion

    #region Characters
    public void AddCharacterAtLocation(Character character) {
        if (!charactersHere.Contains(character)) {
            charactersHere.Add(character);
            character.SetCurrentStructureLocation(this);
            AddPOI(character);
            OnCharacterAddedToLocation(character);
            //PlaceCharacterAtAppropriateTile(character);
        }
    }
    public void RemoveCharacterAtLocation(Character character) {
        if (charactersHere.Remove(character)) {
            character.SetCurrentStructureLocation(null);
            LocationGridTile tile = character.currentStructureTile;
            character.SetCurrentStructureTileLocation(null);
            RemovePOI(character);
            //OnCharacterRemovedFromTile(character, tile);
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
    public void AddPOI(IPointOfInterest poi) {
        if (!pointsOfInterest.Contains(poi)) {
            pointsOfInterest.Add(poi);
        }
    }
    public void RemovePOI(IPointOfInterest poi) {
        pointsOfInterest.Remove(poi);
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
            if (poi.poiType == POINT_OF_INTEREST_TYPE.SUPLY_PILE) {
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
    public void AddCorpse(Character character) {
        if (!HasCorpseOf(character)) {
            corpses.Add(new Corpse(character, this));
        }
    }
    public bool RemoveCorpse(Character character) {
        return corpses.Remove(GetCorpseOf(character));
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
    private void PlaceCharacterAtAppropriateTile(Character character) {
        LocationGridTile chosenTile = tiles[Random.Range(0, tiles.Count)];
        character.SetCurrentStructureTileLocation(chosenTile);
        location.areaMap.PlaceCharacter(character, chosenTile);
    }
    private void OnCharacterRemovedFromTile(Character character, LocationGridTile tile) {
        //check if there are any characters here that are also at that tile, if there are, fo not remove visual
        for (int i = 0; i < charactersHere.Count; i++) {
            Character currCharacter = charactersHere[i];
            if (currCharacter.currentStructureTile == tile) {
                return;
            }
        }
        location.areaMap.RemoveCharacterVisualFromTile(tile);
    }
    #endregion

    public override string ToString() {
        return structureType.ToString() + " " + location.structures[structureType].IndexOf(this).ToString() + " at " + location.name;
    }
}
