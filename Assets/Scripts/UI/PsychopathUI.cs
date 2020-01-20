using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Traits;

public class PsychopathUI : MonoBehaviour {
    public PsychopathRequirementsUI requirements1UI;
    public PsychopathRequirementsUI requirements2UI;
    public Button okButton;

    public Character character { get; private set; }

    public void ShowPsychopathUI(Character character) {
        this.character = character;
        requirements1UI.ShowRequirementsUI();
        requirements2UI.ShowRequirementsUI();
        gameObject.SetActive(true);
    }
    public void HidePsychopathUI() {
        character = null;
        gameObject.SetActive(false);
    }

    public void OnClickOK() {
        if (requirements1UI.victimType == SERIAL_VICTIM_TYPE.NONE && requirements2UI.victimType == SERIAL_VICTIM_TYPE.NONE) {
            PlayerUI.Instance.ShowGeneralConfirmation("Error", "Must have at least 1 requirement.");
            return;
        }
        if (requirements1UI.victimType == requirements2UI.victimType) {
            PlayerUI.Instance.ShowGeneralConfirmation("Error", "Cannot have the same requirements.");
            return;
        }
        if (requirements1UI.victimType == SERIAL_VICTIM_TYPE.RACE || requirements1UI.victimType == SERIAL_VICTIM_TYPE.TRAIT) {
            if(requirements1UI.victimDescriptions == null || requirements1UI.victimDescriptions.Count <= 0) {
                PlayerUI.Instance.ShowGeneralConfirmation("Error", "Requirements are lacking. Please check again.");
                return;
            }
        }
        if (requirements2UI.victimType == SERIAL_VICTIM_TYPE.RACE || requirements2UI.victimType == SERIAL_VICTIM_TYPE.TRAIT) {
            if (requirements2UI.victimDescriptions == null || requirements2UI.victimDescriptions.Count <= 0) {
                PlayerUI.Instance.ShowGeneralConfirmation("Error", "Requirements are lacking. Please check again.");
                return;
            }
        }
        SerialKiller serialKillerTrait = new SerialKiller();
        character.traitContainer.AddTrait(character, serialKillerTrait);
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Serial Killer", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);

        if (requirements1UI.victimType == SERIAL_VICTIM_TYPE.GENDER || requirements1UI.victimType == SERIAL_VICTIM_TYPE.RACE) {
            requirements1UI.victimDescriptions.Add(requirements1UI.reqDescriptionDropdown.options[requirements1UI.reqDescriptionDropdown.value].text);
        }
        if (requirements2UI.victimType == SERIAL_VICTIM_TYPE.GENDER || requirements2UI.victimType == SERIAL_VICTIM_TYPE.RACE) {
            requirements2UI.victimDescriptions.Add(requirements2UI.reqDescriptionDropdown.options[requirements2UI.reqDescriptionDropdown.value].text);
        }

        serialKillerTrait.SetVictimRequirements(requirements1UI.victimType, requirements1UI.victimDescriptions, requirements2UI.victimType, requirements2UI.victimDescriptions);
        HidePsychopathUI();
    }
}
