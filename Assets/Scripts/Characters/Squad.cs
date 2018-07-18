using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Squad {

    public int id { get; private set; }
    public string name { get; private set; }
    public ICharacter squadLeader { get; private set; }
    public List<ICharacter> squadMembers { get; private set; } //all members + leader

    #region getters/setters
    public List<ICharacter> squadFollowers {
        get {
            List<ICharacter> followers = new List<ICharacter>(squadMembers);
            followers.Remove(squadLeader);
            return followers;
        }
    }
    public float potentialPower {
        get { return squadMembers.Sum(x => x.computedPower); }
    }
    #endregion

    public Squad() {
        id = Utilities.SetID(this);
        SetName("Squad " + id.ToString());
    }

    public void SetName(string name) {
        this.name = name;
    }

    public void SetLeader(ICharacter leader) {
        squadLeader = leader;
    }
    public void AddMember(ICharacter member) {
        if (!squadMembers.Contains(member)) {
            squadMembers.Add(member);
        }
    }
    public void RemoveMember(ICharacter member) {
        squadMembers.Remove(member);
    }
}
