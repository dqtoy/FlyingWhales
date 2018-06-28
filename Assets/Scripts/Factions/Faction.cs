/*
 This is the base class for each faction (major/minor)
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Faction {
	protected int _id;
    protected string _name;
    protected ILeader _leader;
    private Sprite _emblem;
    private Sprite _emblemBG;
    protected List<Region> _ownedRegions;
    protected List<BaseLandmark> _ownedLandmarks;
    internal Color factionColor;
    protected List<ECS.Character> _characters; //List of characters that are part of the faction
    protected Dictionary<Faction, FactionRelationship> _relationships;
	protected int _warmongering;
    protected List<BaseLandmark> _landmarkInfo;
    protected List<Area> _ownedAreas;

    #region getters/setters
	public int id {
		get { return _id; }
	}
    public string name {
		get { return _name; }
    }
	public string urlName {
		get { return "<link=" + '"' + this._id.ToString() + "_faction" + '"' +">" + this._name + "</link>"; }
    }
    public ILeader leader {
        get { return _leader; }
    }
    public Sprite emblem {
        get { return _emblem; }
    }
    public Sprite emblemBG {
        get { return _emblemBG; }
    }
    public List<ECS.Character> characters {
        get { return _characters; }
    }
    public List<Region> ownedRegions {
        get { return _ownedRegions; }
    }
    public Dictionary<Faction, FactionRelationship> relationships {
        get { return _relationships; }
    }
	public int warmongering {
		get { return _warmongering; }
	}
	public float factionPower{
		get { return this._characters.Sum (x => x.characterPower); }
	}
    public int activeWars {
        get { return relationships.Where(x => x.Value.isAtWar).Count(); }
    }
    public List<BaseLandmark> landmarkInfo {
        get { return _landmarkInfo; }
    }
    public List<BaseLandmark> ownedLandmarks {
        get { return _ownedLandmarks; }
    }
    public List<Area> ownedAreas {
        get { return _ownedAreas; }
    }
    #endregion

    public Faction() {
		this._id = Utilities.SetID<Faction>(this);
        SetName(RandomNameGenerator.Instance.GenerateKingdomName());
        _emblem = FactionManager.Instance.GenerateFactionEmblem(this);
        _emblemBG = FactionManager.Instance.GenerateFactionEmblemBG();
        factionColor = Utilities.GetColorForFaction();
        _characters = new List<ECS.Character>();
        _ownedLandmarks = new List<BaseLandmark>();
        _ownedRegions = new List<Region>();
        _relationships = new Dictionary<Faction, FactionRelationship>();
		_warmongering = 0;
        _landmarkInfo = new List<BaseLandmark>();
        _ownedAreas = new List<Area>();
    }

    public Faction(FactionSaveData data) {
        _id = Utilities.SetID(this, data.factionID);
        SetName(data.factionName);
        factionColor = data.factionColor;
        _characters = new List<ECS.Character>();
        _ownedLandmarks = new List<BaseLandmark>();
        _ownedRegions = new List<Region>();
        _relationships = new Dictionary<Faction, FactionRelationship>();
        _warmongering = 0;
        _landmarkInfo = new List<BaseLandmark>();
        _ownedAreas = new List<Area>();
    }

    #region virtuals
    /*
     Set the leader of this faction, change this per faction type if needed.
     This creates relationships between the leader and it's village heads by default.
         */
    public virtual void SetLeader(ILeader leader) {
        _leader = leader;
		//if(_leader != null){
		//	List<ECS.Character> villageHeads = GetCharactersOfType(CHARACTER_ROLE.VILLAGE_HEAD);
		//	for (int i = 0; i < villageHeads.Count; i++) {
		//		ECS.Character currHead = villageHeads[i];
		//		//leaders have relationships with their Village heads and vise versa.
		//		CharacterManager.Instance.CreateNewRelationshipBetween(leader, currHead);
		//	}
		//}
    }
    #endregion

    #region Settlements
    //public void AddSettlement(Settlement settlement) {
    //    if (!_settlements.Contains(settlement)) {
    //        _settlements.Add(settlement);
    //        RecalculateFactionSize();
    //        FactionManager.Instance.UpdateFactionOrderBy();
    //    }
    //}
    //public void RemoveSettlement(Settlement settlement) {
    //    _settlements.Remove(settlement);
    //    RecalculateFactionSize();
    //    FactionManager.Instance.UpdateFactionOrderBy();
    //}
    /*
     Recalculate the size of this faction given the 
     number of settlements it has.
         */
    //private void RecalculateFactionSize() {
    //    int settlementCount = settlements.Count;
    //    if (settlementCount < FactionManager.Instance.smallToMediumReq) {
    //        _factionSize = FACTION_SIZE.SMALL;
    //    } else if (settlementCount >= FactionManager.Instance.smallToMediumReq && settlementCount < FactionManager.Instance.mediumToLargeReq) {
    //        _factionSize = FACTION_SIZE.MEDIUM;
    //    } else if (settlementCount >= FactionManager.Instance.mediumToLargeReq) {
    //        _factionSize = FACTION_SIZE.LARGE;
    //    }
    //}
    #endregion

    #region Regions
    public void OwnRegion(Region region) {
        if (!_ownedRegions.Contains(region)) {
            _ownedRegions.Add(region);
        }
    }
    public void UnownRegion(Region region) {
        _ownedRegions.Remove(region);
    }
    #endregion

    #region Landmarks
    public void OwnLandmark(BaseLandmark landmark) {
        if (!_ownedLandmarks.Contains(landmark)) {
            _ownedLandmarks.Add(landmark);
        }
    }
    public void UnownLandmark(BaseLandmark landmark) {
        _ownedLandmarks.Remove(landmark);
    }
    #endregion

    #region Characters
    public void AddNewCharacter(ECS.Character character) {
        if (!_characters.Contains(character)) {
            _characters.Add(character);
            //FactionManager.Instance.UpdateFactionOrderBy();
        }
    }
    public void RemoveCharacter(ECS.Character character) {
        _characters.Remove(character);
        if (_leader != null && character.id == _leader.id) {
            SetLeader(null);
        }
        //FactionManager.Instance.UpdateFactionOrderBy();
    }
    public List<ECS.Character> GetCharactersOfType(CHARACTER_ROLE role) {
        List<ECS.Character> chars = new List<ECS.Character>();
        for (int i = 0; i < _characters.Count; i++) {
            ECS.Character currCharacter = _characters[i];
            if(currCharacter.role.roleType == role) {
                chars.Add(currCharacter);
            }
        }
        return chars;
    }
    #endregion

    #region Utilities
    public void SetName(string name) {
        _name = name;
    }
    //public List<Faction> GetMajorFactionsWithRelationshipStatus(List<RELATIONSHIP_STATUS> relStatuses) {
    //    List<Faction> factionsWithStatus = new List<Faction>();
    //    foreach (KeyValuePair<Faction, FactionRelationship> kvp in _relationships) {
    //        Faction currFaction = kvp.Key;
    //        FactionRelationship currRel = kvp.Value;
    //        if (currFaction.factionType == FACTION_TYPE.MAJOR && relStatuses.Contains(currRel.relationshipStatus)) {
    //            factionsWithStatus.Add(currFaction);
    //        }
    //    }
    //    return factionsWithStatus;
    //}
    //public List<Faction> GetMajorFactionsWithRelationshipStatus(RELATIONSHIP_STATUS relStatus) {
    //    List<Faction> factionsWithStatus = new List<Faction>();
    //    foreach (KeyValuePair<Faction, FactionRelationship> kvp in _relationships) {
    //        Faction currFaction = kvp.Key;
    //        FactionRelationship currRel = kvp.Value;
    //        if (currFaction.factionType == FACTION_TYPE.MAJOR && relStatus == currRel.relationshipStatus) {
    //            factionsWithStatus.Add(currFaction);
    //        }
    //    }
    //    return factionsWithStatus;
    //}
    public ECS.Character GetCharacterByID(int id) {
        for (int i = 0; i < _characters.Count; i++) {
            if (_characters[i].id == id) {
                return _characters[i];
            }
        }
        return null;
    }
 //   public Settlement GetSettlementWithHighestPopulation() {
 //       Settlement highestPopulationSettlement = null;
 //       for (int i = 0; i < _settlements.Count; i++) {
 //           Settlement settlement = _settlements[i];
 //           if (highestPopulationSettlement == null) {
 //               highestPopulationSettlement = settlement;
 //           } else {
 //               //if (settlement.civilians > highestPopulationSettlement.civilians) {
 //               //    highestPopulationSettlement = settlement;
 //               //}
 //           }
 //       }
 //       return highestPopulationSettlement;
	//}
	//public bool IsAtWar(){
	//	foreach (FactionRelationship factionRel in _relationships.Values) {
	//		if(factionRel.factionLookup[this._id].targetFaction.factionType == FACTION_TYPE.MAJOR && factionRel.isAtWar){
	//			return true;
	//		}else if(factionRel.factionLookup[this._id].targetFaction.factionType == FACTION_TYPE.MINOR && factionRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
	//			return true;
	//		}
	//	}
	//	return false;
	//}
    public bool IsHostileWith(Faction faction) {
        if(faction.id == this.id) {
            return false;
        }
        FactionRelationship rel = GetRelationshipWith(faction);
        return rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE;
    }
    public bool HasLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < _ownedLandmarks.Count; i++) {
            BaseLandmark currLandmark = _ownedLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return true;
            }
        }
        return false;
    }
    public bool HasAccessToLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < _ownedRegions.Count; i++) {
            Region currRegion = _ownedRegions[i];
            if (currRegion.HasLandmarkOfType(landmarkType)) {
                return true;
            }
        }
        return false;
    }
    public BaseLandmark GetAccessibleLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < _ownedRegions.Count; i++) {
            Region currRegion = _ownedRegions[i];
            if (currRegion.HasLandmarkOfType(landmarkType)) {
                return currRegion.GetLandmarksOfType(landmarkType).First();
            }
        }
        return null;
    }
    public override string ToString() {
        return name;
    }
    #endregion

    #region Relationships
    public void AddNewRelationship(Faction relWith, FactionRelationship relationship) {
        if (!_relationships.ContainsKey(relWith)) {
            _relationships.Add(relWith, relationship);
        } else {
            throw new System.Exception(this.name + " already has a relationship with " + relWith.name + ", but something is trying to create a new one!");
        }
    }
    public void RemoveRelationshipWith(Faction relWith) {
        if (_relationships.ContainsKey(relWith)) {
            _relationships.Remove(relWith);
        }
    }
    public FactionRelationship GetRelationshipWith(Faction faction) {
        if (_relationships.ContainsKey(faction)) {
            return _relationships[faction];
        }
        return null;
    }
    #endregion

    #region Death
    public void Death() {
        FactionManager.Instance.RemoveRelationshipsWith(this);
    }
    #endregion

    #region Landmarks
    ///*
    // This returns a list of all the owned landmarks
    // of this faction, this includes settlements as well.
    //     */
    //public List<BaseLandmark> GetAllOwnedLandmarks() {
    //    List<BaseLandmark> ownedLandmarks = new List<BaseLandmark>();
    //    for (int i = 0; i < _settlements.Count; i++) {
    //        Settlement currSettlement = _settlements[i];
    //        ownedLandmarks.Add(currSettlement);
    //        ownedLandmarks.AddRange(currSettlement.ownedLandmarks);
    //    }
    //    return ownedLandmarks;
    //}
    public BaseLandmark GetOwnedLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < _ownedLandmarks.Count; i++) {
            BaseLandmark currLandmark = _ownedLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return currLandmark;
            }
        }
        return null;
    }
    public void AddLandmarkInfo(BaseLandmark landmark) {
        if (!_landmarkInfo.Contains(landmark)) {
            _landmarkInfo.Add(landmark);
        }
    }
    public void RemoveLandmarkInfo(BaseLandmark landmark) {
        _landmarkInfo.Remove(landmark);
    }
    #endregion

    #region Areas
    public void OwnArea(Area area) {
        if (!_ownedAreas.Contains(area)) {
            _ownedAreas.Add(area);
        }
    }
    public void UnownArea(Area area) {
        _ownedAreas.Remove(area);
    }
    #endregion
}
