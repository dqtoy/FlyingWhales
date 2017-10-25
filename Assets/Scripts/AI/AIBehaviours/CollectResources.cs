using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CollectResources : AIBehaviour {

    private Kingdom _collectResourcesFrom;

    private bool isGoingHome;

    public CollectResources(GameAgent agentPerformingAction, Kingdom collectResourcesFrom) : base(ACTION_TYPE.RANDOM, agentPerformingAction) {
        _collectResourcesFrom = collectResourcesFrom;
        isGoingHome = false;
    }

    #region overrides
    internal override void DoAction() {
        base.DoAction();
        if (isGoingHome) {
            agentPerformingAction.agentObj.SetTarget(_collectResourcesFrom.cities
                               .OrderBy(x => Vector2.Distance(agentPerformingAction.agentObj.transform.position, x.hexTile.transform.position)).First().hexTile.transform);
        } else {
            //Find nearest resource tile
            List<HexTile> resourceTilesInKingdom = new List<HexTile>();
            for (int i = 0; i < _collectResourcesFrom.regions.Count; i++) {
                if(_collectResourcesFrom.regions[i].tileWithSpecialResource != null) {
                    resourceTilesInKingdom.Add(_collectResourcesFrom.regions[i].tileWithSpecialResource);
                }
            }
            if(resourceTilesInKingdom.Count > 0) {
                agentPerformingAction.agentObj.SetTarget(resourceTilesInKingdom
                                .OrderBy(x => Vector2.Distance(agentPerformingAction.agentObj.transform.position, x.transform.position)).First().transform);
            } else {
                agentPerformingAction.agentObj.SetTarget(Utilities.PickRandomPointInCircle(agentPerformingAction.agentObj.transform.position, agentPerformingAction.visibilityRange));
            }
        }
    }
    internal override void OnActionDone() {
        base.OnActionDone();
        isGoingHome = !isGoingHome;
    }
    #endregion
}
