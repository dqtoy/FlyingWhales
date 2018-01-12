using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Party {

    public delegate void OnPartyFull(Party party);
    public OnPartyFull onPartyFull;

    protected int _maxPartyMembers;

    protected ECS.Character _partyLeader;
    protected List<ECS.Character> _partyMembers; //Contains all party members including the party leader

    #region getters/setters
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
    }

    /*
     Add a new party member.
         */
    public void AddPartyMember(ECS.Character member) {
        if (!_partyMembers.Contains(member)) {
            _partyMembers.Add(member);
            member.SetParty(this);
        }
        if(_partyMembers.Count >= _maxPartyMembers) {
            if(onPartyFull != null) {
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
}
