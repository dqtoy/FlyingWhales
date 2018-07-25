using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffiliationsObject : MonoBehaviour {

    private ECS.Character _character;

    [SerializeField] private GameObject factionGO;
    [SerializeField] private GameObject squadGO;
    [SerializeField] private GameObject partyGO;

    private bool disableParty;

    public void Initialize(ECS.Character character) {
        Initialize();
        SetCharacter(character);
    }
    public void Initialize() {
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
        UIManager.Instance.ShowSmallInfo(_character.faction.name);
    }
    public void ShowSquadInfo() {
        UIManager.Instance.ShowSmallInfo(_character.squad.name);
    }
    public void ShowPartyInfo() {
        UIManager.Instance.ShowSmallInfo(_character.currentParty.name);
    }
    public void OnClickPartyIcon() {
        UIManager.Instance.ShowPartyInfo(_character.currentParty);
    }
    public void HideInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
