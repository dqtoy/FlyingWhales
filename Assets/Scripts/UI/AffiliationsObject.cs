using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffiliationsObject : MonoBehaviour {

    private ECS.Character _character;

    [SerializeField] private GameObject factionGO;
    [SerializeField] private GameObject squadGO;
    [SerializeField] private GameObject partyGO;

    public void UpdateAffiliations(ECS.Character character) {
        _character = character;
        if (character.isFactionless) {
            factionGO.SetActive(false);
        } else {
            factionGO.SetActive(true);
        }

        if (character.squad == null) {
            squadGO.SetActive(false);
        } else {
            squadGO.SetActive(true);
        }

        if (character.currentParty.icharacters.Count <= 1) {
            partyGO.SetActive(false);
        } else {
            partyGO.SetActive(true);
        }
    }

    public void ShowFactionInfo() {
        UIManager.Instance.ShowSmallInfo(_character.faction.name);
    }
    public void ShowSquadInfo() {
        UIManager.Instance.ShowSmallInfo(_character.squad.name);
    }
    public void ShowPartyInfo() {
        UIManager.Instance.ShowSmallInfo(_character.currentParty.name);
    }
    public void HideInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
