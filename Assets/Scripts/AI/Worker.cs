using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Worker : Entity {

    public Worker() : base(ENTITY_TYPE.WORKER){
        SetStartingHP(50, 50);
        SetRatios(new int[3] { 0, 0, 2 });
        SetFOV(3f);
        SetBaseSpeed(3f);
        SetStrengths(new List<ENTITY_TYPE>() { ENTITY_TYPE.NONE });
        SetWeaknesses(new List<ENTITY_TYPE>() { ENTITY_TYPE.MONSTER, ENTITY_TYPE.BANDIT });
        SetEntityColor(Color.white);
    }
}
