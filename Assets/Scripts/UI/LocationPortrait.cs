using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocationPortrait : MonoBehaviour, IPointerClickHandler {

    public Area area { get; private set; }

    [SerializeField] private Image portrait;
    [SerializeField] private GameObject hoverObj;

    public bool disableInteraction;

    public void OnPointerClick(PointerEventData eventData) {
        if (!disableInteraction && area != null) {
            UIManager.Instance.ShowAreaInfo(area);
        }
    }

    public void SetLocation(Area area) {
        this.area = area;
        portrait.sprite = LandmarkManager.Instance.locationPortraits[area.name];
    }

    public void SetHoverHighlightState(bool state) {
        if (!disableInteraction) {
            hoverObj.SetActive(state);
        }
    }

    public void ShowLocationInfo() {
        UIManager.Instance.ShowSmallInfo(this.area.name);
    }
    public void HideLocationInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
