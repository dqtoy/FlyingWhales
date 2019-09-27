using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;

public class WorldEventNameplate : PooledObject {

    public WorldEvent worldEvent { get; private set; }
    public Region region { get; private set; }

    public TextMeshProUGUI eventNameLbl;
    public Image eventIconImg;
    public Button mainBtn;
    public GameObject coverGO;

    public System.Action<Region, WorldEvent> onClickAction { get; private set; }

    public void Initialize(WorldEvent e) {
        worldEvent = e;
        UpdateInfo();
    }
    public void Initialize(Region region, WorldEvent e) {
        worldEvent = e;
        this.region = region;
        UpdateInfo();
    }

    private void UpdateInfo() {
        if(worldEvent == null) { return; }
        eventNameLbl.text = worldEvent.name;
        //TODO: eventIconImg
    }

    public void SetClickAction(System.Action<Region, WorldEvent> onClickAction) {
        this.onClickAction = onClickAction;
    }
    public void SetMainButtonState(bool state) {
        mainBtn.interactable = state;
        coverGO.SetActive(!state);
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        worldEvent = null;
        region = null;
    }
    #endregion

    #region Mouse Actions
    public void OnClickThis() {
        if (onClickAction != null) {
            onClickAction(region, worldEvent);
        }
    }
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
