using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Guard : GameAgent {

    private City _guardingCity;

    public Guard(City guardingCity) : base(AGENT_CATEGORY.LIVING, AGENT_TYPE.GUARD, MOVE_TYPE.GROUND) {
        _guardingCity = guardingCity;
        _attackRange = 0.5f;
        _attackSpeed = 0.5f;
        _attackValue = 10;
        _visibilityRange = 2f;
        _movementSpeed = 3f;
        agentColor = Color.green;
        SetAllyTypes(new AGENT_TYPE[]{AGENT_TYPE.GUARD, AGENT_TYPE.CITY});
        SetInitialHP(100, 100);
    }

    #region overrides
    internal override AIBehaviour DetermineAction(List<GameAgent> threatsInRange, List<GameAgent> targetsInRange, List<GameAgent> alliesInRange, bool isPerformingAction) {
        Vector2 centerPositionOfCity = _guardingCity.hexTile.transform.position;
        float distance = Vector2.Distance(_agentObj.aiPath.transform.position, centerPositionOfCity);
        if(distance > _guardingCity.cityBounds) {
            if(agentObj.currentAction == ACTION_TYPE.ATTACK) {
                //give up attacking because guard is already out of city bounds
                return _randomBehaviour;
            }
        }

        int totalSurroundingInitiative = GetTotalSurroundingInitiative(targetsInRange);
        int totalSurroundingThreat = GetTotalSurroundingThreat(threatsInRange);
        int totalInitiativeFromAllies = GetTotalInitiativeFromAllies(alliesInRange);

        int totalAttackRatio = totalSurroundingInitiative + totalInitiativeFromAllies;
        int totalFleeRatio = totalSurroundingThreat;

        if (isPerformingAction && agentObj.currentAction != ACTION_TYPE.RANDOM) {
            //this agent is currently performing a flee or attack action, let it finish.
            return null;
        }

        if (targetsInRange.Count > 0 && threatsInRange.Count > 0) {
            if (totalAttackRatio > totalFleeRatio) {
                return _attackBehaviour;
            } else if (totalFleeRatio > totalAttackRatio) {
                return _fleeBehaviour;
            } else {
                //totalFleeRatio and totalAttackRatio are equal
                if (Random.Range(0, 2) == 0) {
                    return _attackBehaviour;
                } else {
                    return _fleeBehaviour;
                }
            }
        } else if (targetsInRange.Count > 0 && threatsInRange.Count <= 0) {
            return _attackBehaviour;
        } else if (threatsInRange.Count > 0 && targetsInRange.Count <= 0) {
            return _fleeBehaviour;
        } else {
            if (!isPerformingAction) {
                return _randomBehaviour;
            }
        }
        return null;
    }
    #endregion
}
