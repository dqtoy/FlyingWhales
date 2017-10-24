using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackHostiles : AIBehaviour {

    private Agent _target;

    #region getters/setters
    internal Agent target {
        get { return _target; }
    }
    #endregion
    public AttackHostiles(Agent agentPerformingAction) : base(ACTION_TYPE.ATTACK, agentPerformingAction) {
        //Messenger.AddListener<Entity>("EntityDied", OnEntityDied);
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
                List<Agent> possibleTargets = new List<Agent>(agentPerformingAction.agentObj.targetsInRange);
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

            //target.agentObj.OnEntityAttacked(agentPerformingAction);

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

    //private void OnEntityDied(Entity otherEntity) {
    //    if(otherEntity == target) {
    //        target = null;
    //        agentPerformingAction.entityGO.SetTarget(null);
    //        agentPerformingAction.entityGO.SetIsPerformingAction(false);
    //    } else if(otherEntity == agentPerformingAction) {
    //        Messenger.RemoveListener<Entity>("EntityDied", OnEntityDied);
    //    }
    //}
}
