using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Party: IEncounterable {

    //public delegate void OnPartyFull(Party party);
    //public OnPartyFull onPartyFull;

    protected string _name;

    protected bool _isOpen; //is this party open to new members?
    protected bool _isDisbanded;

    protected ECS.Character _partyLeader;
    protected List<ECS.Character> _partyMembers; //Contains all party members including the party leader
    protected List<ECS.Character> _partyMembersOnTheWay; //Party members that just joined, but are on the way to the party leaders location
    protected Quest _currentQuest;
    protected CharacterAvatar _avatar;

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
    public Quest currentQuest {
        get { return _currentQuest; }
    }
    public HexTile currLocation {
        get { return _partyLeader.currLocation; }
    }
    #endregion

	public Party(ECS.Character partyLeader, bool mustBeAddedToPartyList = true) {
		SetName (RandomNameGenerator.Instance.GetAllianceName ());
        _partyLeader = partyLeader;
        _partyMembers = new List<ECS.Character>();
        _partyMembersOnTheWay = new List<ECS.Character>();
        Debug.Log(partyLeader.name + " has created " + _name);

        AddPartyMember(_partyLeader);
		if(mustBeAddedToPartyList){
			PartyManager.Instance.AddParty(this);
		}
    }

	public void SetName(string name){
		_name = name;
	}
    #region Party Management
    /*
     Add a new party member.
         */
    public virtual void AddPartyMember(ECS.Character member) {
        if (!_partyMembers.Contains(member)) {
			CreateRelationshipsForNewMember(member);
            _partyMembers.Add(member);
            member.SetParty(this);
            member.SetCurrentQuest(_currentQuest);
            if(_avatar != null) {
                member.DestroyAvatar();
                _avatar.AddNewCharacter(member);
            }
            if (!IsCharacterLeaderOfParty(member)) {
                Debug.Log(member.name + " has joined the party of " + partyLeader.name);
                if(_currentQuest != null) {
                    _currentQuest.AddNewLog(member.name + " has joined the party of " + partyLeader.name);
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
        if(_currentQuest != null) {
            if (_currentQuest.onQuestInfoChanged != null) {
                _currentQuest.onQuestInfoChanged();
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
            Debug.Log(member.name + " has left the party of " + partyLeader.name);
            if (currentQuest != null) {
                currentQuest.AddNewLog(member.name + " has left the party");
            }
        }
        
        member.SetParty(null);
		member.SetCurrentQuest (null);
        //if (_partyMembers.Count < 2) {
        //    DisbandParty();
        //}
    }
    public void CheckLeavePartyAfterQuest() {
        //Check which party members will leave
        List<ECS.Character> charactersToLeave = new List<ECS.Character>();
        for (int i = 0; i < _partyMembers.Count; i++) {
            ECS.Character currMember = _partyMembers[i];
            if (!IsCharacterLeaderOfParty(currMember)) {
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
        if (_currentQuest != null && !_currentQuest.isDone) {
            _currentQuest.EndQuest(QUEST_RESULT.CANCEL); //Cancel Quest if party is currently on a quest
        }
		SetCurrentQuest (null);
        for (int i = 0; i < partyMembers.Count; i++) {
            ECS.Character currMember = partyMembers[i];
            currMember.SetParty(null);
            if (_avatar != null && currMember != partyLeader) {
                _avatar.RemoveCharacter(currMember);
            }
            currMember.GoToNearestNonHostileSettlement(() => currMember.OnReachNonHostileSettlementAfterQuest());
        }
    }
	public void JustDisbandParty() {
		_isDisbanded = true;
        Debug.Log("Disbanded " + this.name);
        PartyManager.Instance.RemoveParty(this);
		if (_currentQuest != null && !_currentQuest.isDone) {
			_currentQuest.EndQuest(QUEST_RESULT.CANCEL); //Cancel Quest if party is currently on a quest
        }
		SetCurrentQuest (null);

		while(_partyMembers.Count > 0) {
			ECS.Character currMember = _partyMembers[0];
			currMember.SetParty(null);
			RemovePartyMember(currMember);

			currMember.currLocation.AddCharacterOnTile(currMember);
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
                    if(this.isOpen && !this.isFull) {
						ECS.Character currCharacter = settlement.location.charactersOnTile[i];
                        Faction factionOfCurrCharacter = currCharacter.faction;
                        if(factionOfCurrCharacter.id != _partyLeader.faction.id) {
                            //the curr character is not of the same faction with the party leader
                            if(FactionManager.Instance.GetRelationshipBetween(factionOfCurrCharacter, _partyLeader.faction).relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                                //the curr character cannot join this party, because the faction of the party leader is in hostile relations with his/her faction
                                continue;
                            }
                        }
                        if (currCharacter.role.roleType == role) {
                            if (currCharacter.currentQuest is DoNothing && currCharacter.party == null) {
                                currCharacter.currentQuest.QuestCancel();
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
        if(_currentQuest.questResult == QUEST_RESULT.SUCCESS) {
            stayWeight += 100; //If Quest is a success, add 100 to Stay
        } else  if(_currentQuest.questResult == QUEST_RESULT.FAIL) {
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
     Set the current quest the party is on.
     This will also set the current quest of all
     the characters in the party.
         */
    public void SetCurrentQuest(Quest quest) {
        _currentQuest = quest;
        for (int i = 0; i < _partyMembers.Count; i++) {
            ECS.Character currMember = _partyMembers[i];
            currMember.SetCurrentQuest(quest);
        }
        if(quest == null) {
            Debug.Log("Set current quest of " + name + " to nothing");
        } else {
            Debug.Log("Set current quest of " + name + " to " + quest.questType.ToString());
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
    public void OnQuestEnd(QUEST_RESULT result) {
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
    private void AdjustRelationshipBasedOnQuestResult(QUEST_RESULT result) {
        switch (result) {
            case QUEST_RESULT.SUCCESS:
                AdjustPartyRelationships(5); //Succeeded in a Quest Together: +5 (cumulative)
                break;
            case QUEST_RESULT.FAIL:
                AdjustPartyRelationships(-5); //Failed in a Quest Together: -5 (cumulative)
                break;
            default:
                break;
        }
    }
    #endregion

    #region Utilities
    public void GoToNearestNonHostileSettlement(Action onReachSettlement) {
        //check first if the character is already at a non hostile settlement
        if (_avatar.currLocation.landmarkOnTile != null && _avatar.currLocation.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.CITY
            && _avatar.currLocation.landmarkOnTile.owner != null) {
            if (partyLeader.faction.id != _avatar.currLocation.landmarkOnTile.owner.id) { //the party is not at a tile owned by the faction of the party leader
                if (FactionManager.Instance.GetRelationshipBetween(partyLeader.faction, _avatar.currLocation.landmarkOnTile.owner).relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {
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
        allSettlements = allSettlements.OrderBy(x => Vector2.Distance(_avatar.currLocation.transform.position, x.location.transform.position)).ToList();
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
        FactionManager.Instance.RemoveQuest(currentQuest);
        if (_partyLeader.isDead) {
            //party leader is already dead!
            SetCurrentQuest(null);
            DisbandParty();
        } else {
            CheckLeavePartyAfterQuest();
            _partyLeader.DestroyAvatar();
            _partyLeader.DetermineAction();
        }
        //_currLocation.AddCharacterOnTile(this);
    }
    #endregion

    public virtual void StartEncounter(ECS.Character encounteredBy){ }
	public virtual void StartEncounter(Party encounteredBy){}
	public virtual void ReturnResults(object result){
	}
}
