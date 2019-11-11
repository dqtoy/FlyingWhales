using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Traits;

public class Party {
    protected int _id;
    protected string _partyName;
    protected int _numOfAttackers;
    protected bool _isDead;
    protected bool _isAttacking;
    protected bool _isDefending;
    protected List<Character> _characters;
    protected CharacterAvatar _icon;
    protected Faction _attackedByFaction;
    protected Combat _currentCombat;
    protected Area _specificLocation;
    protected Character _owner;
    protected int _maxCharacters;

    public EmblemBG emblemBG { get; private set; }
    public Sprite emblem { get; private set; }
    public Color partyColor { get; private set; }

    public List<string> specificLocationHistory { get; private set; } //limited to only 50 items

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public string partyName {
        get { return _partyName; }
    }
    public virtual string name {
        get {
            if (characters.Count > 1) {
                return _partyName;
            } else {
                return mainCharacter.name;
            }
        }
    }
    public float computedPower {
        get { return _characters.Sum(x => x.computedPower); }
    }
    public bool isDead {
        get { return _isDead; }
    }
    public List<Character> characters {
        get { return _characters; }
    }
    public CharacterAvatar icon {
        get { return _icon; }
    }
    public Character mainCharacter {
        get { return _characters[0]; }
    }
    public Area specificLocation {
        get { return _specificLocation; }
    }
    public virtual Character owner {
        get { return _owner; }
    }
    public virtual int currentDay {
        get { return 0; }
    }
    public COMBATANT_TYPE combatantType {
        get {
            if (characters.Count > 1) {
                return COMBATANT_TYPE.ARMY; //if the party consists of 2 or more characters, it is considered an army
            } else {
                return COMBATANT_TYPE.CHARACTER;
            }
        }
    }
    public int maxCharacters {
        get { return _maxCharacters; }
    }
    public bool isFull {
        get { return characters.Count >= maxCharacters; }
    }
    #endregion

    public Party(Character owner) {
        _owner = owner;
        if (owner != null) {
            _partyName = owner.name + "'s Party";
        }
        _id = Utilities.SetID(this);
        _isDead = false;
        _characters = new List<Character>();
        specificLocationHistory = new List<string>();
        SetMaxCharacters(4);
    }

    public void SetMaxCharacters(int max) {
        _maxCharacters = max;
    }

    #region Virtuals
    public virtual void CreateIcon() { }
    public virtual void ReturnToLife() {
        if (_isDead) {
            _isDead = false;
            CreateIcon();
            //this.specificLocation.AddCharacterToLocation(this);
        }
    }
    public virtual void PartyDeath() {
        if (_isDead) {
            return;
        }
        _isDead = true;
        //For now, when a party dies and there still members besides the owner of this party, kick them out of the party first before applying death
        RemoveAllOtherCharacters();

        Area deathLocation = this.specificLocation;
        LocationStructure deathStructure = owner.currentStructure;
        this.specificLocation?.RemoveCharacterFromLocation(this);
        SetSpecificLocation(deathLocation); //set the specific location of this party, to the location it died at
        owner.SetCurrentStructureLocation(deathStructure, false);
        RemoveListeners();
        if (_icon.party.owner.race == RACE.SKELETON) {
            GameObject.Destroy(_icon.gameObject);
            _icon = null;
        } else {
            _icon.gameObject.SetActive(false);
        }        

        _currentCombat = null;

        //Messenger.Broadcast<Party>(Signals.PARTY_DIED, this);
    }
    public virtual void RemoveListeners() { }
    #endregion

    #region Interface
    public void SetSpecificLocation(Area location) {
        if (_specificLocation == location) {
            return; //ignore change
        }
        _specificLocation = location;
        if (specificLocationHistory.Count >= 50) {
            specificLocationHistory.RemoveAt(0);
        }
    }
    public bool AddCharacter(Character character, bool isOwner = false) {
        if (!isFull && !_characters.Contains(character)) {
            _characters.Add(character);
            character.SetCurrentParty(this);
            character.OnAddedToParty(); //this will remove character from his/her location
            if (isOwner) {
                if (owner.specificLocation != null) {
                    owner.specificLocation.AddCharacterToLocation(character);
                }
            } else {
                //character.marker.pathfindingAI.ClearAllCurrentPathData();
                character.SetGridTileLocation(owner.gridTileLocation);
                character.SetCurrentStructureLocation(owner.currentStructure);
                character.marker.transform.SetParent(_owner.marker.visualsParent);
                character.marker.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                character.marker.visualsParent.eulerAngles = Vector3.zero;
                character.marker.transform.eulerAngles = Vector3.zero;
                character.marker.nameLbl.gameObject.SetActive(false);

                Plagued targetPlagued = character.traitContainer.GetNormalTrait("Plagued") as Plagued;
                if (targetPlagued != null) {
                    string plaguedSummary = owner.name + " carried a plagued character. Rolling for infection.";
                    int roll = UnityEngine.Random.Range(0, 100);
                    plaguedSummary += "\nRoll is: " + roll.ToString() + ", Chance is: " + targetPlagued.GetCarryInfectChance().ToString();
                    if (roll < targetPlagued.GetCarryInfectChance()) {
                        //carrier will be infected with plague
                        plaguedSummary += "\nWill infect " + owner.name + " with plague!";
                        if (owner.traitContainer.AddTrait(owner, "Plagued", character)) {
                            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_plague");
                            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                            log.AddLogToInvolvedObjects();
                        }
                    }
                    Debug.Log(GameManager.Instance.TodayLogString() + plaguedSummary);
                }

                

                //character.marker.PlayIdle();
            }
            Messenger.Broadcast(Signals.CHARACTER_JOINED_PARTY, character, this);
            return true;
        }
        return false;
    }
    public void RemoveCharacter(Character character, bool addToLocation = true, LocationGridTile dropLocation = null) {
        if(_owner == character) {
            return;
        }
        if (_characters.Remove(character)) {
            //LocationGridTile gridTile = _owner.gridTileLocation.GetNearestUnoccupiedTileFromThis();
            //_owner.specificLocation.AddCharacterToLocation(character);
            character.OnRemovedFromParty();
            if (dropLocation == null) {
                if (_owner.gridTileLocation.isOccupied) {
                    LocationGridTile chosenTile = _owner.gridTileLocation.GetRandomUnoccupiedNeighbor();
                    if (chosenTile != null) {
                        character.marker.PlaceMarkerAt(chosenTile, addToLocation);
                    } else {
                        Debug.LogWarning(GameManager.Instance.TodayLogString() + character.name + " is being dropped by " + _owner.name + " but there is no unoccupied neighbor tile including the tile he/she is standing on. Default behavior is to drop character on the tile he/she is standing on regardless if it is unoccupied or not.");
                        character.marker.PlaceMarkerAt(_owner.gridTileLocation, addToLocation);
                    }
                } else {
                    character.marker.PlaceMarkerAt(_owner.gridTileLocation, addToLocation);
                }
            } else {
                character.marker.PlaceMarkerAt(dropLocation, addToLocation);
            }

            character.marker.transform.eulerAngles = Vector3.zero;
            character.marker.nameLbl.gameObject.SetActive(true);

            character.ownParty.icon.transform.position = this.specificLocation.coreTile.transform.position;
            Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY, character, this);
        }
    }
    /// <summary>
    /// Remove every character from this party, except the owner.
    /// </summary>
    public void RemoveAllOtherCharacters() {
        if (_characters.Count > 1) {
            for (int i = 0; i < _characters.Count; i++) {
                if (_characters[i].id != _owner.id) {
                    RemoveCharacter(_characters[i]);
                    i--;
                }
            }
        }
    }
    #endregion

    #region Utilities
    public void SetPartyName(string name) {
        _partyName = name;
    }
    public void GoToLocation(Region targetLocation, PATHFINDING_MODE pathfindingMode, LocationStructure targetStructure = null,
        Action doneAction = null, Action actionOnStartOfMovement = null, IPointOfInterest targetPOI = null, LocationGridTile targetTile = null) {
        if (_icon.isTravelling && _icon.travelLine != null) {
            return;
        }
        if (specificLocation.region == targetLocation) {
            //action doer is already at the target location
            if (doneAction != null) {
                doneAction();
            }
        } else {
            //_icon.SetActionOnTargetReached(doneAction);
            LocationGridTile exitTile = owner.GetNearestUnoccupiedEdgeTileFromThis();
            owner.marker.GoTo(exitTile, () => MoveToAnotherArea(targetLocation, pathfindingMode, targetStructure, doneAction, actionOnStartOfMovement, targetPOI, targetTile));
        }
    }
    private void MoveToAnotherArea(Region targetLocation, PATHFINDING_MODE pathfindingMode, LocationStructure targetStructure = null,
        Action doneAction = null, Action actionOnStartOfMovement = null, IPointOfInterest targetPOI = null, LocationGridTile targetTile = null) {
        _icon.SetTarget(targetLocation, targetStructure, targetPOI, targetTile);
        _icon.StartPath(PATHFINDING_MODE.PASSABLE, doneAction, actionOnStartOfMovement);
    }
    #endregion
}
