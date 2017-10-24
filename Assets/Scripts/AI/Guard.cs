using UnityEngine;
using System.Collections;

public class Guard : Agent {

    public Guard() : base(AGENT_CATEGORY.LIVING, AGENT_TYPE.GUARD, MOVE_TYPE.GROUND) {
        _attackRange = 0.5f;
        _attackSpeed = 0.5f;
        _attackValue = 10;
        _visibilityRange = 2f;
        _movementSpeed = 3f;
        agentColor = Color.green;
        SetAllyTypes(new AGENT_TYPE[]{AGENT_TYPE.GUARD, AGENT_TYPE.CITY});
        SetInitialHP(100, 100);
    }
}
