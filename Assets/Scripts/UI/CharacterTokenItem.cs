using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using EZObjectPools;

public class CharacterTokenItem : PooledObject, IDragParentItem {

    private CharacterToken _characterToken;

    [SerializeField] private TextMeshProUGUI characterNameLbl;
    [SerializeField] private TextMeshProUGUI characterLvlClassLbl;
    [SerializeField] private DraggableItem _draggable;
    [SerializeField] private GameObject _grayedOutGO;

    public CharacterPortrait characterPortrait;

    #region getters/setters
    public Character character {
        get { return _characterToken.character; }
    }
    public object associatedObj {
        get { return _characterToken; }
    }
    public bool isDraggable {
        get { return _draggable.isDraggable; }
    }
    #endregion

    public void Initialize() {
        //Messenger.AddListener<CharacterAction, CharacterParty>(Signals.ACTION_TAKEN, OnActionTaken);
        //Messenger.AddListener(Signals.UPDATE_UI, UpdateCharacterInfo);
        //actionIcon.Initialize();
    }

    public void SetCharacter(CharacterToken characterToken) {
        _characterToken = characterToken;
        //affiliations.Initialize(character);
        //actionIcon.SetCharacter(character);
        //actionIcon.SetAction(character.currentParty.currentAction);
        //characterPortrait.SetDimensions(42f);
        characterPortrait.GeneratePortrait(characterToken.character);
        UpdateCharacterInfo();
        _draggable.SetAssociatedObject(characterToken);
        //UpdateAffiliations();
    }
    //public void UpdateAffiliations() {
    //    affiliations.UpdateAffiliations();
    //}

    public void UpdateCharacterInfo() {
        if (character == null) {
            return;
        }

        characterNameLbl.text = character.name;
        if (character.isDead) {
            characterNameLbl.text += "(Dead)"; 
        }
        characterLvlClassLbl.text = "Lvl." + character.level.ToString() + " " + character.characterClass.className;
    }
    public void SetDraggable(bool state) {
        _draggable.SetDraggable(state);
        _grayedOutGO.SetActive(!state);
    }

    //public void SetBGColor(Color color) {
    //    bgSprite.color = color;
    //}

    //private void OnActionTaken(CharacterAction action, CharacterParty party) {
    //    if (party.icharacters.Contains(_character)) {
    //        actionIcon.SetAction(action);
    //    }
    //}
    //private void RemoveListeners() {
    //    //Messenger.RemoveListener<CharacterAction, CharacterParty>(Signals.ACTION_TAKEN, OnActionTaken);
    //    //Messenger.RemoveListener(Signals.UPDATE_UI, UpdateCharacterInfo);
    //    //affiliations.Reset();
    //}

    public override void Reset() {
        base.Reset();
        //RemoveListeners();
        _characterToken = null;
        //actionIcon.Reset();
    }
}
