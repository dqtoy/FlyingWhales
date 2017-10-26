using UnityEngine;
using System.Collections;

public class Necromancer : GameAgent {

    public Necromancer() : base(AGENT_CATEGORY.LIVING, AGENT_TYPE.NECROMANCER, MOVE_TYPE.GROUND) {
        _attackRange = 1f;
        _attackSpeed = 0.5f;
        _attackValue = 15;
        _visibilityRange = 4f;
        _movementSpeed = 3f;
        agentColor = Color.red;
        SetAllyTypes(new AGENT_TYPE[] { AGENT_TYPE.NECROMANCER });
        SetInitialHP(50, 50);
    }
}
