using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CityAgent : GameAgent {

    public CityAgent() : base(AGENT_CATEGORY.STRUCTURE, AGENT_TYPE.CITY, MOVE_TYPE.NONE) {
        _attackRange = 2f;
        _attackSpeed = 1f;
        _attackValue = 20;
        _visibilityRange = 5f;
        _movementSpeed = 0f;
        agentColor = Color.white;
        SetAllyTypes(new AGENT_TYPE[] { AGENT_TYPE.GUARD, AGENT_TYPE.CITY });
        SetInitialHP(500, 500);
    }

    #region overrides
    internal override AIBehaviour DetermineAction(List<GameAgent> threatsInRange, List<GameAgent> targetsInRange, List<GameAgent> alliesInRange, bool isPerformingAction) {
        if (isPerformingAction) {
            if(agentObj.currentAction == ACTION_TYPE.ATTACK) {
                GameAgent currTarget = ((AttackHostiles)agentObj.currentBehaviour).target;
                if(!threatsInRange.Contains(currTarget) && !targetsInRange.Contains(currTarget)) {
                    return _attackBehaviour;
                }
            }
        } else {
            if(threatsInRange.Count > 0 || targetsInRange.Count > 0) {
                return _attackBehaviour;
            }
        }
        return null;
    }
    #endregion
}
