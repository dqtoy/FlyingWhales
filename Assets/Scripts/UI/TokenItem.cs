using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TokenItem : MonoBehaviour, IPointerClickHandler {

    public delegate void OnTokenItemClicked(object obj);
    private OnTokenItemClicked onTokenItemClicked;

    [SerializeField] private SlotItem slot;
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI subText;

    #region Pointer Actions
    public void OnPointerClick(PointerEventData eventData) {
        if (onTokenItemClicked != null) {
            onTokenItemClicked(slot.placedObject);
        }
    }
    #endregion

    public void SetObject(Token token) {
        slot.PlaceObject(token);
        UpdateText();
    }
    public void SetObject(Minion minion) {
        slot.PlaceObject(minion);
        UpdateText();
    }
    private void UpdateText() {
        if (slot.placedObject is CharacterToken) {
            Character character = (slot.placedObject as CharacterToken).character;
            mainText.text = character.name;
            subText.text = "Lvl. " + character.level.ToString() + " " + character.characterClass.className;
        } else if (slot.placedObject is LocationToken) {
            Area location = (slot.placedObject as LocationToken).location;
            mainText.text = location.name;
            subText.text = Utilities.NormalizeString(location.GetAreaTypeString());
        } else if (slot.placedObject is FactionToken) {
            Faction faction = (slot.placedObject as FactionToken).faction;
            mainText.text = faction.name;
            subText.text = Utilities.NormalizeString(faction.raceType.ToString());
        }
    }

    #region Utilities
    public void SetClickAction(OnTokenItemClicked itemClicked) {
        onTokenItemClicked = itemClicked;
    }
    #endregion
}
