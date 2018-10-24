using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using ECS;

public class Party : IParty {
    public delegate void DailyAction();
    public DailyAction onDailyAction;

    protected int _id;
    protected int _numOfAttackers;
    protected bool _isDead;
    protected bool _isAttacking;
    protected bool _isDefending;
    protected List<ICharacter> _icharacters;
    protected Region _currentRegion;
    protected CharacterAvatar _icon;
    protected Faction _attackedByFaction;
    protected Combat _currentCombat;
    protected ILocation _specificLocation;
    protected ICharacterObject _icharacterObject;
    protected ICharacter _owner;
    protected List<Buff> _partyBuffs;

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public int numOfAttackers {
        get { return _numOfAttackers; }
        set { _numOfAttackers = value; }
    }
    public virtual string name {
        get {
            if (icharacters.Count > 1) {
                return mainCharacter.name + "'s Party";
            } else {
                return mainCharacter.name;
            }
        }
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
    public bool isAttacking {
        get { return _isAttacking; }
    }
    public bool isDefending {
        get { return _isDefending; }
    }
    public MODE currentMode {
        get { return _icharacters[0].currentMode; }
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
    public BaseLandmark landmarkLocation {
        get {
            if(_specificLocation != null && _specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                return _specificLocation as BaseLandmark;
            }
            return null;
        }
    }
    public BaseLandmark homeLandmark {
        get { return mainCharacter.homeLandmark; }
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
    //public List<CharacterQuestData> questData {
    //    get { return GetQuestData(); }
    //}
    public virtual ICharacter owner {
        get { return _owner; }
    }
    public virtual CharacterAction currentAction {
        get { return null; }
    }
    public virtual int currentDay {
        get { return 0; }
    }
    public virtual IActionData iactionData {
        get { return null; }
    }
    public COMBATANT_TYPE combatantType {
        get {
            if (icharacters.Count > 1) {
                return COMBATANT_TYPE.ARMY; //if the party consists of 2 or more characters, it is considered an army
            } else {
                return COMBATANT_TYPE.CHARACTER;
            }
        }
    }
    #endregion

    public Party(ICharacter owner) {
        _owner = owner;
        _id = Utilities.SetID(this);
        _isDead = false;
        _icharacters = new List<ICharacter>();
        _partyBuffs = new List<Buff>();
#if !WORLD_CREATION_TOOL
        Messenger.AddListener<ActionThread>(Signals.LOOK_FOR_ACTION, AdvertiseSelf);
        //Messenger.AddListener<BuildStructureQuestData>(Signals.BUILD_STRUCTURE_LOOK_ACTION, BuildStructureLookingForAction);

        //ConstructResourceInventory();
#endif
    }

    #region Virtuals
    public virtual void CreateIcon() { }
    public virtual void PartyDeath() {
        if (_isDead) {
            return;
        }
        _isDead = true;
        ILocation deathLocation = this.specificLocation;
        this.specificLocation.RemoveCharacterFromLocation(this);
        SetSpecificLocation(deathLocation); //set the specific location of this party, to the location it died at
        RemoveListeners();
        DetachActionData();
        ObjectState deadState = _icharacterObject.GetState("Dead");
        _icharacterObject.ChangeState(deadState);
        GameObject.Destroy(_icon.gameObject);
        _icon = null;

        _currentCombat = null;
    }
    //public virtual void DisbandParty() {
    //    while (icharacters.Count != 0) {
    //        RemoveCharacter(icharacters[0]);
    //    }
    //}
    public virtual void RemoveListeners() {
        Messenger.RemoveListener<ActionThread>(Signals.LOOK_FOR_ACTION, AdvertiseSelf);
        //Messenger.RemoveListener<BuildStructureQuestData>(Signals.BUILD_STRUCTURE_LOOK_ACTION, BuildStructureLookingForAction);
    }
    public virtual void EndAction() { }
    public virtual void DetachActionData() { }
    #endregion

    #region Interface
    public void AdvertiseSelf(ActionThread actionThread) {
        actionThread.AddToChoices(_icharacterObject);
    }
    private ILocation GetSpecificLocation() {
        return _specificLocation;
        //if (_specificLocation != null) {
        //    return _specificLocation;
        //} else {
        //    if (_icon != null) {
        //        Collider2D collide = Physics2D.OverlapCircle(icon.aiPath.transform.position, 0.1f, LayerMask.GetMask("Hextiles"));
        //        //Collider[] collide = Physics.OverlapSphere(icon.aiPath.transform.position, 5f);
        //        HexTile tile = collide.gameObject.GetComponent<HexTile>();
        //        if (tile != null) {
        //            return tile;
        //        } else {
        //            LandmarkVisual landmarkObject = collide.gameObject.GetComponent<LandmarkVisual>();
        //            if (landmarkObject != null) {
        //                return landmarkObject.landmark.tileLocation;
        //            }
        //        }
        //    }
        //    return null;
        //}
    }
    public void SetSpecificLocation(ILocation location) {
        _specificLocation = location;
        if (_specificLocation != null) {
            _currentRegion = _specificLocation.tileLocation.region;
        }
    }
    public bool AddCharacter(ICharacter icharacter) {
        if (icharacters.Count < 4 && !_icharacters.Contains(icharacter)) {
            _icharacters.Add(icharacter);
            icharacter.SetCurrentParty(this);
            icharacter.OnAddedToParty();
            ApplyCurrentBuffsToCharacter(icharacter);
            if (icharacter is ECS.Character) {
                Messenger.Broadcast(Signals.CHARACTER_JOINED_PARTY, icharacter, this);
            }
            return true;
        }
        return false;
    }
    public void RemoveCharacter(ICharacter icharacter) {
        //bool isCharacterMain = false;
        //if(mainCharacter == icharacter) {
        //    isCharacterMain = true;
        //}
        if(_owner == icharacter) {
            return;
        }
        if (_icharacters.Remove(icharacter)) {
            icharacter.OnRemovedFromParty();
            RemoveCurrentBuffsFromCharacter(icharacter);
            icharacter.ownParty.icon.transform.position = this.specificLocation.tileLocation.transform.position;
            if (this.specificLocation is BaseLandmark) {
                this.specificLocation.AddCharacterToLocation(icharacter.ownParty);
            } else {
                //icharacter.ownParty.icon.SetAIPathPosition(this.specificLocation.tileLocation.transform.position);
                icharacter.ownParty.SetSpecificLocation(this.specificLocation);
                icharacter.ownParty.icon.SetVisualState(true);
            }
            if (icharacter is ECS.Character) {
                Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY, icharacter, this);
                //if (isCharacterMain) {
                //    icharacter.ownParty.icon.SetAnimator(CharacterManager.Instance.GetAnimatorByRole(mainCharacter.role.roleType));
                //}
            }
            //Check if there are still characters in this party, if not, change to dead state
            if (_icharacters.Count <= 0) {
                PartyDeath();
            }
        }
    }
    public void GoHome(Action doneAction = null, Action actionOnStartOfMovement = null) {
        GoToLocation(mainCharacter.homeLandmark, PATHFINDING_MODE.PASSABLE, doneAction, null, actionOnStartOfMovement);
    }
    #endregion

    #region Quests
    //private List<CharacterQuestData> GetQuestData() {
    //    if (_icharacters.Count > 0 && mainCharacter is ECS.Character) {
    //        return (mainCharacter as ECS.Character).questData;
    //    }
    //    return null;
    //}
    #endregion

    #region Utilities
    public void GoToLocation(ILocation targetLocation, PATHFINDING_MODE pathfindingMode, Action doneAction = null, ICharacter trackTarget = null, Action actionOnStartOfMovement = null) {
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
    public void CancelTravel() {
        _icon.CancelTravel();
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
        for (int i = 0; i < _icharacters.Count; i++) {
            if (_icharacters[i].isBeingInspected) {
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
    public void BerserkModeOn() {
        Messenger.AddListener<Party, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, FindCombat);
    }
    public void BerserkModeOff() {
        Messenger.RemoveListener<Party, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, FindCombat);
    }
    private void FindCombat(Party partyThatEntered, BaseLandmark landmark) {
        if(partyThatEntered._specificLocation != null && this._specificLocation != null && this._specificLocation == partyThatEntered._specificLocation && partyThatEntered.id != this.id && this._currentCombat == null) {
            StartCombatWith(partyThatEntered);
        }
    }
    #endregion

    #region Combat
    public Combat StartCombatWith(Party enemy) {

        //if(enemy is CharacterParty) {
        //    (enemy as CharacterParty).actionData.SetIsHalted(true);
        //}
        //if (this is CharacterParty) {
        //    (this as CharacterParty).actionData.SetIsHalted(true);
        //}

        Combat combat = null;
        if (this.currentCombat != null) {
            //If this party has current combat
            if (enemy.currentCombat != null) {
                //If the enemy has current combat
                if (enemy.currentCombat == this.currentCombat) {
                    //If they are already fighting with each other, return their combat
                    return this.currentCombat;
                } else {
                    //If they are not fighting with each other, this should not happen at all cost, a party must not combat another party if they are both in different combats
                    return null;
                }
            } else {
                //If this party has current combat and enemy does not have, enemy will join this party's combat on the opposite side
                combat = this.currentCombat;
                SIDES sideToJoin = CombatManager.Instance.GetOppositeSide(this.mainCharacter.currentSide);
                this.currentCombat.AddParty(sideToJoin, enemy);
            }
        } else {
            //If this party doesn't have current combat
            if (enemy.currentCombat != null) {
                //If the enemy has current combat, this party will join the enemy's combat on the opposite side
                combat = enemy.currentCombat;
                SIDES sideToJoin = CombatManager.Instance.GetOppositeSide(enemy.mainCharacter.currentSide);
                enemy.currentCombat.AddParty(sideToJoin, this);
            } else {
                //If both this party and the enemy don't have a combat, they will start a new combat
                combat = new Combat();
                combat.AddParty(SIDES.A, enemy);
                combat.AddParty(SIDES.B, this);
                //MultiThreadPool.Instance.AddToThreadPool(combat);
                Debug.Log("Starting combat between " + enemy.name + " and  " + this.name);
                combat.CombatSimulation();
            }
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
        return combat;
    }
    public void JoinCombatWith(Party friend) {
        if (friend.currentCombat != null) {
            //if (this is CharacterParty) {
            //    (this as CharacterParty).actionData.SetIsHalted(true);
            //}
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

    #region Buffs
    public void AddBuff(Buff buff) {
        _partyBuffs.Add(buff);
        ApplyBuffToPartyMembers(icharacters, buff);
    }
    public void RemoveBuff(Buff buff) {
        if (_partyBuffs.Contains(buff)) {
            _partyBuffs.Remove(buff);
            RemoveBuffFromPartyMembers(icharacters, buff);
        }
    }
    private void ApplyBuffToPartyMembers(List<ICharacter> characters, Buff buff) {
        for (int i = 0; i < characters.Count; i++) {
            ApplyBuffToPartyMember(characters[i], buff);
        }
    }
    private void ApplyBuffToPartyMember(ICharacter member, Buff buff) {
        member.AddBuff(buff);
    }
    private void RemoveBuffFromPartyMembers(List<ICharacter> characters, Buff buff) {
        for (int i = 0; i < characters.Count; i++) {
            RemoveBuffFromPartyMember(characters[i], buff);
        }
    }
    private void RemoveBuffFromPartyMember(ICharacter member, Buff buff) {
        member.RemoveBuff(buff);
    }
    private void ApplyCurrentBuffsToCharacter(ICharacter character) {
        for (int i = 0; i < _partyBuffs.Count; i++) {
            ApplyBuffToPartyMember(character, _partyBuffs[i]);
        }
    }
    private void RemoveCurrentBuffsFromCharacter(ICharacter character) {
        for (int i = 0; i < _partyBuffs.Count; i++) {
            RemoveBuffFromPartyMember(character, _partyBuffs[i]);
        }
    }
    #endregion
}
