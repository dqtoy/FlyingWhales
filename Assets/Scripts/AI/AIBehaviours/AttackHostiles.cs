using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackHostiles : AIBehaviour {

    private GameAgent _target;

    #region getters/setters
    internal GameAgent target {
        get { return _target; }
    }
    #endregion
    public AttackHostiles(GameAgent agentPerformingAction) : base(ACTION_TYPE.ATTACK, agentPerformingAction) {
        Messenger.AddListener<GameAgent>("OnAgentDied", OnAgentDied);
    }

    #region overrides
    internal override void DoAction() {
        base.DoAction();
        if(agentPerformingAction.agentCategory == AGENT_CATEGORY.LIVING) {
            if (agentPerformingAction.agentObj.targetsInRange.Count <= 0) {
                throw new System.Exception(agentPerformingAction.agentObj.name + " is trying to attack but there are no targets in range!");
            }

            if (_target == null && !agentPerformingAction.isDead) {
                _target = agentPerformingAction.agentObj.targetsInRange[Random.Range(0, agentPerformingAction.agentObj.targetsInRange.Count)];
                agentPerformingAction.agentObj.SetTarget(_target.agentObj.transform);
            }
        } else {
            if (agentPerformingAction.agentObj.targetsInRange.Count <= 0 && agentPerformingAction.agentObj.threatsInRange.Count <= 0) {
                throw new System.Exception(agentPerformingAction.agentObj.name + " is trying to attack but there are no targets in range!");
            }

            if (_target == null && !agentPerformingAction.isDead) {
                List<GameAgent> possibleTargets = new List<GameAgent>(agentPerformingAction.agentObj.targetsInRange);
                possibleTargets.AddRange(agentPerformingAction.agentObj.threatsInRange);
                _target = possibleTargets[Random.Range(0, possibleTargets.Count)];
                agentPerformingAction.agentObj.SetTarget(_target.agentObj.transform);
            }
        }
        

    }
    internal override void OnActionDone() {
        base.OnActionDone();
        if(_target != null) {
            //Battle
            int damageToTarget = agentPerformingAction.attackValue;
            //int damageToThisEntity = target.entityGO.entity.attackDamage;

            target.agentObj.OnAttacked(agentPerformingAction);

            _target.AdjustHP(-damageToTarget);
            //entityPerformingAction.AdjustHP(-damageToThisEntity);

            if (_target.isDead) {
                _target.KillAgent();
            }

            //if (entityPerformingAction.isDead) {
            //    entityPerformingAction.KillEntity();
            //}
        }
    }
    #endregion

    private void OnAgentDied(GameAgent otherAgent) {
        if (otherAgent == target) {
            _target = null;
            agentPerformingAction.agentObj.SetTarget(null);
            agentPerformingAction.agentObj.SetIsPerformingAction(false);
        } else if (otherAgent == agentPerformingAction) {
            Messenger.RemoveListener<GameAgent>("OnAgentDied", OnAgentDied);
        }
    }
}
