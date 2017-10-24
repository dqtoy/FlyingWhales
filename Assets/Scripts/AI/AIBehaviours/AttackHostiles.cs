using UnityEngine;
using System.Collections;

public class AttackHostiles : AIBehaviour {

    private Agent target;

    public AttackHostiles(Agent agentPerformingAction) : base(ACTION_TYPE.ATTACK, agentPerformingAction) {
        //Messenger.AddListener<Entity>("EntityDied", OnEntityDied);
    }

    #region overrides
    internal override void DoAction() {
        base.DoAction();
        if (agentPerformingAction.agentObj.targetsInRange.Count <= 0) {
            throw new System.Exception(agentPerformingAction.agentObj.name + " is trying to attack but there are no targets in range!");
        }

        if (target == null && !agentPerformingAction.isDead) {
            target = agentPerformingAction.agentObj.targetsInRange[Random.Range(0, agentPerformingAction.agentObj.targetsInRange.Count)];
            agentPerformingAction.agentObj.SetTarget(target.agentObj.transform);
        }

    }
    internal override void OnActionDone() {
        base.OnActionDone();
        if(target != null) {
            //Battle
            int damageToTarget = agentPerformingAction.attackValue;
            //int damageToThisEntity = target.entityGO.entity.attackDamage;

            //target.agentObj.OnEntityAttacked(agentPerformingAction);

            target.AdjustHP(-damageToTarget);
            //entityPerformingAction.AdjustHP(-damageToThisEntity);

            if (target.isDead) {
                target.KillAgent();
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
