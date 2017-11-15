using UnityEngine;
using System.Collections;

public class KingdomEmblem : MonoBehaviour {

    public delegate void OnEmblemClicked(KingdomEmblem emblem);
    public OnEmblemClicked onEmblemClicked;

    private Kingdom _kingdom;
    private bool _isSelected;

    [SerializeField] private UI2DSprite _emblemBG;
    [SerializeField] private UI2DSprite _emblemSprite;
    [SerializeField] private UI2DSprite _emblemOutline;

    [SerializeField] private GameObject _selectedGO;
    [SerializeField] private UIEventTrigger eventTrigger;

    #region getters/setters
    internal Kingdom kingdom {
        get { return _kingdom; }
    }
    internal bool isSelected {
        get { return _isSelected; }
    }
    #endregion

    private void Awake() {
        EventDelegate.Set(eventTrigger.onClick, OnEmblemClick);
    }

    internal void SetKingdom(Kingdom kingdom) {
        _kingdom = kingdom;

        _emblemBG.sprite2D = kingdom.emblemBG;
        _emblemSprite.sprite2D = kingdom.emblem;
        _emblemOutline.sprite2D = kingdom.emblemBG;

        Color emblemBGColor = kingdom.kingdomColor;
        emblemBGColor.a = 255f / 255f;
        _emblemBG.color = emblemBGColor;

        onEmblemClicked = null;
    }

    internal void ToggleEmblemSelectedState() {
        SetEmblemSelectedState(!_selectedGO.activeSelf);
    }
    internal void SetEmblemSelectedState(bool isSelected) {
        _selectedGO.SetActive(isSelected);
        _isSelected = isSelected;
    }

    private void OnEmblemClick() {
        if(onEmblemClicked != null) {
            onEmblemClicked(this);
        }
    }
}
