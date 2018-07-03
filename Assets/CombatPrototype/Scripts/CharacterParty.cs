using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using ECS;

public class CharacterParty : IParty {
    private bool _isHalted;
    private int _numOfAttackers;
    private List<ICharacter> _characters;
    private Region _currentRegion;
    private CharacterIcon _icon;
    private Faction _attackedByFaction;
    private CharacterObj _characterObj;
    private Combat _currentCombat;
    private ActionData _actionData;
    private ILocation _specificLocation;

    #region getters/setters
    public bool isHalted {
        get { return _isHalted; }
    }
    public int numOfAttackers {
        get { return _numOfAttackers; }
        set { _numOfAttackers = value; }
    }
    public float computedPower {
        get { return _characters.Sum(x => x.computedPower); }
    }
    public List<ICharacter> icharacters {
        get { return _characters; }
    }
    public Faction attackedByFaction {
        get { return _attackedByFaction; }
        set { _attackedByFaction = value; }
    }
    public Faction faction {
        get { return _characters[0].faction; }
    }
    public Combat currentCombat {
        get { return _currentCombat; }
        set { _currentCombat = value; }
    }
    public CharacterIcon icon {
        get { return _icon; }
    }
    public Region currentRegion {
        get { return _currentRegion; }
    }
    public CharacterObj characterObject {
        get { return _characterObj; }
    }
    public ActionData actionData {
        get { return _actionData; }
    }
    public ICharacterObject icharacterObject {
        get { return _characterObj; }
    }
    public ILocation specificLocation {
        get { return GetSpecificLocation(); }
    }
    #endregion

    public CharacterParty() {
        _characters = new List<ICharacter>();
        _actionData = new ActionData(this);
        _isHalted = false;
#if !WORLD_CREATION_TOOL
        _characterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.CHARACTER, "CharacterObject") as CharacterObj;
        _characterObj.SetCharacter(this);
        Messenger.AddListener<ActionThread>("LookForAction", AdvertiseSelf);
        //ConstructResourceInventory();
#endif
    }

    #region Interface
    public void AdvertiseSelf(ActionThread actionThread) {
        if (actionThread.party != this && _currentRegion.id == actionThread.party.currentRegion.id) {
            actionThread.AddToChoices(_characterObj);
        }
    }
    private ILocation GetSpecificLocation() {
        if (_specificLocation != null) {
            return _specificLocation;
        } else {
            if (_icon != null) {
                Collider2D collide = Physics2D.OverlapCircle(icon.aiPath.transform.position, 0.1f, LayerMask.GetMask("Hextiles"));
                //Collider[] collide = Physics.OverlapSphere(icon.aiPath.transform.position, 5f);
                HexTile tile = collide.gameObject.GetComponent<HexTile>();
                if (tile != null) {
                    return tile;
                } else {
                    LandmarkObject landmarkObject = collide.gameObject.GetComponent<LandmarkObject>();
                    if (landmarkObject != null) {
                        return landmarkObject.landmark.tileLocation;
                    }
                }
            }
            return null;
        }
    }
    public List<string> specificLocationHistory = new List<string>();
    public void SetSpecificLocation(ILocation specificLocation) {
        string previousLocationString = string.Empty;
        string newLocationString = string.Empty;
        if (_specificLocation == null) {
            previousLocationString = "null";
        } else {
            previousLocationString = _specificLocation.ToString();
        }
        if (specificLocation == null) {
            newLocationString = "null";
        } else {
            newLocationString = specificLocation.ToString();
        }
        specificLocationHistory.Add("Specific Location was changed from " + previousLocationString + " to " + newLocationString + " ST: " + StackTraceUtility.ExtractStackTrace());
        _specificLocation = specificLocation;
        if (_specificLocation != null) {
            _currentRegion = _specificLocation.tileLocation.region;
        }
    }
    public void AddCharacter(ICharacter character) {
        if (!_characters.Contains(character)) {
            _characters.Add(character);
            character.SetParty(this);
        }
    }
    public void RemoveCharacter(ICharacter character) {
        if (_characters.Remove(character)) {
            character.SetParty(null);
            //Check if there are still characters in this party, if not, change to dead state
            if(_characters.Count <= 0) {
                PartyDeath();
            }
        }
    }
    public void GoHome() {
        GoToLocation(_characters[0].homeStructure.objectLocation, PATHFINDING_MODE.USE_ROADS);
    }
    #endregion

    #region Utilities
    public void GoToLocation(ILocation targetLocation, PATHFINDING_MODE pathfindingMode, Action doneAction = null) {
        if (specificLocation == targetLocation) {
            //action doer is already at the target location
            if (doneAction != null) {
                doneAction();
            }
        } else {
            _icon.SetActionOnTargetReached(doneAction);
            _icon.SetTarget(targetLocation);
        }
    }
    public void GoToLocation(GameObject locationGO, PATHFINDING_MODE pathfindingMode, Action doneAction = null) {
        _icon.SetActionOnTargetReached(doneAction);
        _icon.SetTargetGO(locationGO);
    }
    
    public void SetIsHalted(bool state) {
        if (_isHalted != state) {
            _isHalted = state;
            if (state) {
                _icon.aiPath.maxSpeed = 0f;
            }
        }
    }
    public void PartyDeath() {
        Messenger.RemoveListener<ActionThread>("LookForAction", AdvertiseSelf);
        ObjectState deadState = _characterObj.GetState("Dead");
        _characterObj.ChangeState(deadState);
        GameObject.Destroy(_icon.gameObject);
        _icon = null;
    }
    #endregion


    #region Icon
    /*
        Create a new icon for this character.
        Each character owns 1 icon.
            */
    public void CreateIcon() {
        GameObject characterIconGO = GameObject.Instantiate(CharacterManager.Instance.characterIconPrefab,
            Vector3.zero, Quaternion.identity, CharacterManager.Instance.characterIconsParent);
        _icon = characterIconGO.GetComponent<CharacterIcon>();
        _icon.SetCharacter(this);
        PathfindingManager.Instance.AddAgent(_icon.aiPath);
    }
    #endregion
}
