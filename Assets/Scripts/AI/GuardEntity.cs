using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuardEntity : Entity {

    public GuardEntity() : base(ENTITY_TYPE.GUARD){
        SetStartingHP(100, 100);
        SetRatios(new int[3] { 0, 0, 2 });
        SetFOV(3f);
        SetBaseSpeed(2.9f);
        SetStrengths(new List<ENTITY_TYPE>() { ENTITY_TYPE.BANDIT });
        SetWeaknesses(new List<ENTITY_TYPE>() { ENTITY_TYPE.MONSTER });
        SetEntityColor(Color.green);
    }
}
