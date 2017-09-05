using UnityEngine;
using System.Collections;

public class MilitaryAllianceOfficer : Role {
    public MilitaryAllianceOfficer(Citizen citizen) : base(citizen) {

    }

    internal override void Initialize(GameEvent gameEvent) {
        base.Initialize(gameEvent);
        this.avatar.GetComponent<MilitaryAllianceOfficer>().Initialize(gameEvent);
    }
}
