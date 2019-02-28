using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterMarker : PooledObject, IPointerClickHandler{

    public delegate void HoverMarkerAction(Character character, LocationGridTile location);
    public HoverMarkerAction hoverEnterAction;

    public System.Action hoverExitAction;

	public Character character { get; private set; }
    public LocationGridTile location { get; private set; }

    public void SetCharacter(Character character, LocationGridTile location) {
        this.character = character;
        this.location = location;
    }

    public void SetHoverAction(HoverMarkerAction hoverEnterAction, System.Action hoverExitAction) {
        this.hoverEnterAction = hoverEnterAction;
        this.hoverExitAction = hoverExitAction;
    }

    public void HoverAction() {
        if (hoverEnterAction != null) {
            hoverEnterAction.Invoke(character, location);
        }
    }
    public void HoverExitAction() {
        if (hoverExitAction != null) {
            hoverExitAction();
        }
    }

    public override void Reset() {
        base.Reset();
        character = null;
        location = null;
        hoverEnterAction = null;
        hoverExitAction = null;
    }

    public void OnPointerClick(PointerEventData eventData) {
        UIManager.Instance.ShowCharacterInfo(character);
    }
}
