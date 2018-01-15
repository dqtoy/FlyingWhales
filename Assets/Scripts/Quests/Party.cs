using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Party {

    public delegate void OnPartyFull(Party party);
    public OnPartyFull onPartyFull;

    protected bool _isOpen; //is this party open to new members?

    protected ECS.Character _partyLeader;
    protected List<ECS.Character> _partyMembers; //Contains all party members including the party leader
    protected Quest _currentQuest;

    private const int MAX_PARTY_MEMBERS = 5;

    #region getters/setters
    public bool isFull {
        get { return partyMembers.Count >= MAX_PARTY_MEMBERS; }
    }
    public bool isOpen {
        get { return _isOpen; }
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
    #endregion

    public Party(ECS.Character partyLeader) {
        _partyLeader = partyLeader;
        _partyMembers = new List<ECS.Character>();
        AddPartyMember(_partyLeader);
        PartyManager.Instance.AddParty(this);
    }

    #region Party Management
    /*
     Add a new party member.
         */
    public void AddPartyMember(ECS.Character member) {
        if (!_partyMembers.Contains(member)) {
            _partyMembers.Add(member);
            member.SetParty(this);
            member.SetCurrentQuest(_currentQuest);
            Debug.Log(member.name + " has joined the party of " + partyLeader.name);
        }
        if (_partyMembers.Count >= MAX_PARTY_MEMBERS) {
            if (onPartyFull != null) {
                Debug.Log("The party of " + partyLeader.name + "is full!");
                //Party is now full
                onPartyFull(this);
            }
        }
    }
    /*
     Remove a character from this party.
         */
    public void RemovePartyMember(ECS.Character member) {
        _partyMembers.Remove(member);
        if(_partyMembers.Count < 2) {
            DisbandParty();
        }
    }
    public void DisbandParty() {
        for (int i = 0; i < partyMembers.Count; i++) {
            ECS.Character currMember = partyMembers[i];
            currMember.SetParty(null);
            //TODO: Cancel Quest if party is currently on a quest?
        }
        PartyManager.Instance.RemoveParty(this);
		if(!_currentQuest.isDone){
			_currentQuest.EndQuest (QUEST_RESULT.CANCEL);
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
        return isPartyComplete;
    }
    public void SetOpenStatus(bool isOpen) {
        _isOpen = isOpen;
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
        if(member.missingHP > 0.5f) {
            //If character HP is less than 50%, add 50 to Leave
            leaveWeight += 50;
        }
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
    }
    /*
     Make the party leader decide the next action for the party.
         */
    public void DetermineNextAction() {
        _partyLeader.DetermineAction();
    }
    #endregion
}
