using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AreaNameplate : MonoBehaviour {

    private Area area;

    [SerializeField] private FactionEmblem emblem;
    [SerializeField] private TextMeshProUGUI areaNameLbl;

    public void SetArea(Area area) {
        this.area = area;
        name = area.name + " Nameplate";
        UpdateVisuals();
        UpdatePosition();

        Messenger.AddListener<Area>(Signals.AREA_OWNER_CHANGED, OnAreaOwnerChanged);
    }

    private void UpdateVisuals() {
        emblem.SetFaction(area.owner);
        areaNameLbl.text = area.name;
    }

    private void UpdatePosition() {
        //Vector2 originalPos = area.coreTile.transform.position;
        //originalPos.y -= 1f;
        Vector2 ScreenPosition = Camera.main.WorldToScreenPoint(area.nameplatePos);
        this.transform.position = ScreenPosition;
    }

    public void Update() {
        UpdatePosition();
    }

    private void OnAreaOwnerChanged(Area area) {
        if (this.area.id == area.id) {
            UpdateVisuals();
        }
    }
}
