using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterNameplateItem : NameplateItem<Character> {

    [Header("Character Nameplate Attributes")]
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private GameObject travellingIcon;
    [SerializeField] private GameObject arrivedIcon;
    [SerializeField] private GameObject restrainedIcon;

    public Character character { get; private set; }

    #region Overrides
    public override void SetObject(Character character) {
        base.SetObject(character);
        this.character = character;
        mainLbl.text = character.name;
        subLbl.text = character.raceClassName;
        portrait.GeneratePortrait(character);
        UpdateStatusIcons();
    }
    public override void OnHoverEnter() {
        portrait.SetHoverHighlightState(true);
        //if (character != null && character.minion != null) {
        //    UIManager.Instance.ShowMinionCardTooltip(character.minion);
        //}
        base.OnHoverEnter();
    }
    public override void OnHoverExit() {
        portrait.SetHoverHighlightState(false);
        //if (character != null && character.minion != null) {
        //    UIManager.Instance.HideMinionCardTooltip();
        //}
        base.OnHoverExit();
    }
    public override void Reset() {
        base.Reset();
        SetPortraitInteractableState(true);
    }
    #endregion

    /// <summary>
    /// Set this nameplate to behave in the default settings (button, onclick shows character UI, etc.)
    /// </summary>
    public void SetAsDefaultBehaviour() {
        SetAsButton();
        ClearAllOnClickActions();
        AddOnClickAction((character) => UIManager.Instance.ShowCharacterInfo(character));
        SetSupportingLabelState(false);
    }

    private void UpdateStatusIcons() {
        if (character.currentParty.icon.isTravellingOutside) {
            //character is travelling outside
            travellingIcon.SetActive(true);
            arrivedIcon.SetActive(false);
            restrainedIcon.SetActive(false);
        } else if (!character.isAtHomeRegion) {
            //character is at another location other than his/her home region
            travellingIcon.SetActive(false);
            arrivedIcon.SetActive(true);
            restrainedIcon.SetActive(false);
        } else if (character.traitContainer.GetNormalTrait("Restrained") != null) {
            //character is restrained
            travellingIcon.SetActive(false);
            arrivedIcon.SetActive(false);
            restrainedIcon.SetActive(true);
        } else {
            travellingIcon.SetActive(false);
            arrivedIcon.SetActive(false);
            restrainedIcon.SetActive(false);
        }
    }

    public void SetPortraitInteractableState(bool state) {
        portrait.ignoreInteractions = !state;
    }
}
