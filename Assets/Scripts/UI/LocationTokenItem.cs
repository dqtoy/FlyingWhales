using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LocationTokenItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragParentItem {

    private LocationToken locationToken;

    [SerializeField] private TextMeshProUGUI areaNameLbl;
    [SerializeField] private DraggableItem draggable;

    public AreaEmblem emblem;

    public object associatedObj {
        get { return locationToken; }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        locationToken.location.SetOutlineState(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        locationToken.location.SetOutlineState(false);
    }

    public void SetLocation(LocationToken locationToken) {
        this.locationToken = locationToken;
        areaNameLbl.text = locationToken.location.name;
        draggable.SetAssociatedObject(locationToken);
    }

}
