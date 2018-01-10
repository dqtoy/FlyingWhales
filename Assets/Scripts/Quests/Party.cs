using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Party {

    protected int _maxPartyMembers;

    protected Character _partyLeader;
    protected List<Character> _partyMembers; //Contains all party members including the party leader

    public delegate void OnPartyFull(Party party);
    public OnPartyFull onPartyFull;

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
