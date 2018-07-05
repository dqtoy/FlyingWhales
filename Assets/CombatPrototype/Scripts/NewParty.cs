using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using ECS;

public class NewParty : IParty {
    protected int _id;
    protected int _numOfAttackers;
    protected List<ICharacter> _icharacters;
    protected Region _currentRegion;
    protected CharacterIcon _icon;
    protected Faction _attackedByFaction;
    protected Combat _currentCombat;
    protected ILocation _specificLocation;
    protected ICharacterObject _icharacterObject;

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public int numOfAttackers {
        get { return _numOfAttackers; }
        set { _numOfAttackers = value; }
    }
    public string name {
        get { return _icharacters[0].name + "'s Party"; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_party" + '"' + ">" + name + "</link>"; }
    }
    public string coloredUrlName {
        get { return "<link=" + '"' + this._id.ToString() + "_party" + '"' + ">" + "<color=#000000>" + name + "</color></link>"; }
    }
    public float computedPower {
        get { return _icharacters.Sum(x => x.computedPower); }
    }
    public List<ICharacter> icharacters {
        get { return _icharacters; }
    }
    public Faction attackedByFaction {
        get { return _attackedByFaction; }
        set { _attackedByFaction = value; }
    }
    public Faction faction {
        get { return _icharacters[0].faction; }
    }
    public CharacterIcon icon {
        get { return _icon; }
    }
    public Region currentRegion {
        get { return _currentRegion; }
    }
    public Area home {
        get { return _icharacters[0].home; }
    }
    public StructureObj homeStructure {
        get { return _icharacters[0].homeStructure; }
    }
    public ICharacter mainCharacter {
        get { return _icharacters[0]; }
    }
    public ICharacterObject icharacterObject {
        get { return _icharacterObject; }
    }
    public ILocation specificLocation {
        get { return GetSpecificLocation(); }
    }
    public List<CharacterQuestData> questData {
        get { return GetQuestData(); }
    }
    #endregion

    public NewParty() {
        _id = Utilities.SetID(this);
        _icharacters = new List<ICharacter>();
#if !WORLD_CREATION_TOOL
        Messenger.AddListener<ActionThread>("LookForAction", AdvertiseSelf);
        //ConstructResourceInventory();
#endif
    }

    #region Virtuals
    public virtual void CreateIcon() { }
    public virtual void PartyDeath() {
        Messenger.RemoveListener<ActionThread>("LookForAction", AdvertiseSelf);
        ObjectState deadState = _icharacterObject.GetState("Dead");
        _icharacterObject.ChangeState(deadState);
        GameObject.Destroy(_icon.gameObject);
        _icon = null;
    }
    #endregion

    #region Interface
    public void AdvertiseSelf(ActionThread actionThread) {
        actionThread.AddToChoices(_icharacterObject);
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
    public void SetSpecificLocation(ILocation location) {
        _specificLocation = location;
        if (_specificLocation != null) {
            _currentRegion = _specificLocation.tileLocation.region;
        }
    }
    public void AddCharacter(ICharacter icharacter) {
        if (!_icharacters.Contains(icharacter)) {
            _icharacters.Add(icharacter);
            icharacter.SetParty(this);
        }
    }
    public void RemoveCharacter(ICharacter icharacter) {
        if (_icharacters.Remove(icharacter)) {
            icharacter.SetParty(null);
            //Check if there are still characters in this party, if not, change to dead state
            if (_icharacters.Count <= 0) {
                PartyDeath();
            }
        }
    }
    public void GoHome() {
        GoToLocation(_icharacters[0].homeStructure.objectLocation, PATHFINDING_MODE.USE_ROADS);
    }
    #endregion

    #region Quests
    private List<CharacterQuestData> GetQuestData() {
        if (_icharacters.Count > 0 && _icharacters[0] is ECS.Character) {
            return (_icharacters[0] as ECS.Character).questData;
        }
        return null;
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
    #endregion
}
