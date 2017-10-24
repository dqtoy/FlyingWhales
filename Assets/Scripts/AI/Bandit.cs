using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bandit : Entity {

    public Bandit() : base(ENTITY_TYPE.BANDIT){
        SetStartingHP(75, 75);
        SetRatios(new int[3] { 0, 0, 1 });
        SetFOV(5f);
        SetBaseSpeed(3.1f);
        SetStrengths(new List<ENTITY_TYPE>() { ENTITY_TYPE.MONSTER });
        SetWeaknesses(new List<ENTITY_TYPE>() { ENTITY_TYPE.GUARD });
        SetEntityColor(Color.black);
    }
}
