using UnityEngine;
using System.Collections;

public class Tribe : Faction {

    private ECS.Character _successor;

    #region getters/setters
    public ECS.Character successor {
        get { return _successor; }
    }
    #endregion

    public Tribe(RACE race) : base(race, FACTION_TYPE.MAJOR) {

    }

    public void SetSuccessor(ECS.Character successor) {
        _successor = successor;
    }
}
