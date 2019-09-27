using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;

public class WorldEventNameplate : PooledObject {

    public WorldEvent worldEvent { get; private set; }

    public TextMeshProUGUI eventNameLbl;
    public Image eventIconImg;
    public Button mainBtn;
    public GameObject coverGO;

    public void Initialize(WorldEvent e) {
        worldEvent = e;
        UpdateInfo();
    }

    private void UpdateInfo() {
        if(worldEvent == null) { return; }
        eventNameLbl.text = worldEvent.name;
        //TODO: eventIconImg
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        worldEvent = null;
    }
    #endregion

    #region Mouse Actions
    public void OnHover() {
        if(worldEvent == null) { return; }
        UIManager.Instance.ShowSmallInfo(worldEvent.description);
    }
    public void OnHoverOut() {
        if (worldEvent == null) { return; }
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
