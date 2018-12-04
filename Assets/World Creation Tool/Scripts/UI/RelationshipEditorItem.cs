using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelationshipEditorItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private Relationship _relationship;

    [SerializeField] private Text characterNameLbl;
    [SerializeField] private Dropdown relationshipsDropdown;
    [SerializeField] private Text relationshipsLbl;

    #region getters/setters
    public Relationship relationship {
        get { return _relationship; }
    }
    #endregion

    private void Awake() {
        Messenger.AddListener<Character, GENDER>(Signals.GENDER_CHANGED, OnCharacterChangedGender);
    }

    public void SetRelationship(Relationship rel) {
        _relationship = rel;
        LoadRelationshipChoices();
        UpdateInfo();
    }
    private void LoadRelationshipChoices() {
        relationshipsDropdown.ClearOptions();
        relationshipsDropdown.AddOptions(Utilities.GetPossibleRelationshipsChoicesBasedOnGender(_relationship.sourceCharacter.gender, _relationship.targetCharacter.gender));
    }
    private void UpdateInfo() {
        characterNameLbl.text = _relationship.targetCharacter.name;
        UpdateStatusSummary();
    }
    private void UpdateStatusSummary() {
        relationshipsLbl.text  = string.Empty;
        for (int i = 0; i < _relationship.relationshipStatuses.Count; i++) {
            relationshipsLbl.text += "[<b>" + _relationship.relationshipStatuses[i].ToString() + "</b>] ";
        }
    }

    public void AddRelationshipStatus() {
        CHARACTER_RELATIONSHIP relStat = (CHARACTER_RELATIONSHIP)Enum.Parse(typeof(CHARACTER_RELATIONSHIP), relationshipsDropdown.options[relationshipsDropdown.value].text);
        if (IsRelationshipStatusValid(relStat)) {
            _relationship.AddRelationshipStatus(relStat);
            UpdateStatusSummary();
        } else {
            worldcreator.WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Invalid relationship", _relationship.targetCharacter.name + " cannot be " + _relationship.sourceCharacter.name + "'s " + relStat.ToString());
        }
    }
    private bool IsRelationshipStatusValid(CHARACTER_RELATIONSHIP relStat) {
        //if (Utilities.IsRelationshipStatusUnique(relStat) && _relationship.sourceCharacter.AlreadyHasRelationshipStatus(relStat)) { //check if a relationship status is unique, and if the source character already has a relationship status of that type
        //    return false;
        //}
        //Relationship targetRelWithSource = _relationship.targetCharacter.GetRelationshipWith(_relationship.sourceCharacter);
        //if (Utilities.IsRelationshipStatusMutuallyExclusive(relStat, _relationship, targetRelWithSource)) {
        //    return false;
        //}
        return true;
    }
    public void RemoveRelationshipStatus() {
        CHARACTER_RELATIONSHIP relStat = (CHARACTER_RELATIONSHIP)Enum.Parse(typeof(CHARACTER_RELATIONSHIP), relationshipsDropdown.options[relationshipsDropdown.value].text);
        _relationship.RemoveRelationshipStatus(relStat);
        UpdateStatusSummary();
    }
    public void RemoveRelationship() {
        //_relationship.sourceCharacter.RemoveRelationshipWith(_relationship.targetCharacter);
    }
    private void OnCharacterChangedGender(Character character, GENDER newGender) {
        if (_relationship.sourceCharacter.id == character.id || _relationship.targetCharacter.id == character.id) {
            LoadRelationshipChoices();
            _relationship.OnCharacterChangedGender(character, newGender);
            UpdateStatusSummary();
        }
    }

    #region Events
    public void OnPointerEnter(PointerEventData eventData) {
        worldcreator.WorldCreatorUI.Instance.ShowSmallCharacterInfo(_relationship.targetCharacter);
    }
    public void OnPointerExit(PointerEventData eventData) {
        worldcreator.WorldCreatorUI.Instance.HideSmallCharacterInfo();
    }
    #endregion

    #region Monobehaviours
    private void OnDestroy() {
        Messenger.RemoveListener<Character, GENDER>(Signals.GENDER_CHANGED, OnCharacterChangedGender);
    }
    #endregion
}
