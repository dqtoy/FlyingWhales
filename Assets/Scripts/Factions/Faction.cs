﻿/*
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
    }
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
        }
    }
    public void RemoveCharacter(ECS.Character character) {
        _characters.Remove(character);
        if (_leader != null && character.id == _leader.id) {
            SetLeader(null);
        }
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
    public ECS.Character GetCharacterByID(int id) {
        for (int i = 0; i < _characters.Count; i++) {
            if (_characters[i].id == id) {
                return _characters[i];
            }
        }
        return null;
    }
    public bool IsHostileWith(Faction faction) {
        if(faction.id == this.id) {
            return false;
        }
        FactionRelationship rel = GetRelationshipWith(faction);
        return rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE;
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
