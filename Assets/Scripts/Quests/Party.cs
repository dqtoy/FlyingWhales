using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Party {

    public delegate void OnPartyFull(Party party);
    public OnPartyFull onPartyFull;

    protected int _maxPartyMembers;

    protected Character _partyLeader;
    protected List<Character> _partyMembers; //Contains all party members including the party leader

    #region getters/setters
    public Character partyLeader {
        get { return _partyLeader; }
    }
    public List<Character> partyMembers {
        get { return _partyMembers; }
    }
    #endregion

    public Party(Character partyLeader, int maxPartyMembers) {
        _maxPartyMembers = maxPartyMembers;
        _partyLeader = partyLeader;
        _partyMembers = new List<Character>();
        AddPartyMember(_partyLeader);
    }

    /*
     Add a new party member.
         */
    public void AddPartyMember(Character member) {
        if (!_partyMembers.Contains(member)) {
            _partyMembers.Add(member);
            member.SetParty(this);
        }
        if(_partyMembers.Count >= _maxPartyMembers) {
            //Party is now full
            onPartyFull(this);
        }
    }
    /*
     Remove a character from this party.
         */
    public void RemovePartyMember(Character member) {
        _partyMembers.Remove(member);
    }
}
