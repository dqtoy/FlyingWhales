
using System.Collections.Generic;
using Inner_Maps;

public interface ILocation {
    
    int id { get; }
    string name { get; }
    HexTile coreTile { get; }
    InnerTileMap innerMap { get; }
    LOCATION_TYPE locationType { get; }
    List<Character> charactersAtLocation { get; }
    Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; }
    LocationStructure mainStorage { get; }

    #region Awareness
    bool AddAwareness(IPointOfInterest pointOfInterest);
    void RemoveAwareness(IPointOfInterest pointOfInterest);
    void RemoveAwareness(POINT_OF_INTEREST_TYPE poiType);
    bool HasAwareness(IPointOfInterest poi);
    #endregion

    #region Structures
    void AddStructure(LocationStructure structure);
    void RemoveStructure(LocationStructure structure);
    LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type);
    LocationStructure GetRandomStructure();
    LocationStructure GetStructureByID(STRUCTURE_TYPE type, int id);
    List<LocationStructure> GetStructuresAtLocation();
    bool HasStructure(STRUCTURE_TYPE type);
    void OnLocationStructureObjectPlaced(LocationStructure structure);
    #endregion

    #region Characters
    void AddCharacterToLocation(Character character, LocationGridTile tileOverride = null, bool isInitial = false);
    void RemoveCharacterFromLocation(Character character);
    void RemoveCharacterFromLocation(Party party);
    #endregion

    //bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null);
    //void RemoveSpecialTokenFromLocation(SpecialToken token);
    bool IsRequiredByLocation(TileObject item);
    /// <summary>
    /// Does the given location share the same core tile as this location?
    /// </summary>
    /// <param name="location">The location to query.</param>
    /// <returns>True or false</returns>
    bool IsSameCoreLocationAs(ILocation location);
}
