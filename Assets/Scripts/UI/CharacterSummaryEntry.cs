using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using EZObjectPools;

public class CharacterSummaryEntry : PooledObject {

    private ECS.Character _character;

    [SerializeField] private Image bgSprite;
    [SerializeField] private TextMeshProUGUI characterNameLbl;
    [SerializeField] private TextMeshProUGUI characterLvlClassLbl;
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private AffiliationsObject affiliations;
    [SerializeField] private ActionIcon actionIcon;

    #region getters/setters
    public ECS.Character character {
        get { return _character; }
    }
    #endregion

    public void Initialize() {
        //Messenger.AddListener<CharacterAction, CharacterParty>(Signals.ACTION_TAKEN, OnActionTaken);
        //Messenger.AddListener(Signals.UPDATE_UI, UpdateCharacterInfo);
        actionIcon.Initialize();
    }

    public void SetCharacter(ECS.Character character) {
        _character = character;
        affiliations.Initialize(character);
        actionIcon.SetCharacter(character);
        actionIcon.SetAction((character.currentParty as CharacterParty).actionData.currentAction);
        characterPortrait.SetDimensions(42f);
        characterPortrait.GeneratePortrait(character, IMAGE_SIZE.X256, true, true);
        UpdateCharacterInfo();
        UpdateAffiliations();
    }
    public void UpdateAffiliations() {
        affiliations.UpdateAffiliations();
    }

    public void UpdateCharacterInfo() {
        if (character == null) {
            return;
        }
        characterNameLbl.text = character.name;
        characterLvlClassLbl.text = "Lvl." + character.level.ToString() + " " + character.characterClass.className;
    }

    public void SetBGColor(Color color) {
        bgSprite.color = color;
    }

    //private void OnActionTaken(CharacterAction action, CharacterParty party) {
    //    if (party.icharacters.Contains(_character)) {
    //        actionIcon.SetAction(action);
    //    }
    //}
    private void RemoveListeners() {
        //Messenger.RemoveListener<CharacterAction, CharacterParty>(Signals.ACTION_TAKEN, OnActionTaken);
        //Messenger.RemoveListener(Signals.UPDATE_UI, UpdateCharacterInfo);
        affiliations.Reset();
    }

    public override void Reset() {
        base.Reset();
        RemoveListeners();
        _character = null;
        actionIcon.Reset();
    }
}
