using UnityEngine;
using System.Collections;

public class AttackSpecificHostile : AIBehaviour {

    private Agent target;

    public AttackSpecificHostile(Agent agentPerformingAction, Agent hostileToAttack) : base(ACTION_TYPE.ATTACK, agentPerformingAction) {
        target = hostileToAttack;
        //Messenger.RemoveListener<Entity>("EntityDied", OnEntityDied);
    }

    #region overrides
    internal override void DoAction() {
        base.DoAction();
        agentPerformingAction.agentObj.SetTarget(target.agentObj.transform);
    }
    internal override void OnActionDone() {
        base.OnActionDone();
        if (target != null) {
            //Battle
            int damageToTarget = agentPerformingAction.attackValue;
            //int damageToThisEntity = target.entityGO.entity.attackDamage;

            target.AdjustHP(-damageToTarget);
            //entityPerformingAction.AdjustHP(-damageToThisEntity);

            //if (target.isDead) {
            //    target.KillEntity();
            //}

            //if (entityPerformingAction.isDead) {
            //    entityPerformingAction.KillEntity();
            //}
        }
    }
    #endregion

    //private void OnEntityDied(Entity otherEntity) {
    //    if (otherEntity == target) {
    //        target = null;
    //        agentPerformingAction.entityGO.SetTarget(null);
    //        agentPerformingAction.entityGO.SetIsPerformingAction(false);
    //    } else if (otherEntity == agentPerformingAction) {
    //        Messenger.RemoveListener<Entity>("EntityDied", OnEntityDied);
    //    }
    //}
}
