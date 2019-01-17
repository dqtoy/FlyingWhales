using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocationPortrait : MonoBehaviour, IPointerClickHandler {

    public Area area { get; private set; }

    [SerializeField] private Image portrait;

    public void OnPointerClick(PointerEventData eventData) {
        if (area != null) {
            UIManager.Instance.ShowAreaInfo(area);
        }
    }

    public void SetLocation(Area area) {
        this.area = area;
        portrait.sprite = LandmarkManager.Instance.locationPortraits[area.name];
    }
}
