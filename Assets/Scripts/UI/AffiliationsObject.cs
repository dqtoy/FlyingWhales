using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AffiliationsObject : MonoBehaviour {

    private Character _character;
    private enum HoveredObject {
        None,
        Faction,
        Party,
    }

    [SerializeField] private GameObject factionGO;
    [SerializeField] private GameObject partyGO;

    [SerializeField] private FactionEmblem factionEmblem;
    //[SerializeField] private SquadEmblem squadEmblem;

    private bool disableParty;
    private bool isHovering = false;
    private HoveredObject hoveredObject = HoveredObject.None;

    public void Initialize(Character character) {
        Initialize();
        SetCharacter(character);
    }
    public void Initialize() {
        Image[] factionChildImages = Utilities.GetComponentsInDirectChildren<Image>(factionGO);
        Messenger.AddListener(Signals.UPDATE_UI, UpdateAffiliations);
        Messenger.AddListener<Character, Squad>(Signals.SQUAD_MEMBER_ADDED, OnSquadEdited);
        Messenger.AddListener<Character, Squad>(Signals.SQUAD_MEMBER_REMOVED, OnSquadEdited);
        Messenger.AddListener<Character, Party>(Signals.CHARACTER_JOINED_PARTY, OnPartyEdited);
        Messenger.AddListener<Character, Party>(Signals.CHARACTER_LEFT_PARTY, OnPartyEdited);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnFactionEdited);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnFactionEdited);
    }
    public void Reset() {
        Messenger.RemoveListener<Character, Squad>(Signals.SQUAD_MEMBER_ADDED, OnSquadEdited);
        Messenger.RemoveListener<Character, Squad>(Signals.SQUAD_MEMBER_REMOVED, OnSquadEdited);
        Messenger.RemoveListener<Character, Party>(Signals.CHARACTER_JOINED_PARTY, OnPartyEdited);
        Messenger.RemoveListener<Character, Party>(Signals.CHARACTER_LEFT_PARTY, OnPartyEdited);
        Messenger.RemoveListener<Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnFactionEdited);
        Messenger.RemoveListener<Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnFactionEdited);
        Messenger.RemoveListener(Signals.UPDATE_UI, UpdateAffiliations);
        _character = null;
    }

    #region Signal Handlers
    private void OnSquadEdited(Character character, Squad affectedSquad) {
        if (_character == null) {
            return;
        }
        if (_character.id == character.id) {
            UpdateAffiliations();
        }
    }
    private void OnPartyEdited(Character character, Party affectedParty) {
        if (_character == null) {
            return;
        }
        if (_character.party == null || _character.currentParty.id == affectedParty.id) {
            UpdateAffiliations();
        }
    }
    private void OnFactionEdited(Character character, Faction affectedFaction) {
        if (_character == null) {
            return;
        }
        if (_character.id == character.id) {
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
        if (_character.faction == null) {
            factionGO.SetActive(false);
        } else {
            factionGO.SetActive(true);
            if (_character.faction != null) {
                factionEmblem.SetFaction(_character.faction);
            }
        }

        //if (_character.squad == null) {
        //    squadGO.SetActive(false);
        //} else {
        //    squadGO.SetActive(true);
        //    squadEmblem.SetSquad(_character.squad);
        //}

        if (disableParty || _character.currentParty.characters.Count <= 1) {
            partyGO.SetActive(false);
        } else {
            partyGO.SetActive(true);
        }
    }
    public void SetCharacter(Character character) {
        _character = character;
        UpdateAffiliations();
    }
    #endregion

    #region Pointer Functions
    public void ShowFactionInfo() {
        isHovering = true;
        hoveredObject = HoveredObject.Faction;
    }
    //public void ShowSquadInfo() {
    //    isHovering = true;
        //hoveredObject = HoveredObject.Squad;
    //}
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
                //case HoveredObject.Squad:
                //    UIManager.Instance.ShowSmallInfo(_character.squad.name);
                //    break;
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
