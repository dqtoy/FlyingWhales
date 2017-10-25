using UnityEngine;
using System.Collections;
using System.Linq;

public class StealFromKingdom : AIBehaviour {

    private Kingdom _targetKingdom;

    private bool isReadyToSteal;

    private int currStealCooldown;
    private const int STEAL_COOLDOWN = 3;

    public StealFromKingdom(GameAgent agentPerformingAction, Kingdom targetKingdom) : base(ACTION_TYPE.ATTACK, agentPerformingAction) {
        _targetKingdom = targetKingdom;
        isReadyToSteal = true;
    }

    #region overrides
    internal override void DoAction() {
        base.DoAction();
        if (isReadyToSteal) {
            //Get nearest city of kingdom to steal from
            agentPerformingAction.agentObj.SetTarget(_targetKingdom.cities
                               .OrderBy(x => Vector2.Distance(agentPerformingAction.agentObj.transform.position, x.hexTile.transform.position)).First().hexTile.transform);
            currStealCooldown = STEAL_COOLDOWN;
            isReadyToSteal = false;
        } else {
            //Chose random place to go to
            agentPerformingAction.agentObj.SetTarget(Utilities.PickRandomPointInCircle(agentPerformingAction.agentObj.transform.position, agentPerformingAction.visibilityRange));

        }
    }
    internal override void OnActionDone() {
        base.OnActionDone();
        if(currStealCooldown <= 0) {
            isReadyToSteal = true;
        } else {
            currStealCooldown--;
        }
    }
    #endregion
}
