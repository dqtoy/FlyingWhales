using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;


public class Party {
    public delegate void DailyAction();
    public DailyAction onDailyAction;

    protected int _id;
    protected string _partyName;
    protected int _numOfAttackers;
    protected bool _isDead;
    protected bool _isAttacking;
    protected bool _isDefending;
    protected List<Character> _characters;
    protected Region _currentRegion;
    protected CharacterAvatar _icon;
    protected Faction _attackedByFaction;
    protected Combat _currentCombat;
    protected Area _specificLocation;
    protected Character _owner;
    protected List<Buff> _partyBuffs;
    protected int _maxCharacters;

    public EmblemBG emblemBG { get; private set; }
    public Sprite emblem { get; private set; }
    public Color partyColor { get; private set; }

    public List<string> specificLocationHistory { get; private set; } //limited to only 50 items

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public int numOfAttackers {
        get { return _numOfAttackers; }
        set { _numOfAttackers = value; }
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
    public string urlName {
        get {
            if (_characters.Count == 1) {
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
        get { return _characters.Sum(x => x.computedPower); }
    }
    public bool isDead {
        get { return _isDead; }
    }
    public bool isAttacking {
        get { return _isAttacking; }
    }
    public bool isDefending {
        get { return _isDefending; }
    }
    public MODE currentMode {
        get { return _characters[0].currentMode; }
    }
    public List<Character> characters {
        get { return _characters; }
    }
    public Faction attackedByFaction {
        get { return _attackedByFaction; }
        set { _attackedByFaction = value; }
    }
    public Faction faction {
        get { return owner.faction; }
    }
    public CharacterAvatar icon {
        get { return _icon; }
    }
    public Region currentRegion {
        get { return _currentRegion; }
    }
    //public Area home {
    //    get { return mainCharacter.home; }
    //}
    public Combat currentCombat {
        get { return _currentCombat; }
        set { _currentCombat = value; }
    }
    public Character mainCharacter {
        get { return _characters[0]; }
    }
    public Area specificLocation {
        get { return _specificLocation; }
    }
    //public List<CharacterQuestData> questData {
    //    get { return GetQuestData(); }
    //}
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
        _partyBuffs = new List<Buff>();
        specificLocationHistory = new List<string>();
        SetEmblemSettings(CharacterManager.Instance.GetRandomEmblemBG(),
                            CharacterManager.Instance.GetRandomEmblem(),
                            UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
        SetMaxCharacters(4);
#if !WORLD_CREATION_TOOL
        //Messenger.AddListener<ActionThread>(Signals.LOOK_FOR_ACTION, AdvertiseSelf);
        //Messenger.AddListener<BuildStructureQuestData>(Signals.BUILD_STRUCTURE_LOOK_ACTION, BuildStructureLookingForAction);

        //ConstructResourceInventory();
#endif
    }

    public void SetMaxCharacters(int max) {
        _maxCharacters = max;
    }

    #region Virtuals
    public virtual void CreateIcon() { }
    public virtual void ReturnToLife() {
        if (!_isDead) {
            return;
        }
        _isDead = false;

        CreateIcon();
        this.specificLocation.AddCharacterToLocation(this);
    }
    public virtual void PartyDeath() {
        if (_isDead) {
            return;
        }
        _isDead = true;
        //For now, when a party dies and there still members besides the owner of this party, kick them out of the party first before applying death
        if(_characters.Count > 1) {
            for (int i = 0; i < _characters.Count; i++) {
                if(_characters[i].id != _owner.id) {
                    RemoveCharacter(_characters[i]);
                    i--;
                }
            }
        }

        Area deathLocation = this.specificLocation;
        this.specificLocation.RemoveCharacterFromLocation(this);
        SetSpecificLocation(deathLocation); //set the specific location of this party, to the location it died at
        RemoveListeners();
        //DetachActionData();
        //ObjectState deadState = _icharacterObject.GetState("Dead");
        //_icharacterObject.ChangeState(deadState);
        GameObject.Destroy(_icon.gameObject);
        _icon = null;

        _currentCombat = null;

        //_owner.homeLandmark.RemoveAssaultArmyParty(this);
        Messenger.Broadcast<Party>(Signals.PARTY_DIED, this);
    }
    public void DisbandParty() {
        if(_characters.Count > 1) {
            for (int i = 0; i < _characters.Count; i++) {
                if (_characters[i] != _owner) {
                    RemoveCharacter(_characters[i]);
                    i--;
                }
            }
        }
    }
    public virtual void RemoveListeners() {
        //Messenger.RemoveListener<ActionThread>(Signals.LOOK_FOR_ACTION, AdvertiseSelf);
        //Messenger.RemoveListener<BuildStructureQuestData>(Signals.BUILD_STRUCTURE_LOOK_ACTION, BuildStructureLookingForAction);
    }
    #endregion

    #region Interface
    public void SetSpecificLocation(Area location) {
        _specificLocation = location;
        specificLocationHistory.Add("Set specific location to " + _specificLocation.ToString() 
            + " ST: " + StackTraceUtility.ExtractStackTrace());
        if (specificLocationHistory.Count >= 50) {
            specificLocationHistory.RemoveAt(0);
        }
        if (_specificLocation != null) {
            _currentRegion = _specificLocation.coreTile.region;
        }
    }
    public bool AddCharacter(Character character) {
        if (!isFull && !_characters.Contains(character)) {
            _characters.Add(character);
            character.SetCurrentParty(this);
            character.OnAddedToParty();
            ApplyCurrentBuffsToCharacter(character);
            Messenger.Broadcast(Signals.CHARACTER_JOINED_PARTY, character, this);
            return true;
        }
        return false;
    }
    public void RemoveCharacter(Character character) {
        if(_owner == character) {
            return;
        }
        if (_characters.Remove(character)) {
            character.OnRemovedFromParty();
            RemoveCurrentBuffsFromCharacter(character);
            character.ownParty.icon.transform.position = this.specificLocation.coreTile.transform.position;
            //if (this.specificLocation is BaseLandmark) {
            this.specificLocation.AddCharacterToLocation(character.ownParty);
            //} else {
            //    character.ownParty.SetSpecificLocation(this.specificLocation);
            //}
            Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY, character, this);

            ////Check if there are still characters in this party, if not, change to dead state
            //if (_characters.Count <= 0) {
            //    PartyDeath();
            //}
        }
    }
    public void GoHome(Action doneAction = null, Action actionOnStartOfMovement = null) {
        if (_isDead) { return; }
        GoToLocation(mainCharacter.homeArea, PATHFINDING_MODE.PASSABLE, doneAction, null, actionOnStartOfMovement);
    }
    public void GoHomeAndDisband(Action actionOnStartOfMovement = null) {
        if(_isDead) { return; }
        GoToLocation(mainCharacter.homeArea, PATHFINDING_MODE.PASSABLE, () => DisbandParty(), null, actionOnStartOfMovement);
    }
    #endregion

    #region Quests
    //private List<CharacterQuestData> GetQuestData() {
    //    if (_icharacters.Count > 0 && mainCharacter is Character) {
    //        return (mainCharacter as Character).questData;
    //    }
    //    return null;
    //}
    #endregion

    #region Utilities
    public void SetPartyName(string name) {
        _partyName = name;
    }
    public void SetEmblemSettings(EmblemBG emblemBG, Sprite emblem, Color partyColor) {
        this.emblemBG = emblemBG;
        this.emblem = emblem;
        this.partyColor = partyColor;
    }
    public void GoToLocation(Area targetLocation, PATHFINDING_MODE pathfindingMode, Action doneAction = null, Character trackTarget = null, Action actionOnStartOfMovement = null) {
        //if (_icon.isMovingToHex) {
        //    _icon.SetQueuedAction(() => GoToLocation(targetLocation, pathfindingMode, doneAction, trackTarget, actionOnStartOfMovement));
        //    return;
        //}
        if (_icon.isTravelling) {
            return;
        }
        if (specificLocation == targetLocation) {
            //action doer is already at the target location
            if (doneAction != null) {
                doneAction();
            }
        } else {
            //_icon.SetActionOnTargetReached(doneAction);
            _icon.SetTarget(targetLocation);
            _icon.StartPath(PATHFINDING_MODE.PASSABLE, doneAction, trackTarget, actionOnStartOfMovement);
        }
    }
    public void CancelTravel(Action onCancelTravel = null) {
        _icon.CancelTravel(onCancelTravel);
    }
    public void SetIsAttacking(bool state) {
        _isAttacking = state;
    }
    public void SetIsDefending(bool state) {
        _isDefending = state;
    }
    public Party GetBase() {
        return this;
    }
    public bool IsPartyBeingInspected() {
        for (int i = 0; i < _characters.Count; i++) {
            if (_characters[i].isBeingInspected) {
                return true;
            }
        }
        return false;
    }
    //public void GoToLocation(GameObject locationGO, PATHFINDING_MODE pathfindingMode, Action doneAction = null) {
    //    _icon.SetActionOnTargetReached(doneAction);
    //    _icon.SetTargetGO(locationGO);
    //}
    //public void BuildStructureLookingForAction(BuildStructureQuestData questData) {
    //    if(_currentRegion.id == questData.owner.party.currentRegion.id) {
    //        questData.AddToChoicesOfAllActionsThatCanObtainResource(_icharacterObject);
    //    }
    //}
    #endregion

    #region Berserk
    private void FindCombat(Party partyThatEntered, BaseLandmark landmark) {
        //if(partyThatEntered._specificLocation != null && this._specificLocation != null && this._specificLocation == partyThatEntered._specificLocation && partyThatEntered.id != this.id && this._currentCombat == null) {
        //    StartCombatWith(partyThatEntered);
        //}
    }
    #endregion

    #region Combat
    public Combat CreateCombatWith(Party enemy) {
        Combat combat = new Combat(this, enemy, _specificLocation);
        Debug.Log("Starting combat between " + enemy.name + " and  " + this.name);

        Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
        combatLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        combatLog.AddToFillers(null, " fought with ", LOG_IDENTIFIER.COMBAT);
        combatLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        for (int i = 0; i < enemy.characters.Count; i++) {
            enemy.characters[i].AddHistory(combatLog);
        }
        for (int i = 0; i < this.characters.Count; i++) {
            this.characters[i].AddHistory(combatLog);
        }
        return combat;
    }
    //public void StartCombatWith(Combat combat, Action afterCombatAction = null) {
    //    combat.AssignAfterCombatAction(afterCombatAction);
    //    combat.Fight();
    //    //return combat;
    //}
    public void JoinCombatWith(Party friend) {
        if (friend.currentCombat != null) {
            //if (this is CharacterParty) {
            //    (this as CharacterParty).actionData.SetIsHalted(true);
            //}
            //friend.currentCombat.AddParty(friend.mainCharacter.currentSide, this);

            Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "join_combat");
            combatLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            combatLog.AddToFillers(friend.currentCombat, " joins battle of ", LOG_IDENTIFIER.COMBAT);
            combatLog.AddToFillers(friend, friend.name, LOG_IDENTIFIER.TARGET_CHARACTER);

            for (int i = 0; i < this.characters.Count; i++) {
                this.characters[i].AddHistory(combatLog);
            }
            for (int i = 0; i < friend.characters.Count; i++) {
                friend.characters[i].AddHistory(combatLog);
            }
        }
    }
    #endregion

    #region Buffs
    public void AddBuff(Buff buff) {
        _partyBuffs.Add(buff);
        ApplyBuffToPartyMembers(characters, buff);
    }
    public void RemoveBuff(Buff buff) {
        if (_partyBuffs.Contains(buff)) {
            _partyBuffs.Remove(buff);
            RemoveBuffFromPartyMembers(characters, buff);
        }
    }
    private void ApplyBuffToPartyMembers(List<Character> characters, Buff buff) {
        for (int i = 0; i < characters.Count; i++) {
            ApplyBuffToPartyMember(characters[i], buff);
        }
    }
    private void ApplyBuffToPartyMember(Character member, Buff buff) {
        member.AddBuff(buff);
    }
    private void RemoveBuffFromPartyMembers(List<Character> characters, Buff buff) {
        for (int i = 0; i < characters.Count; i++) {
            RemoveBuffFromPartyMember(characters[i], buff);
        }
    }
    private void RemoveBuffFromPartyMember(Character member, Buff buff) {
        member.RemoveBuff(buff);
    }
    private void ApplyCurrentBuffsToCharacter(Character character) {
        for (int i = 0; i < _partyBuffs.Count; i++) {
            ApplyBuffToPartyMember(character, _partyBuffs[i]);
        }
    }
    private void RemoveCurrentBuffsFromCharacter(Character character) {
        for (int i = 0; i < _partyBuffs.Count; i++) {
            RemoveBuffFromPartyMember(character, _partyBuffs[i]);
        }
    }
    #endregion
}
