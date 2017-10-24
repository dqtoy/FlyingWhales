using UnityEngine;
using System.Collections;

public class RoamKingdom : AIBehaviour {

    private Kingdom _targetKingdom;

    public RoamKingdom(Agent agentPerformingAction, Kingdom targetKingdom) : base(ACTION_TYPE.RANDOM, agentPerformingAction) {
        _targetKingdom = targetKingdom;
    }

    #region overrides
    internal override void DoAction() {
        base.DoAction();
        agentPerformingAction.agentObj.SetTarget(Utilities.PickRandomPointInCircle(agentPerformingAction.agentObj.transform.position, agentPerformingAction.visibilityRange));
    }
    #endregion

}
