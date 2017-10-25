using UnityEngine;
using System.Collections;
using System.Linq;

public class RunAwayFromHostile : AIBehaviour {

    private Kingdom _allowedInKingdom;

    public RunAwayFromHostile(GameAgent agentPerformingAction, Kingdom allowedInKingdom) : base(ACTION_TYPE.FLEE, agentPerformingAction) {
        _allowedInKingdom = allowedInKingdom;
    }

    #region overrides
    internal override void DoAction() {
        base.DoAction();
        GameAgent fleeingFrom = agentPerformingAction.agentObj.threatsInRange
            .OrderBy(x => Vector2.Distance(agentPerformingAction.agentObj.transform.position, x.agentObj.transform.position)).First();
        Vector2 randomPoint = Utilities.PickRandomPointInCircle(agentPerformingAction.agentObj.transform.position, agentPerformingAction.visibilityRange + 2f);
        Vector2 dirAwayFromWeakness = (fleeingFrom.agentObj.transform.position - agentPerformingAction.agentObj.transform.position).normalized;
        agentPerformingAction.agentObj.SetTarget(randomPoint + dirAwayFromWeakness);

        //agentPerformingAction.entityGO.OnEntityAttacked(fleeingFrom.entity);
    }
    #endregion
}
