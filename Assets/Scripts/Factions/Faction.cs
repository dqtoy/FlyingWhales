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
    protected string _description;
    protected int _level;
    protected int _currentInteractionTick;
    protected bool _isPlayerFaction;
    protected Race _race;
    protected ILeader _leader;
    protected Sprite _emblem;
    protected List<Region> _ownedRegions;
    protected List<BaseLandmark> _ownedLandmarks;
    protected Color _factionColor;
    protected List<Character> _characters; //List of characters that are part of the faction
    protected Dictionary<Faction, FactionRelationship> _relationships;
    protected List<BaseLandmark> _landmarkInfo;
    protected List<Area> _ownedAreas;
    protected INTERACTION_TYPE[] _nonNeutralInteractionTypes;
    protected INTERACTION_TYPE[] _neutralInteractionTypes;

    public MORALITY morality { get; private set; }
    public FactionIntel factionIntel { get; private set; }
    public Dictionary<Faction, int> favor { get; private set; }
    public WeightedDictionary<AreaCharacterClass> defenderWeights { get; private set; }

    #region getters/setters
    public int id {
		get { return _id; }
	}
    public int level {
        get { return _level; }
    }
    public string name {
		get { return _name; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_faction" + '"' + ">" + this._name + "</link>"; }
    }
    public string description {
        get { return _description; }
    }
    public bool isDestroyed {
        get { return _leader == null; }
    }
    public RACE raceType {
        get { return _race.race; }
    }
    public RACE_SUB_TYPE subRaceType {
        get { return _race.subType; }
    }
    public ILeader leader {
        get { return _leader; }
    }
    public Race race {
        get { return _race; }
    }
    public Sprite emblem {
        get { return _emblem; }
    }
    public Color factionColor {
        get { return _factionColor; }
    }
    public List<Character> characters {
        get { return _characters; }
    }
    public List<Region> ownedRegions {
        get { return _ownedRegions; }
    }
    public Dictionary<Faction, FactionRelationship> relationships {
        get { return _relationships; }
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

    public Faction(bool isPlayerFaction = false) {
        _isPlayerFaction = isPlayerFaction;
		this._id = Utilities.SetID<Faction>(this);
        SetName(RandomNameGenerator.Instance.GenerateKingdomName());
        SetEmblem(FactionManager.Instance.GenerateFactionEmblem(this));
        SetFactionColor (Utilities.GetColorForFaction());
        SetRace(new Race(RACE.HUMANS, RACE_SUB_TYPE.NORMAL));
        SetMorality(MORALITY.GOOD);
        _level = 1;
        _characters = new List<Character>();
        _ownedLandmarks = new List<BaseLandmark>();
        _ownedRegions = new List<Region>();
        _relationships = new Dictionary<Faction, FactionRelationship>();
        _landmarkInfo = new List<BaseLandmark>();
        _ownedAreas = new List<Area>();
        factionIntel = new FactionIntel(this);
        favor = new Dictionary<Faction, int>();
        defenderWeights = new WeightedDictionary<AreaCharacterClass>();
        InitializeInteractions();
        SetDailyInteractionGenerationTick();
#if !WORLD_CREATION_TOOL
        AddListeners();
#endif
    }

    public Faction(FactionSaveData data) {
        _id = Utilities.SetID(this, data.factionID);
        SetName(data.factionName);
        SetDescription(data.factionDescription);
        SetFactionColor(data.factionColor);
        SetEmblem(FactionManager.Instance.GetFactionEmblem(data.emblemIndex));
        SetMorality(data.morality);
        SetRace(data.race);
        SetLevel(data.level);
        _characters = new List<Character>();
        _ownedLandmarks = new List<BaseLandmark>();
        _ownedRegions = new List<Region>();
        _relationships = new Dictionary<Faction, FactionRelationship>();
        _landmarkInfo = new List<BaseLandmark>();
        _ownedAreas = new List<Area>();
        factionIntel = new FactionIntel(this);
        favor = new Dictionary<Faction, int>();
        if (data.defenderWeights != null) {
            defenderWeights = new WeightedDictionary<AreaCharacterClass>(data.defenderWeights);
        } else {
            defenderWeights = new WeightedDictionary<AreaCharacterClass>();
        }
        InitializeInteractions();
        SetDailyInteractionGenerationTick();
#if !WORLD_CREATION_TOOL
        AddListeners();
#endif
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
    public void AddNewCharacter(Character character) {
        if (!_characters.Contains(character)) {
            _characters.Add(character);
            character.SetFaction(this);
            Messenger.Broadcast(Signals.CHARACTER_ADDED_TO_FACTION, character, this);
        }
    }
    public void RemoveCharacter(Character character) {
        if (_characters.Remove(character)) {
            character.SetFaction(null);
            Messenger.Broadcast(Signals.CHARACTER_REMOVED_FROM_FACTION, character, this);
        }
        //if (_leader != null && character.id == _leader.id) {
        //    SetLeader(null);
        //}
    }
    public List<Character> GetCharactersOfType(CHARACTER_ROLE role) {
        List<Character> chars = new List<Character>();
        for (int i = 0; i < _characters.Count; i++) {
            Character currCharacter = _characters[i];
            if(currCharacter.role.roleType == role) {
                chars.Add(currCharacter);
            }
        }
        return chars;
    }
    #endregion

    #region Utilities
    private void AddListeners() {
        Messenger.AddListener<Character>(Signals.CHARACTER_REMOVED, RemoveCharacter);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        if (!_isPlayerFaction) {
            Messenger.AddListener(Signals.DAY_STARTED, DailyInteractionGeneration);
        }
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_REMOVED, RemoveCharacter);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        if (!_isPlayerFaction) {
            Messenger.RemoveListener(Signals.DAY_STARTED, DailyInteractionGeneration);
        }
    }
    public void SetRace(Race race) {
        _race = race;
    }
    public void SetRaceType(RACE race) {
        _race.race = race;
    }
    public void SetSubRaceType(RACE_SUB_TYPE race) {
        _race.subType = race;
    }
    public void SetFactionColor(Color color) {
        _factionColor = color;
    }
    public void SetName(string name) {
        _name = name;
    }
    public void SetDescription(string description) {
        _description = description;
    }
    public Character GetCharacterByID(int id) {
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
    private void OnCharacterDied(Character characterThatDied) {
        if (leader != null && leader is Character && leader.id == characterThatDied.id) {
            OnLeaderDied();
        }
    }
    private void OnLeaderDied() {
        Debug.Log(this.name + "'s leader died");
        SetLeader(null);
        //kill all characters in faction and un own all areas
        List<Character> remainingCharacters = new List<Character>(characters);
        for (int i = 0; i < remainingCharacters.Count; i++) {
            remainingCharacters[i].Death();
        }
        List<Area> areasToUnown = new List<Area>(ownedAreas);
        for (int i = 0; i < areasToUnown.Count; i++) {
            LandmarkManager.Instance.UnownArea(areasToUnown[i]);
        }
        Messenger.Broadcast(Signals.FACTION_LEADER_DIED, this);
    }
    public void SetLevel(int amount) {
        _level = amount;
    }
    public void LevelUp() {
        _level ++;
    }
    public void LevelUp(int amount) {
        _level += amount;
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
    public bool HasRelationshipStatus(FACTION_RELATIONSHIP_STATUS stat, bool excludePlayer = true) {
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (excludePlayer && kvp.Key.id == PlayerManager.Instance.player.playerFaction.id) {
                continue; //exclude player faction
            }
            if (kvp.Value.relationshipStatus == stat) {
                return true;
            }
        }
        return false;
    }
    public Faction GetFactionWithRelationship(FACTION_RELATIONSHIP_STATUS stat, bool excludePlayer = true) {
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (excludePlayer && kvp.Key.id == PlayerManager.Instance.player.playerFaction.id) {
                continue; //exclude player faction
            }
            if (kvp.Value.relationshipStatus == stat) {
                return kvp.Key;
            }
        }
        return null;
    }
    public List<Faction> GetFactionsWithRelationship(FACTION_RELATIONSHIP_STATUS stat, bool excludePlayer = true) {
        List<Faction> factions = new List<Faction>();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (excludePlayer && kvp.Key.id == PlayerManager.Instance.player.playerFaction.id) {
                continue; //exclude player faction
            }
            if (kvp.Value.relationshipStatus == stat) {
                factions.Add(kvp.Key);
            }
        }
        return factions;
    }
    #endregion

    #region Death
    public void Death() {
        RemoveListeners();
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

    #region Emblems
    public void SetEmblem(Sprite sprite) {
        _emblem = sprite;
    }
    #endregion

    #region Morality
    public void SetMorality(MORALITY morality) {
        this.morality = morality;
    }
    #endregion

    #region Favor
    public void AddNewFactionFavor(Faction faction, int value = 0) {
        if (favor.ContainsKey(faction)) {
            favor[faction] = value;
        } else {
            favor.Add(faction, value);
        }
    }
    public void AdjustFavorFor(Faction otherFaction, int adjustment) {
        if (favor.ContainsKey(otherFaction)) {
            favor[otherFaction] += adjustment;
        } else {
            Debug.LogWarning("There is no favor key for " + otherFaction.name + " in " + this.name + "'s favor dictionary");
        }
    }
    #endregion

    #region Interaction
    private void InitializeInteractions() {
        _nonNeutralInteractionTypes = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.SPAWN_CHARACTER,
            //Defense Mobilization
            //Move to Attack
            //Defense Upgrade
        };
        _neutralInteractionTypes = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.SPAWN_NEUTRAL_CHARACTER,
        };
    }
    private void SetDailyInteractionGenerationTick() {
        _currentInteractionTick = UnityEngine.Random.Range(1, GameManager.hoursPerDay + 1);
    }
    private void DailyInteractionGeneration() {
        if (_currentInteractionTick == GameManager.Instance.days) {
            GenerateDailyInteraction();
            SetDailyInteractionGenerationTick();
        }
    }
    private void GenerateDailyInteraction() {
        if(name == "Neutral") {
            GenerateNeutralInteraction();
        } else {
            GenerateNonNeutralInteraction();
        }
    }
    private void GenerateNonNeutralInteraction() {
        string interactionLog = GameManager.Instance.TodayLogString() + "Generating non neutral interaction for " + this.name;
        List<InteractionAndInteractable> interactionCandidates = new List<InteractionAndInteractable>();
        for (int i = 0; i < _ownedAreas.Count; i++) {
            Area area = _ownedAreas[i];
            if(area.suppliesInBank >= 100) {
                for (int j = 0; j < _nonNeutralInteractionTypes.Length; j++) {
                    INTERACTION_TYPE type = _nonNeutralInteractionTypes[j];
                    if (InteractionManager.Instance.CanCreateInteraction(type, area.coreTile.landmarkOnTile)) {
                        InteractionAndInteractable candidate = new InteractionAndInteractable {
                            interactionType = type,
                            landmark = area.coreTile.landmarkOnTile,
                        };
                        interactionCandidates.Add(candidate);
                    }
                }
            }
        }
        if(interactionCandidates.Count > 0) {
            int chosenIndex = UnityEngine.Random.Range(0, interactionCandidates.Count);
            Interaction createdInteraction = InteractionManager.Instance.CreateNewInteraction(interactionCandidates[chosenIndex].interactionType, interactionCandidates[chosenIndex].landmark);
            interactionCandidates[chosenIndex].landmark.AddInteraction(createdInteraction);
            interactionLog += "\nCreated " + createdInteraction.type.ToString() + " on " + createdInteraction.interactable.tileLocation.areaOfTile.name;
            Debug.Log(interactionLog);
        } else {
            interactionLog += "\nCannot create interaction because all interactions do not meet the requirements";
            Debug.Log(interactionLog);
        }
    }
    private void GenerateNeutralInteraction() {
        string interactionLog = GameManager.Instance.TodayLogString() + "Generating neutral interaction for " + this.name;
        List<InteractionAndInteractable> interactionCandidates = new List<InteractionAndInteractable>();
        for (int i = 0; i < _ownedAreas.Count; i++) {
            Area area = _ownedAreas[i];
            for (int j = 0; j < _neutralInteractionTypes.Length; j++) {
                INTERACTION_TYPE type = _neutralInteractionTypes[j];
                if (InteractionManager.Instance.CanCreateInteraction(type, area.coreTile.landmarkOnTile)) {
                    InteractionAndInteractable candidate = new InteractionAndInteractable {
                        interactionType = type,
                        landmark = area.coreTile.landmarkOnTile,
                    };
                    interactionCandidates.Add(candidate);
                }
            }
        }
        if (interactionCandidates.Count > 0) {
            int chosenIndex = UnityEngine.Random.Range(0, interactionCandidates.Count);
            Interaction createdInteraction = InteractionManager.Instance.CreateNewInteraction(interactionCandidates[chosenIndex].interactionType, interactionCandidates[chosenIndex].landmark);
            interactionCandidates[chosenIndex].landmark.AddInteraction(createdInteraction);
            interactionLog += "\nCreated " + createdInteraction.type.ToString() + " on " + createdInteraction.interactable.tileLocation.areaOfTile.name;
            Debug.Log(interactionLog);
        } else {
            interactionLog += "\nCannot create interaction because all interactions do not meet the requirements";
            Debug.Log(interactionLog);
        }
    }
    #endregion
}
