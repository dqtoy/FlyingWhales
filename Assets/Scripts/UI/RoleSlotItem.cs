using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoleSlotItem : MonoBehaviour {

    private Character character;

    [SerializeField] private JOB slotJob;
    [SerializeField] private Image jobIcon;
    [SerializeField] private TextMeshProUGUI jobNameLbl;
    [SerializeField] private CharacterPortrait portrait;

    public void SetSlotJob(JOB job) {
        slotJob = job;
        UpdateVisuals();
        Messenger.AddListener<JOB, Character>(Signals.CHARACTER_ASSIGNED_TO_JOB, OnCharacterAssignedToJob);
        Messenger.AddListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
    }

    public void SetCharacter(Character character) {
        this.character = character;
        if (character == null) {
            Debug.Log("Setting character in role slot " + slotJob.ToString() + " to null");
        } else {
            Debug.Log("Setting character in role slot " + slotJob.ToString() + " to " + character.name);
        }
        
        UpdateVisuals();
    }
    private void UpdateVisuals() {
        jobIcon.sprite = CharacterManager.Instance.GetJobSprite(slotJob);
        jobNameLbl.text = Utilities.NormalizeString(slotJob.ToString());
        if (character == null) {
            portrait.gameObject.SetActive(false);
        } else {
            portrait.GeneratePortrait(character);
            portrait.gameObject.SetActive(true);
        }
    }
	
    public void OnClickAssign() {
        UIManager.Instance.ShowClickableObjectPicker(PlayerManager.Instance.player.allOwnedCharacters, AssignCharacterToJob, new CharacterLevelComparer(), CanAssignCharacterToJob);
    }
    private bool CanAssignCharacterToJob(Character character) {
        if (this.character != null && this.character.id == character.id) {
            return false; //This means that the character is already assigned to this job
        }
        return PlayerManager.Instance.player.CanAssignCharacterToJob(slotJob, character);
    }
    private void AssignCharacterToJob(Character character) {
        Debug.Log("Assigning " + character.name + " to job " + slotJob.ToString());
        PlayerManager.Instance.player.AssignCharacterToJob(slotJob, character);
        UIManager.Instance.HideObjectPicker();
    }

    private void OnCharacterAssignedToJob(JOB job, Character character) {
        if (slotJob == job) {
            SetCharacter(character);
        }
    }
    private void OnCharacterUnassignedFromJob(JOB job, Character character) {
        if (slotJob == job) {
            SetCharacter(null);
        }
    }
}

public class CharacterLevelComparer : IComparer<Character> {
    public int Compare(Character x, Character y) {
        return x.level.CompareTo(y.level);
    }
}
