using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using ECS;

public class NewParty : IParty {
    protected int _id;
    protected int _numOfAttackers;
    protected bool _isDead;
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
    public virtual string name {
        get { return mainCharacter.name + "'s Party"; }
    }
    public string urlName {
        get {
            if (_icharacters.Count == 1) {
                return "<link=" + '"' + mainCharacter.id.ToString() + "_character" + '"' + ">" + name + "</link>";
            } else {
                return "<link=" + '"' + this._id.ToString() + "_party" + '"' + ">" + name + "</link>";
            }
        }
    }
    public string coloredUrlName {
        get { return "<link=" + '"' + this._id.ToString() + "_party" + '"' + ">" + "<color=#000000>" + name + "</color></link>"; }
    }
    public float computedPower {
        get { return _icharacters.Sum(x => x.computedPower); }
    }
    public bool isDead {
        get { return _isDead; }
    }
    public List<ICharacter> icharacters {
        get { return _icharacters; }
    }
    public Faction attackedByFaction {
        get { return _attackedByFaction; }
        set { _attackedByFaction = value; }
    }
    public Faction faction {
        get { return mainCharacter.faction; }
    }
    public CharacterIcon icon {
        get { return _icon; }
    }
    public Region currentRegion {
        get { return _currentRegion; }
    }
    public Area home {
        get { return mainCharacter.home; }
    }
    public StructureObj homeStructure {
        get { return mainCharacter.homeStructure; }
    }
    public Combat currentCombat {
        get { return _currentCombat; }
        set { _currentCombat = value; }
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
        _isDead = false;
        _icharacters = new List<ICharacter>();
#if !WORLD_CREATION_TOOL
        Messenger.AddListener<ActionThread>(Signals.LOOK_FOR_ACTION, AdvertiseSelf);
        Messenger.AddListener<BuildStructureQuestData>(Signals.BUILD_STRUCTURE_LOOK_ACTION, BuildStructureLookingForAction);

        //ConstructResourceInventory();
#endif
    }

    #region Virtuals
    public virtual void CreateIcon() { }
    public virtual void PartyDeath() {
        _isDead = true;
        this.specificLocation.RemoveCharacterFromLocation(this);
        Messenger.RemoveListener<ActionThread>(Signals.LOOK_FOR_ACTION, AdvertiseSelf);
        Messenger.RemoveListener<BuildStructureQuestData>(Signals.BUILD_STRUCTURE_LOOK_ACTION, BuildStructureLookingForAction);
        ObjectState deadState = _icharacterObject.GetState("Dead");
        _icharacterObject.ChangeState(deadState);
        GameObject.Destroy(_icon.gameObject);
        _icon = null;

        _currentCombat = null;
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
                    LandmarkVisual landmarkObject = collide.gameObject.GetComponent<LandmarkVisual>();
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
            icharacter.SetCurrentParty(this);
            Messenger.Broadcast(Signals.CHARACTER_JOINED_PARTY, icharacter, this);
        }
    }
    public void RemoveCharacter(ICharacter icharacter) {
        if (_icharacters.Remove(icharacter)) {
            icharacter.OnRemovedFromParty();
            //Check if there are still characters in this party, if not, change to dead state
            if (_icharacters.Count <= 0) {
                PartyDeath();
            }
        }
    }
    public void GoHome() {
        GoToLocation(mainCharacter.homeStructure.objectLocation, PATHFINDING_MODE.USE_ROADS);
    }
    #endregion

    #region Quests
    private List<CharacterQuestData> GetQuestData() {
        if (_icharacters.Count > 0 && mainCharacter is ECS.Character) {
            return (mainCharacter as ECS.Character).questData;
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
    public void BuildStructureLookingForAction(BuildStructureQuestData questData) {
        if(_currentRegion.id == questData.owner.party.currentRegion.id) {
            questData.AddToChoicesOfAllActionsThatCanObtainResource(_icharacterObject);
        }
    }
    #endregion

    #region Berserk
    public void BerserkModeOn() {
        Messenger.AddListener<NewParty>(Signals.PARTY_ENTERED_LANDMARK, FindCombat);
    }
    public void BerserkModeOff() {
        Messenger.RemoveListener<NewParty>(Signals.PARTY_ENTERED_LANDMARK, FindCombat);
    }
    private void FindCombat(NewParty partyThatEntered) {
        if(partyThatEntered._specificLocation != null && this._specificLocation != null && this._specificLocation == partyThatEntered._specificLocation && this._currentCombat == null) {
            StartCombatWith(partyThatEntered);
        }
    }
    #endregion

    #region Combat
    public void StartCombatWith(NewParty enemy) {
        if(enemy is CharacterParty) {
            (enemy as CharacterParty).actionData.SetIsHalted(true);
        }
        if (this is CharacterParty) {
            (this as CharacterParty).actionData.SetIsHalted(true);
        }
        //If attack target is not yet in combat, start new combat, else, join the combat on the opposing side
        Combat combat = this.currentCombat;
        if (combat == null) {
            combat = new Combat();
            combat.AddParty(SIDES.A, enemy);
            combat.AddParty(SIDES.B, this);
            //MultiThreadPool.Instance.AddToThreadPool(combat);
            Debug.Log("Starting combat between " + enemy.name + " and  " + this.name);
            combat.CombatSimulation();
        } else {
            if (enemy.currentCombat != null && enemy.currentCombat == combat) {
                return;
            }
            SIDES sideToJoin = CombatManager.Instance.GetOppositeSide(this.mainCharacter.currentSide);
            combat.AddParty(sideToJoin, enemy);
        }

        Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
        combatLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        combatLog.AddToFillers(combat, " fought with ", LOG_IDENTIFIER.COMBAT);
        combatLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        for (int i = 0; i < enemy.icharacters.Count; i++) {
            enemy.icharacters[i].AddHistory(combatLog);
        }
        for (int i = 0; i < this.icharacters.Count; i++) {
            this.icharacters[i].AddHistory(combatLog);
        }
    }
    public void JoinCombatWith(NewParty friend) {
        if (friend.currentCombat != null) {
            if (this is CharacterParty) {
                (this as CharacterParty).actionData.SetIsHalted(true);
            }
            friend.currentCombat.AddParty(friend.mainCharacter.currentSide, this);

            Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "join_combat");
            combatLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            combatLog.AddToFillers(friend.currentCombat, " joins battle of ", LOG_IDENTIFIER.COMBAT);
            combatLog.AddToFillers(friend, friend.name, LOG_IDENTIFIER.TARGET_CHARACTER);

            for (int i = 0; i < this.icharacters.Count; i++) {
                this.icharacters[i].AddHistory(combatLog);
            }
            for (int i = 0; i < friend.icharacters.Count; i++) {
                friend.icharacters[i].AddHistory(combatLog);
            }
        }
    }
    #endregion
}
