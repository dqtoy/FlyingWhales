using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelationshipEditorItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    //private CharacterRelationshipData _relationship;

    [SerializeField] private Text characterNameLbl;
    [SerializeField] private Dropdown relationshipsDropdown;
    [SerializeField] private Text relationshipsLbl;

    #region getters/setters
    //public CharacterRelationshipData relationship {
    //    get { return _relationship; }
    //}
    #endregion

    //public void SetRelationship(CharacterRelationshipData rel) {
    //    _relationship = rel;
    //    relationshipsDropdown.ClearOptions();
    //    relationshipsDropdown.AddOptions(Utilities.GetEnumChoices<RELATIONSHIP_TRAIT>());
    //    UpdateInfo();
    //}
    public void UpdateInfo() {
        //characterNameLbl.text = _relationship.targetCharacter.name;
        UpdateStatusSummary();
    }
    private void UpdateStatusSummary() {
        relationshipsLbl.text  = string.Empty;
        //for (int i = 0; i < _relationship.rels.Count; i++) {
        //    relationshipsLbl.text += "[<b>" + _relationship.rels[i].name + "</b>] ";
        //}
    }
    public void RemoveRelationship() {
        //RelationshipManager.Instance.RemoveRelationshipBetween(_relationship.owner, _relationship.targetCharacter);
    }

    public void AddRelationshipOfType() {
        RELATIONSHIP_TRAIT relType = (RELATIONSHIP_TRAIT) Enum.Parse(typeof(RELATIONSHIP_TRAIT), relationshipsDropdown.options[relationshipsDropdown.value].text);
        //RelationshipManager.Instance.RemoveOneWayRelationship(_relationship.owner, _relationship.targetCharacter, relType);
    }
    public void RemoveRelationshipOfType() {
        RELATIONSHIP_TRAIT relType = (RELATIONSHIP_TRAIT)Enum.Parse(typeof(RELATIONSHIP_TRAIT), relationshipsDropdown.options[relationshipsDropdown.value].text);
        //RelationshipManager.Instance.RemoveRelationshipBetween(_relationship.owner, _relationship.targetCharacter, relType);
    }

    #region Events
    public void OnPointerEnter(PointerEventData eventData) {
        //worldcreator.WorldCreatorUI.Instance.ShowSmallCharacterInfo(_relationship.targetCharacter);
    }
    public void OnPointerExit(PointerEventData eventData) {
        worldcreator.WorldCreatorUI.Instance.HideSmallCharacterInfo();
    }
    #endregion

    private void OnDestroy() {
        worldcreator.WorldCreatorUI.Instance.HideSmallCharacterInfo();
    }
}
