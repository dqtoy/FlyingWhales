using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterEntity : Entity {

    public MonsterEntity() : base(ENTITY_TYPE.MONSTER){
        SetStartingHP(110, 110);
        SetRatios(new int[3] { 0, 0, 1 });
        SetFOV(5f);
        SetBaseSpeed(2.9f);
        SetStrengths(new List<ENTITY_TYPE>() { ENTITY_TYPE.WORKER, ENTITY_TYPE.GUARD });
        SetWeaknesses(new List<ENTITY_TYPE>() { ENTITY_TYPE.BANDIT });
        SetEntityColor(Color.red);
    }
}
