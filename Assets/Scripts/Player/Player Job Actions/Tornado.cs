﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tornado : PlayerJobAction {

    private int radius;
    private int durationInTicks;

    public Tornado() : base(INTERVENTION_ABILITY.TORNADO) {
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE };
        radius = 1;
        tier = 1;
        durationInTicks = GameManager.Instance.GetTicksBasedOnHour(2);
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("TornadoObject", Vector3.zero, Quaternion.identity, targetTile.parentAreaMap.objectsParent);
        TornadoObject obj = go.GetComponent<TornadoObject>();
        obj.Initialize(targetTile, radius + (radius * 2));
       
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (level == 1) {
            durationInTicks = GameManager.Instance.GetTicksBasedOnHour(2);
        } else if (level == 2) {
            durationInTicks = GameManager.Instance.GetTicksBasedOnHour(4);
        } else {
            durationInTicks = GameManager.Instance.GetTicksBasedOnHour(6);
        }
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = targetTile.parentAreaMap.GetTilesInRadius(targetTile, radius, 0, true);
        InteriorMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = targetTile.parentAreaMap.GetTilesInRadius(targetTile, radius, 0, true);
        InteriorMapManager.Instance.UnhighlightTiles(tiles);
    }
    #endregion
}

public class TornadoData : PlayerJobActionData {
    public override string name { get { return "Tornado"; } }
    public override string description { get { return "Spawn a tornado that randomly moves around dealing heavy damage to objects and characters caught in its path."; } }
    public override INTERVENTION_ABILITY_CATEGORY category { get { return INTERVENTION_ABILITY_CATEGORY.DEVASTATION; } }
}