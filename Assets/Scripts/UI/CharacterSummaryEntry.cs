using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CharacterSummaryEntry : MonoBehaviour {

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
        Messenger.AddListener<ICharacter, Squad>(Signals.SQUAD_MEMBER_ADDED, OnSquadEdited);
        Messenger.AddListener<ICharacter, Squad>(Signals.SQUAD_MEMBER_REMOVED, OnSquadEdited);
        Messenger.AddListener<ICharacter, NewParty>(Signals.CHARACTER_JOINED_PARTY, OnPartyEdited);
        Messenger.AddListener<ICharacter, NewParty>(Signals.CHARACTER_LEFT_PARTY, OnPartyEdited);
        Messenger.AddListener<ECS.Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnFactionEdited);
        Messenger.AddListener<ECS.Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnFactionEdited);
        Messenger.AddListener<CharacterAction, CharacterParty>(Signals.ACTION_TAKEN, OnActionTaken);
        actionIcon.Initialize();
    }

    public void SetCharacter(ECS.Character character) {
        _character = character;
        characterPortrait.GeneratePortrait(character, IMAGE_SIZE.X256, true, true);
        UpdateCharacterInfo();
        affiliations.UpdateAffiliations(character);
        actionIcon.SetCharacter(character);
    }

    public void UpdateCharacterInfo() {
        characterNameLbl.text = character.name;
        characterLvlClassLbl.text = "Lvl." + character.level.ToString() + " " + character.characterClass.className;
        //if (_character.isFactionless) {
        //    factionNameLbl.text = "Unaligned";
        //} else {
        //    factionNameLbl.text = character.faction.name;
        //    //bgSprite.color = character.faction.factionColor;
        //}
        //raceLbl.text = Utilities.NormalizeString(character.raceSetting.race.ToString());
        //roleLbl.text = Utilities.NormalizeString(character.role.roleType.ToString());
    }

    public void SetBGColor(Color color) {
        bgSprite.color = color;
    }

    //public void OnPointerClick(PointerEventData eventData) {
    //    UIManager.Instance.ShowCharacterInfo(_character);
    //}

    #region Sorting
    public void OnOrderByName() {
        this.name = _character.name;
    }
    public void OnOrderByFaction() {
        if (_character.faction == null) {
            this.name = "Unaligned";
        } else {
            this.name = _character.faction.name;
        }
    }
    public void OnOrderByRace() {
        this.name = _character.raceSetting.race.ToString();
    }
    public void OnOrderByRole() {
        this.name = _character.role.roleType.ToString();
    }
    #endregion

    private void OnSquadEdited(ICharacter character, Squad affectedSquad) {
        if (character is ECS.Character && _character.id == character.id) {
            affiliations.UpdateAffiliations(character as ECS.Character);
        }
    }
    private void OnPartyEdited(ICharacter character, NewParty affectedParty) {
        if (_character.party == null || _character.currentParty.id == affectedParty.id) {
            affiliations.UpdateAffiliations(_character as ECS.Character);
        }
    }
    private void OnFactionEdited(ICharacter character, Faction affectedFaction) {
        if (character is ECS.Character && _character.id == character.id) {
            affiliations.UpdateAffiliations(character as ECS.Character);
        }
    }
    private void OnActionTaken(CharacterAction action, CharacterParty party) {
        if (party.icharacters.Contains(_character)) {
            actionIcon.SetAction(action);
        }
    }

    private void OnDestroy() {
        Messenger.RemoveListener<ICharacter, Squad>(Signals.SQUAD_MEMBER_ADDED, OnSquadEdited);
        Messenger.RemoveListener<ICharacter, Squad>(Signals.SQUAD_MEMBER_REMOVED, OnSquadEdited);
        Messenger.RemoveListener<ICharacter, NewParty>(Signals.CHARACTER_JOINED_PARTY, OnPartyEdited);
        Messenger.RemoveListener<ICharacter, NewParty>(Signals.CHARACTER_LEFT_PARTY, OnPartyEdited);
        Messenger.RemoveListener<ECS.Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnFactionEdited);
        Messenger.RemoveListener<ECS.Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnFactionEdited);
        Messenger.RemoveListener<CharacterAction, CharacterParty>(Signals.ACTION_TAKEN, OnActionTaken);
    }
}
