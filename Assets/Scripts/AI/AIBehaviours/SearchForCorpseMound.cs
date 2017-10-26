using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SearchForCorpseMound : AIBehaviour {

    private CorpseMound _targetCorpseMound;

    public SearchForCorpseMound(GameAgent agentPerformingAction) : base(ACTION_TYPE.RANDOM, agentPerformingAction) {
        
    }

    #region overrides
    internal override void DoAction() {
        base.DoAction();
        LayerMask itemsMask = LayerMask.GetMask("AgentItems");
        Collider[] itemsInRange = Physics.OverlapSphere(agentPerformingAction.agentObj.aiPath.transform.position, agentPerformingAction.visibilityRange, itemsMask);
        List<CorpseMound> corpseMoundsInRange = new List<CorpseMound>();
        for (int i = 0; i < itemsInRange.Length; i++) {
            CorpseMound corpseMoundInRange = itemsInRange[i].GetComponent<CorpseMound>();
            if(corpseMoundInRange != null) {
                corpseMoundsInRange.Add(corpseMoundInRange);
            }
        }

        if(corpseMoundsInRange.Count > 0) {
            _targetCorpseMound = corpseMoundsInRange.OrderBy(x => Vector2.Distance(agentPerformingAction.agentObj.aiPath.transform.position, x.transform.position)).First();
            agentPerformingAction.agentObj.SetTarget(_targetCorpseMound.transform);
        } else {
            agentPerformingAction.agentObj.SetTarget(Utilities.PickRandomPointInCircle(agentPerformingAction.agentObj.aiPath.transform.position, agentPerformingAction.visibilityRange));
        }
    }
    internal override void OnActionDone() {
        base.OnActionDone();
        if(_targetCorpseMound != null) {
            agentPerformingAction.SetNewMaxHP(agentPerformingAction.maxHP + _targetCorpseMound.corpseCount);
            _targetCorpseMound.DestroyCorpseMound();
            _targetCorpseMound = null;
        }
    }
    #endregion
}
