using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Party {

    public delegate void OnPartyFull(Party party);
    public OnPartyFull onPartyFull;

    protected int _maxPartyMembers;

    protected ECS.Character _partyLeader;
    protected List<ECS.Character> _partyMembers; //Contains all party members including the party leader
    protected Quest _currentQuest;

    #region getters/setters
    public bool isFull {
        get { return partyMembers.Count >= _maxPartyMembers; }
    }
    public ECS.Character partyLeader {
        get { return _partyLeader; }
    }
    public List<ECS.Character> partyMembers {
        get { return _partyMembers; }
    }
    #endregion

    public Party(ECS.Character partyLeader, int maxPartyMembers) {
        _maxPartyMembers = maxPartyMembers;
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
        }
        if (_partyMembers.Count >= _maxPartyMembers) {
            if (onPartyFull != null) {
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
    }
    public void DisbandParty() {
        for (int i = 0; i < partyMembers.Count; i++) {
            ECS.Character currMember = partyMembers[i];
            currMember.SetParty(null);
            //TODO: Cancel Quest if party is currently on a quest?
        }
        PartyManager.Instance.RemoveParty(this);
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
    #endregion
}
