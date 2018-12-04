using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using EZObjectPools;

public class CharacterIntelItem : PooledObject, IDragParentItem {

    private CharacterIntel _characterIntel;

    [SerializeField] private TextMeshProUGUI characterNameLbl;
    [SerializeField] private TextMeshProUGUI characterLvlClassLbl;
    [SerializeField] private DraggableItem draggable;
    public CharacterPortrait characterPortrait;

    #region getters/setters
    public Character character {
        get { return _characterIntel.character; }
    }
    public object associatedObj {
        get { return _characterIntel; }
    }
    #endregion

    public void Initialize() {
        //Messenger.AddListener<CharacterAction, CharacterParty>(Signals.ACTION_TAKEN, OnActionTaken);
        //Messenger.AddListener(Signals.UPDATE_UI, UpdateCharacterInfo);
        //actionIcon.Initialize();
    }

    public void SetCharacter(CharacterIntel characterIntel) {
        _characterIntel = characterIntel;
        //affiliations.Initialize(character);
        //actionIcon.SetCharacter(character);
        //actionIcon.SetAction(character.currentParty.currentAction);
        //characterPortrait.SetDimensions(42f);
        characterPortrait.GeneratePortrait(characterIntel.character);
        UpdateCharacterInfo();
        draggable.SetAssociatedObject(characterIntel);
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
        _characterIntel = null;
        //actionIcon.Reset();
    }
}
