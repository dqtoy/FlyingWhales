using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffiliationsObject : MonoBehaviour {

    private ECS.Character _character;
    private enum HoveredObject {
        None,
        Faction,
        Squad,
        Party,
    }

    [SerializeField] private GameObject factionGO;
    [SerializeField] private GameObject squadGO;
    [SerializeField] private GameObject partyGO;

    private bool disableParty;
    private bool isHovering = false;
    private HoveredObject hoveredObject = HoveredObject.None;

    public void Initialize(ECS.Character character) {
        Initialize();
        SetCharacter(character);
    }
    public void Initialize() {
        Messenger.AddListener(Signals.UPDATE_UI, UpdateAffiliations);
        Messenger.AddListener<ICharacter, Squad>(Signals.SQUAD_MEMBER_ADDED, OnSquadEdited);
        Messenger.AddListener<ICharacter, Squad>(Signals.SQUAD_MEMBER_REMOVED, OnSquadEdited);
        Messenger.AddListener<ICharacter, NewParty>(Signals.CHARACTER_JOINED_PARTY, OnPartyEdited);
        Messenger.AddListener<ICharacter, NewParty>(Signals.CHARACTER_LEFT_PARTY, OnPartyEdited);
        Messenger.AddListener<ECS.Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnFactionEdited);
        Messenger.AddListener<ECS.Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnFactionEdited);
    }
    public void Reset() {
        Messenger.RemoveListener<ICharacter, Squad>(Signals.SQUAD_MEMBER_ADDED, OnSquadEdited);
        Messenger.RemoveListener<ICharacter, Squad>(Signals.SQUAD_MEMBER_REMOVED, OnSquadEdited);
        Messenger.RemoveListener<ICharacter, NewParty>(Signals.CHARACTER_JOINED_PARTY, OnPartyEdited);
        Messenger.RemoveListener<ICharacter, NewParty>(Signals.CHARACTER_LEFT_PARTY, OnPartyEdited);
        Messenger.RemoveListener<ECS.Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnFactionEdited);
        Messenger.RemoveListener<ECS.Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnFactionEdited);
        Messenger.RemoveListener(Signals.UPDATE_UI, UpdateAffiliations);
        _character = null;
    }

    #region Signal Handlers
    private void OnSquadEdited(ICharacter character, Squad affectedSquad) {
        if (_character == null) {
            return;
        }
        if (character is ECS.Character && _character.id == character.id) {
            UpdateAffiliations();
        }
    }
    private void OnPartyEdited(ICharacter character, NewParty affectedParty) {
        if (_character == null) {
            return;
        }
        if (_character.party == null || _character.currentParty.id == affectedParty.id) {
            UpdateAffiliations();
        }
    }
    private void OnFactionEdited(ICharacter character, Faction affectedFaction) {
        if (_character == null) {
            return;
        }
        if (character is ECS.Character && _character.id == character.id) {
            UpdateAffiliations();
        }
    }
    #endregion

    public void SetDisablePartyState(bool disableParty) {
        this.disableParty = disableParty;
    }

    #region Utilities
    public void UpdateAffiliations() {
        if (_character == null) {
            return;
        }
        if (_character.isFactionless) {
            factionGO.SetActive(false);
        } else {
            factionGO.SetActive(true);
        }

        if (_character.squad == null) {
            squadGO.SetActive(false);
        } else {
            squadGO.SetActive(true);
        }

        if (disableParty || _character.currentParty.icharacters.Count <= 1) {
            partyGO.SetActive(false);
        } else {
            partyGO.SetActive(true);
        }
    }
    public void SetCharacter(ECS.Character character) {
        _character = character;
        UpdateAffiliations();
    }
    #endregion

    #region Pointer Functions
    public void ShowFactionInfo() {
        isHovering = true;
        hoveredObject = HoveredObject.Faction;
    }
    public void ShowSquadInfo() {
        isHovering = true;
        hoveredObject = HoveredObject.Squad;
    }
    public void ShowPartyInfo() {
        isHovering = true;
        hoveredObject = HoveredObject.Party;
    }
    public void OnClickPartyIcon() {
        UIManager.Instance.ShowPartyInfo(_character.currentParty);
    }
    public void OnClickFactionIcon() {
        UIManager.Instance.ShowFactionInfo(_character.faction);
    }
    public void HideInfo() {
        isHovering = false;
        hoveredObject = HoveredObject.None;
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Monobehaviours
    private void Update() {
        if (isHovering) {
            switch (hoveredObject) {
                case HoveredObject.Faction:
                    UIManager.Instance.ShowSmallInfo(_character.faction.name);
                    break;
                case HoveredObject.Squad:
                    UIManager.Instance.ShowSmallInfo(_character.squad.name);
                    break;
                case HoveredObject.Party:
                    UIManager.Instance.ShowSmallInfo(_character.currentParty.name);
                    break;
                default:
                    break;
            }
        }
    }
    #endregion
}
