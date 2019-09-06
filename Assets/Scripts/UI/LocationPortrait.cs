using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocationPortrait : PooledObject, IPointerClickHandler {

    public Area area { get; private set; }
    public BaseLandmark landmark { get; private set; }

    [SerializeField] private Image portrait;
    [SerializeField] private GameObject hoverObj;

    public bool disableInteraction;

    public void OnPointerClick(PointerEventData eventData) {
        if (!disableInteraction) {
            if (area != null) {
                UIManager.Instance.ShowHextileInfo(area.coreTile);
            } else if (landmark != null) {
                UIManager.Instance.ShowHextileInfo(landmark.tileLocation);
            }
        }
    }

    public void SetLocation(Area area) {
        this.area = area;
        this.landmark = null;
        portrait.sprite = area.locationPortrait;
    }
    public void SetLocation(BaseLandmark landmark) {
        this.landmark = landmark;
        this.area = null;
        portrait.sprite = LandmarkManager.Instance.GetLandmarkData(landmark.specificLandmarkType).landmarkPortrait;
    }

    public void SetHoverHighlightState(bool state) {
        if (!disableInteraction) {
            hoverObj.SetActive(state);
        }
    }

    public void ShowLocationInfo() {
        if (this.area != null) {
            UIManager.Instance.ShowSmallInfo(this.area.name);
        } else if (this.landmark != null) {
            UIManager.Instance.ShowSmallInfo(this.landmark.tileLocation.region.name);
        }
        
    }
    public void HideLocationInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    public override void Reset() {
        base.Reset();
        area = null;
        landmark = null;
    }
}
