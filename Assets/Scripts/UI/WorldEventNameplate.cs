using System.Collections;
using System.Collections.Generic;
using Events.World_Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;

public class WorldEventNameplate : NameplateItem<Region> {

    public WorldEvent worldEvent { get; private set; }
    public Region region { get; private set; }

    //public TextMeshProUGUI eventNameLbl;
    //public Image eventIconImg;
    //public Button mainBtn;
    //public GameObject coverGO;

    //public System.Action<Region, WorldEvent> onClickAction { get; private set; }

    #region Overrides
    public override void SetObject(Region r) {
        base.SetObject(r);
        region = r;
        worldEvent = r.activeEvent;
        mainLbl.text = worldEvent.name;
        subLbl.text = string.Empty;
    }
    public override void OnHoverEnter() {
        if (worldEvent == null) { return; }
        UIManager.Instance.ShowSmallInfo(worldEvent.description);
        base.OnHoverEnter();
    }
    public override void OnHoverExit() {
        if (worldEvent == null) { return; }
        UIManager.Instance.HideSmallInfo();
        base.OnHoverExit();
    }
    #endregion
}
