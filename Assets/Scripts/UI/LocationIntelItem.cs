using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LocationIntelItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragParentItem {

    private LocationIntel locationIntel;

    [SerializeField] private TextMeshProUGUI areaNameLbl;
    [SerializeField] private DraggableItem draggable;

    public AreaEmblem emblem;

    public object associatedObj {
        get { return locationIntel; }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        locationIntel.location.SetOutlineState(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        locationIntel.location.SetOutlineState(false);
    }

    public void SetLocation(LocationIntel locationIntel) {
        this.locationIntel = locationIntel;
        areaNameLbl.text = locationIntel.location.name;
        draggable.SetAssociatedObject(locationIntel);
    }

}
