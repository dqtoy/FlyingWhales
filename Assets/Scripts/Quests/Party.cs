using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Party: IEncounterable, ICombatInitializer {

    //public delegate void OnPartyFull(Party party);
    //public OnPartyFull onPartyFull;

    protected string _name;

    protected bool _isOpen; //is this party open to new members?
    protected bool _isDisbanded;

    protected ECS.Character _partyLeader;
    protected List<ECS.Character> _partyMembers; //Contains all party members including the party leader
    protected List<ECS.Character> _partyMembersOnTheWay; //Party members that just joined, but are on the way to the party leaders location
	protected List<ECS.Character> _prisoners;

    protected CharacterTask _currentTask;
    protected CharacterAvatar _avatar;

	protected HexTile _currLocation;

	protected bool _isDefeated;

    private const int MAX_PARTY_MEMBERS = 5;

    #region getters/setters
    public string encounterName {
		get { return _name; }
    }
    public string name {
        get { return _name; }
    }
    public bool isFull {
        get { return partyMembers.Count + _partyMembersOnTheWay.Count >= MAX_PARTY_MEMBERS; }
    }
    public bool isOpen {
        get { return _isOpen; }
    }
    public bool isDisbanded {
        get { return _isDisbanded; }
    }
    public ECS.Character partyLeader {
        get { return _partyLeader; }
    }
    public List<ECS.Character> partyMembers {
        get { return _partyMembers; }
    }
    public CharacterTask currentTask {
        get { return _currentTask; }
	}
	public List<ECS.Character> prisoners {
		get { return _prisoners; }
	}
    public HexTile currLocation {
        get { return _currLocation; }
    }
	public bool isDefeated {
		get { return _isDefeated; }
	}
    #endregion

	public Party(ECS.Character partyLeader, bool mustBeAddedToPartyList = true) {
		SetName (RandomNameGenerator.Instance.GetAllianceName ());
        _partyLeader = partyLeader;
        _partyMembers = new List<ECS.Character>();
        _partyMembersOnTheWay = new List<ECS.Character>();
		_prisoners = new List<ECS.Character> ();
		_isDefeated = false;
        Debug.Log(partyLeader.name + " has created " + _name);
		partyLeader.currLocation.AddCharacterOnTile (this, false);

        AddPartyMember(_partyLeader);

		if(mustBeAddedToPartyList){
			PartyManager.Instance.AddParty(this);
		}
    }

	public void SetName(string name){
		_name = name;
	}
	public void SetLocation(HexTile hextile){
		_currLocation = hextile;
	}
	public void SetIsDefeated(bool state){
		_isDefeated = state;
		for (int i = 0; i < _partyMembers.Count; i++) {
			_partyMembers [i].SetIsDefeated (state);
		}
	}
    #region Party Management
    /*
     Add a new party member.
         */
    public virtual void AddPartyMember(ECS.Character member) {
        if (!_partyMembers.Contains(member)) {
			member.AddHistory ("Joined party: " + this._name + ".");
			CreateRelationshipsForNewMember(member);
            _partyMembers.Add(member);
            if(_avatar != null) {
                member.DestroyAvatar();
                _avatar.AddNewCharacter(member);
            }
			this._currLocation.RemoveCharacterOnTile (member);
            member.SetParty(this);
            member.SetCurrentTask(_currentTask);
            if (!IsCharacterLeaderOfParty(member)) {
                Debug.Log(member.name + " has joined the party of " + partyLeader.name);
                if(_currentTask != null && _currentTask.taskType == TASK_TYPE.QUEST) {
                    ((Quest)_currentTask).AddNewLog(member.name + " has joined the party of " + partyLeader.name);
                }
            }
        }
        //if (_partyMembers.Count >= MAX_PARTY_MEMBERS) {
        //    if (onPartyFull != null) {
        //        Debug.Log("Party " + _name + " is full!");
        //        //Party is now full
        //        onPartyFull(this);
        //    }
        //}
        if(_currentTask != null && _currentTask.taskType == TASK_TYPE.QUEST) {
            Quest currQuest = (Quest)_currentTask;
            if (currQuest.onTaskInfoChanged != null) {
                currQuest.onTaskInfoChanged();
            }
        }
    }
    public void AddPartyMemberAsOnTheWay(ECS.Character member) {
        _partyMembersOnTheWay.Add(member);
    }
    public void PartyMemberHasArrived(ECS.Character member) {
        _partyMembersOnTheWay.Remove(member);
    }
    /*
     Remove a character from this party.
         */
    public virtual void RemovePartyMember(ECS.Character member, bool forDeath = false) {
        _partyMembers.Remove(member);
        if(_avatar != null) {
            _avatar.RemoveCharacter(member);
        }
        if (!forDeath) {
			member.AddHistory ("Left party: " + this._name + ".");
			member.currLocation.AddCharacterOnTile(member, false);
            Debug.Log(member.name + " has left the party of " + partyLeader.name);
            if (currentTask != null && _currentTask.taskType == TASK_TYPE.QUEST) {
                ((Quest)currentTask).AddNewLog(member.name + " has left the party");
            }
        }
        
        member.SetParty(null);
		member.SetCurrentTask (null);
        if (_partyMembers.Count <= 0) {
			this._currLocation.RemoveCharacterOnTile(this);
        }
    }
	public void AddPrisoner(ECS.Character character){
		character.SetPrisoner (true, this);
		_prisoners.Add (character);
	}
	public void RemovePrisoner(ECS.Character character){
		_prisoners.Remove (character);
	}
    public void CheckLeavePartyAfterQuest() {
        //Check which party members will leave
        List<ECS.Character> charactersToLeave = new List<ECS.Character>();
        Faction factionOfLeader = _partyLeader.faction;
        for (int i = 0; i < _partyMembers.Count; i++) {
            ECS.Character currMember = _partyMembers[i];
            Faction factionOfMember = currMember.faction;
            if (!IsCharacterLeaderOfParty(currMember)) {
                if(factionOfMember != null && factionOfLeader.id != factionOfMember.id) {//if the faction of the member is different from the faction of the leader
                    FactionRelationship factionRel = FactionManager.Instance.GetRelationshipBetween(factionOfLeader, factionOfMember);
                    if(factionRel != null && factionRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                        //- if hostile, characters from both factions must leave party led by a character from the other faction after completing a quest
                        charactersToLeave.Add(currMember);
                        continue;
                    }
                }
                
                WeightedDictionary<PARTY_ACTION> partyActionWeights = GetPartyActionWeightsForCharacter(currMember);
                if (partyActionWeights.PickRandomElementGivenWeights() == PARTY_ACTION.LEAVE) {
                    charactersToLeave.Add(currMember);
                }
            }
        }

        for (int i = 0; i < charactersToLeave.Count; i++) {
            ECS.Character characterToLeave = charactersToLeave[i];
            RemovePartyMember(characterToLeave);
            characterToLeave.GoToNearestNonHostileSettlement(() => characterToLeave.OnReachNonHostileSettlementAfterQuest()); //Make the character that left, go home then decide a new action
        }
    }
    /*
     This will disband this party.
     All party members will set their party to null and this party
     will be removed from the Party Manager. Each member will now return to
     non hostile settlements and determine their action there.
         */
    public void DisbandParty() {
        _isDisbanded = true;
        Debug.Log("Disbanded " + this.name);
        PartyManager.Instance.RemoveParty(this);
        if (_currentTask != null && _currentTask.taskType == TASK_TYPE.QUEST) {
            Quest currQuest = (Quest)_currentTask;
            if (!currQuest.isDone) {
                currQuest.EndTask(TASK_RESULT.CANCEL); //Cancel Quest if party is currently on a quest
            }
        }
		SetCurrentTask (null);
		if(_partyLeader.isDead){
			while(_prisoners.Count > 0){
				_prisoners [0].Death ();
			}
		}else{
			while(_prisoners.Count > 0){
				_prisoners [0].TransferPrisoner (_partyLeader);
			}
		}
        for (int i = 0; i < partyMembers.Count; i++) {
            ECS.Character currMember = partyMembers[i];
            currMember.SetParty(null);
            if (_avatar != null && currMember != partyLeader) {
                _avatar.RemoveCharacter(currMember);
            }
			currMember.currLocation.AddCharacterOnTile(currMember, false);
            currMember.GoToNearestNonHostileSettlement(() => currMember.OnReachNonHostileSettlementAfterQuest());
        }
    }
	public void JustDisbandParty() {
		_isDisbanded = true;
        Debug.Log("Disbanded " + this.name);
        PartyManager.Instance.RemoveParty(this);
        if (_currentTask != null && _currentTask.taskType == TASK_TYPE.QUEST) {
            Quest currQuest = (Quest)_currentTask;
            if (!currQuest.isDone) {
                currQuest.EndTask(TASK_RESULT.CANCEL); //Cancel Quest if party is currently on a quest
            }
        }
        SetCurrentTask (null);

		if(_partyLeader.isDead){
			while(_prisoners.Count > 0){
				_prisoners [0].Death ();
			}
		}else{
			while(_prisoners.Count > 0){
				_prisoners [0].TransferPrisoner (_partyLeader);
			}
		}

		while(_partyMembers.Count > 0) {
			ECS.Character currMember = _partyMembers[0];
			currMember.SetParty(null);
			RemovePartyMember(currMember);
			currMember.DetermineAction();
		}


	}
    public bool AreAllPartyMembersPresent() {
        bool isPartyComplete = true;
        for (int i = 0; i < _partyMembers.Count; i++) {
            ECS.Character currMember = _partyMembers[i];
            if (currMember.currLocation.id != _partyLeader.currLocation.id) {
                isPartyComplete = false;
                break;
            }
        }
        if(_partyMembersOnTheWay.Count > 0) {
            isPartyComplete = false;
        }
        return isPartyComplete;
    }
    public void SetOpenStatus(bool isOpen) {
        _isOpen = isOpen;
        //Do Nothing adventurers within the same city will be informed whenever a new character is registering for a Party. They will have first choice to join the party.

    }
    public void InviteCharactersOnTile(CHARACTER_ROLE role, HexTile tile) {
        if(tile.landmarkOnTile != null) {
            if(tile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.CITY) {
                Settlement settlement = (Settlement)tile.landmarkOnTile;
                for (int i = 0; i < settlement.location.charactersOnTile.Count; i++) {
					if(this.isOpen && !this.isFull && settlement.location.charactersOnTile[i] is ECS.Character) {
						ECS.Character currCharacter = (ECS.Character)settlement.location.charactersOnTile[i];
                        Faction factionOfCurrCharacter = currCharacter.faction;
						if(factionOfCurrCharacter == null){
							//Unaligned characters are hostile by default
							continue;
						}
                        if(factionOfCurrCharacter.id != _partyLeader.faction.id) {
                            //the curr character is not of the same faction with the party leader
                            if(FactionManager.Instance.GetRelationshipBetween(factionOfCurrCharacter, _partyLeader.faction).relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                                //the curr character cannot join this party, because the faction of the party leader is in hostile relations with his/her faction
                                continue;
                            }
                        }
                        if (currCharacter.role.roleType == role) {
                            if (currCharacter.currentTask is DoNothing && currCharacter.party == null) {
                                currCharacter.currentTask.TaskCancel();
                                currCharacter.JoinParty(this);
                            }
                        }
                    }
                }
            }
        }
    }

    public WeightedDictionary<PARTY_ACTION> GetPartyActionWeightsForCharacter(ECS.Character member) {
        WeightedDictionary<PARTY_ACTION> partyActionWeights = new WeightedDictionary<PARTY_ACTION>();
        int stayWeight = 50; //Default value for Stay is 50
        int leaveWeight = 50; //Default value for Leave is 50
        if(_currentTask.taskResult == TASK_RESULT.SUCCESS) {
            stayWeight += 100; //If Quest is a success, add 100 to Stay
        } else  if(_currentTask.taskResult == TASK_RESULT.FAIL) {
            leaveWeight += 100; //If Quest is a failure, add 100 to Leave
        }
        if (member.HasStatusEffect(STATUS_EFFECT.INJURED)) {
            //If character is injured, add 100 to Leave
            leaveWeight += 100;
        }
        
        float memberMissingHP = (member.remainingHP * 100) - 100;
        if(memberMissingHP >= 1) {
            leaveWeight += 5 * (int)memberMissingHP; //Add 5 to Leave for every 1% HP below 100%
        }
        partyActionWeights.AddElement(PARTY_ACTION.STAY, stayWeight);
        partyActionWeights.AddElement(PARTY_ACTION.LEAVE, leaveWeight);
        return partyActionWeights;
    }
    #endregion

    #region Quest
    /*
     Set the current task the party is on.
     This will also set the current task of all
     the characters in the party.
         */
    public void SetCurrentTask(CharacterTask task) {
        _currentTask = task;
		SetIsDefeated (false);
        for (int i = 0; i < _partyMembers.Count; i++) {
            ECS.Character currMember = _partyMembers[i];
            currMember.SetCurrentTask(task);
        }
        if(task == null) {
            Debug.Log("Set current quest of " + name + " to nothing");
        } else {
            if(task.taskType == TASK_TYPE.QUEST) {
                Debug.Log("Set current quest of " + name + " to " + ((Quest)task).questType.ToString());
            } else {
                Debug.Log("Set current task of " + name + " to " + task.taskType.ToString());
            }
            
        }
        
    }
    /*
     Make the party leader decide the next action for the party.
         */
    public void DetermineNextAction() {
        _partyLeader.DetermineAction();
    }
    /*
     This is called when the quest assigned to this party ends.
         */
    public void OnQuestEnd(TASK_RESULT result) {
        AdjustRelationshipBasedOnQuestResult(result);
    }
    #endregion

    #region Character Avatar
    public void SetAvatar(CharacterAvatar avatar) {
        _avatar = avatar;
        for (int i = 0; i < _partyMembers.Count; i++) {
            ECS.Character currCharacter = _partyMembers[i];
            if(currCharacter.avatar != avatar) {
                currCharacter.DestroyAvatar();
            }
            avatar.AddNewCharacter(currCharacter);
        }
    }
    #endregion

    #region Utilities
    internal bool IsCharacterLeaderOfParty(ECS.Character character) {
        return character.id == _partyLeader.id;
    }
    #endregion

    #region Relationships
    public void CreateRelationshipsForNewMember(ECS.Character newMember) {
        for (int i = 0; i < _partyMembers.Count; i++) {
            ECS.Character currPartyMember = _partyMembers[i];
            if(newMember.GetRelationshipWith(currPartyMember) == null) {
                CharacterManager.Instance.CreateNewRelationshipBetween(currPartyMember, newMember);
            }
        }
    }
    /*
     Adjust the relationship of each party member with each other by an amount
         */
    public void AdjustPartyRelationships(int adjustment) {
        for (int i = 0; i < _partyMembers.Count; i++) {
            ECS.Character currPartyMember = _partyMembers[i];
            for (int j = 0; j < _partyMembers.Count; j++) {
                ECS.Character otherPartyMember = _partyMembers[j];
                if (currPartyMember.id != otherPartyMember.id) {
                    currPartyMember.GetRelationshipWith(otherPartyMember).AdjustValue(adjustment);
                }
            }
        }
    }
    private void AdjustRelationshipBasedOnQuestResult(TASK_RESULT result) {
        switch (result) {
            case TASK_RESULT.SUCCESS:
                AdjustPartyRelationships(5); //Succeeded in a Quest Together: +5 (cumulative)
                break;
            case TASK_RESULT.FAIL:
                AdjustPartyRelationships(-5); //Failed in a Quest Together: -5 (cumulative)
                break;
            default:
                break;
        }
    }
    #endregion

    #region Utilities
    public void GoBackToQuestGiver(TASK_RESULT taskResult) {
        if(currentTask == null || currentTask.taskType != TASK_TYPE.QUEST) {
            throw new Exception(this.name + " cannot go back to quest giver because the party has no quest!");
        }
        Quest currentQuest = (Quest)currentTask;
        _avatar.SetTarget(currentQuest.postedAt.location);
        _avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => currentQuest.TurnInQuest(taskResult));
    }
    public void GoToNearestNonHostileSettlement(Action onReachSettlement) {
        //check first if the character is already at a non hostile settlement
        if (currLocation.landmarkOnTile != null && currLocation.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.CITY
            && currLocation.landmarkOnTile.owner != null) {
            if (partyLeader.faction.id != currLocation.landmarkOnTile.owner.id) { //the party is not at a tile owned by the faction of the party leader
                if (FactionManager.Instance.GetRelationshipBetween(partyLeader.faction, currLocation.landmarkOnTile.owner).relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {
                    onReachSettlement();
                    return;
                }
            } else {
                onReachSettlement();
                return;
            }
        }
        //party is not on a non hostile settlement
        List<Settlement> allSettlements = new List<Settlement>();
        for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) { //Get all the occupied settlements
            Tribe currTribe = FactionManager.Instance.allTribes[i];
            if (partyLeader.faction.id == currTribe.id ||
                FactionManager.Instance.GetRelationshipBetween(partyLeader.faction, currTribe).relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {
                allSettlements.AddRange(currTribe.settlements);
            }
        }
        allSettlements = allSettlements.OrderBy(x => Vector2.Distance(currLocation.transform.position, x.location.transform.position)).ToList();
        //if (_avatar == null) {
        //    _partyLeader.CreateNewAvatar();
        //}
        _avatar.SetTarget(allSettlements[0].location);
        _avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => onReachSettlement());
    }
    /*
     This is the default action to be done when a 
     character returns to a non hostile settlement after a quest.
         */
    internal void OnReachNonHostileSettlementAfterQuest() {
        FactionManager.Instance.RemoveQuest((Quest)currentTask);
        if (_partyLeader.isDead) {
            //party leader is already dead!
            SetCurrentTask(null);
            DisbandParty();
        } else {
            CheckLeavePartyAfterQuest();
            _partyLeader.DestroyAvatar();
            _partyLeader.DetermineAction();
        }
        //_currLocation.AddCharacterOnTile(this);
    }
    public bool CanJoinParty(ECS.Character candidate) {
        if(isFull || !_isOpen) {
            return false; //cannot join party because it is already full or party is not open
        }
        Faction factionOfParty = _partyLeader.faction;
        Faction factionOfCandidate = candidate.faction;
        if(factionOfCandidate == null || factionOfParty == null) {
            return true; //one of the characters are factionless, allow join
        } else {
            if(factionOfCandidate.id == factionOfParty.id) {
                return true; //faction of party is the same as faction of candidate
            }
            FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(factionOfParty, factionOfCandidate);
            if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                return false; //candidate cannot join party, because faction of leader and his/her faction are in hostile relations
            }
            return true;
        }
    }
    #endregion

	public ECS.Character GetCharacterByID(int id){
		if(_partyLeader.id == id){
			return _partyLeader;
		}
		for (int i = 0; i < _partyMembers.Count; i++) {
			if (_partyMembers [i].id == id){
				return _partyMembers [i];
			}
		}
		return null;
	}

	public ECS.Character GetPrisonerByID(int id){
		for (int i = 0; i < _prisoners.Count; i++) {
			if (_prisoners [i].id == id){
				return _prisoners [i];
			}
		}
		return null;
	}

	#region Virtuals
    public virtual void StartEncounter(ECS.Character encounteredBy){ }
	public virtual void StartEncounter(Party encounteredBy){}
	public virtual void ReturnResults(object result){}
	public virtual bool InitializeCombat(){
		if(isDefeated){
			return false;
		}
		if(_partyLeader.faction == null){
			ICombatInitializer enemy = this.currLocation.GetCombatEnemy (this);
			if(enemy != null){
				ECS.CombatPrototype combat = new ECS.CombatPrototype (this, this.currLocation);
				combat.AddCharacters (ECS.SIDES.A, this._partyMembers);
				if(enemy is Party){
					combat.AddCharacters (ECS.SIDES.B, ((Party)enemy).partyMembers);
				}else{
					combat.AddCharacters (ECS.SIDES.B, new List<ECS.Character>(){((ECS.Character)enemy)});
				}
				this.currLocation.SetCurrentCombat (combat);
				CombatThreadPool.Instance.AddToThreadPool (combat);
				return true;
			}
			return false;
		}else{
			if(_partyLeader.role != null && _partyLeader.role.roleType == CHARACTER_ROLE.WARLORD){
				ICombatInitializer enemy = this.currLocation.GetCombatEnemy (this);
				if(enemy != null){
					ECS.CombatPrototype combat = new ECS.CombatPrototype (this, this.currLocation);
					combat.AddCharacters (ECS.SIDES.A, this._partyMembers);
					if(enemy is Party){
						combat.AddCharacters (ECS.SIDES.B, ((Party)enemy).partyMembers);
					}else{
						combat.AddCharacters (ECS.SIDES.B, new List<ECS.Character>(){((ECS.Character)enemy)});
					}
					this.currLocation.SetCurrentCombat (combat);
					CombatThreadPool.Instance.AddToThreadPool (combat);
					return true;
				}
				return false;
			}
			return false;
		}
	}
	public virtual bool CanBattleThis(ICombatInitializer combatInitializer){
		if(_partyLeader.faction == null){
			return true;
		}else{
			if(_partyLeader.role != null && _partyLeader.role.roleType == CHARACTER_ROLE.WARLORD){
				//Check here if the combatInitializer is hostile with this character, if yes, return true
				return true;
			}
			return false;
		}
	}
	public virtual void ReturnCombatResults(ECS.CombatPrototype combat){
		if(combat.winningSide == ECS.SIDES.A){
			//This is the winner
		}else{
			//Enemy won
		}
	}
	#endregion

}
