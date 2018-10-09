using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionItem : MonoBehaviour {
    private Interaction _interaction;
    private ActionOption _currentSelectedActionOption;

    public CharacterPortrait portrait;
    public TextMeshProUGUI descriptionText;
    public ActionOptionButton[] actionOptionButtons;
    public Button confirmNoMinionButton;
    public Button confirmMinionButton;
    public GameObject confirmMinionGO;


    public void SetInteraction(Interaction interaction) {
        _interaction = interaction;
        UpdateState();
    }
    public void UpdateState() {
        portrait.GeneratePortrait(null, 50, true);
        descriptionText.text = _interaction.currentState.description;
        for (int i = 0; i < actionOptionButtons.Length; i++) {
            if(i < _interaction.currentState.actionOptions.Length) {
                actionOptionButtons[i].SetOption(_interaction.currentState.actionOptions[i]);
                actionOptionButtons[i].gameObject.SetActive(true);
            } else {
                actionOptionButtons[i].gameObject.SetActive(false);
            }
        }
        confirmNoMinionButton.gameObject.SetActive(false);
        confirmMinionGO.SetActive(false);
    }
    public void SetCurrentSelectedActionOption(ActionOption actionOption) {
        _currentSelectedActionOption = actionOption;
        if (_currentSelectedActionOption.needsMinion) {
            portrait.GeneratePortrait(null, 50, true);
            confirmMinionGO.SetActive(true);
            confirmNoMinionButton.gameObject.SetActive(false);
        } else {
            confirmMinionGO.SetActive(false);
            confirmNoMinionButton.gameObject.SetActive(true);
        }
    }
    public void OnClickConfirm() {
        _currentSelectedActionOption.ActivateOption(_interaction.interactable);
    }
    public void OnMinionDrop(Transform transform) {
        MinionItem minionItem = transform.GetComponent<MinionItem>();
        _currentSelectedActionOption.assignedMinion = minionItem.minion;
        portrait.GeneratePortrait(minionItem.portrait.portraitSettings, 50, true);
        if (_currentSelectedActionOption.assignedMinion != null) {
            confirmMinionButton.interactable = true;
        }
    }
}
