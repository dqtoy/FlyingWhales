using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using ECS;

public class MonsterParty : IParty {
    private List<ICharacter> _monsters;
    private int _numOfAttackers;
    private Region _currentRegion;
    private CharacterIcon _icon;
    private Faction _attackedByFaction;
    private MonsterObj _monsterObj;
    private Combat _currentCombat;
    private Area _home;
    private ILocation _specificLocation;

    #region getters/setters
    public float computedPower {
        get { return _monsters.Sum(x => x.computedPower); }
    }
    public int numOfAttackers {
        get { return _numOfAttackers; }
        set { _numOfAttackers = value; }
    }
    public List<ICharacter> icharacters {
        get { return _monsters; }
    }
    public Faction attackedByFaction {
        get { return _attackedByFaction; }
        set { _attackedByFaction = value; }
    }
    public Faction faction {
        get { return _monsters[0].faction; }
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
    public MonsterObj monsterObj {
        get { return _monsterObj; }
    }
    public ICharacterObject icharacterObject {
        get { return _monsterObj; }
    }
    public ILocation specificLocation {
        get { return GetSpecificLocation(); }
    }
    #endregion

    public MonsterParty() {
        _monsters = new List<ICharacter>();
#if !WORLD_CREATION_TOOL
        _monsterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.MONSTER, "MonsterObject") as MonsterObj;
        _monsterObj.SetMonster(this);
        Messenger.AddListener<ActionThread>("LookForAction", AdvertiseSelf);
        //ConstructResourceInventory();
#endif
    }

    #region Interface
    public void AdvertiseSelf(ActionThread actionThread) {
        actionThread.AddToChoices(_monsterObj);
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
    public void AddCharacter(ICharacter monster) {
        if (!_monsters.Contains(monster)) {
            _monsters.Add(monster);
            monster.SetParty(this);
        }
    }
    public void RemoveCharacter(ICharacter monster) {
        if (_monsters.Remove(monster)) {
            monster.SetParty(null);
            //Check if there are still characters in this party, if not, change to dead state
            if (_monsters.Count <= 0) {
                PartyDeath();
            }
        }
    }
    public void GoHome() {
        GoToLocation(_monsters[0].homeStructure.objectLocation, PATHFINDING_MODE.USE_ROADS);
    }
    public void PartyDeath() {
        Messenger.RemoveListener<ActionThread>("LookForAction", AdvertiseSelf);
        ObjectState deadState = _monsterObj.GetState("Dead");
        _monsterObj.ChangeState(deadState);
        GameObject.Destroy(_icon.gameObject);
        _icon = null;
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


    #region Icon
    /*
        Create a new icon for this character.
        Each character owns 1 icon.
            */
    public void CreateIcon() {
        GameObject characterIconGO = GameObject.Instantiate(MonsterManager.Instance.monsterIconPrefab,
            Vector3.zero, Quaternion.identity, CharacterManager.Instance.characterIconsParent);
        _icon = characterIconGO.GetComponent<CharacterIcon>();
        _icon.SetCharacter(this);
        PathfindingManager.Instance.AddAgent(_icon.aiPath);
    }
    #endregion
}
