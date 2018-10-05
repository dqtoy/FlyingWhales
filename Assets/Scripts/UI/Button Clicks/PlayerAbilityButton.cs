using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAbilityButton : MonoBehaviour {
    private PlayerAbility _playerAbility;

    public Text buttonText;
    public Text cooldownText;
    public Button button;

    private bool _canBeDone;

    #region getters/setters
    public PlayerAbility playerAbility {
        get { return _playerAbility; }
    }
    public bool canBeDone {
        get { return _canBeDone; }
    }
    #endregion

    public void SetPlayerAbility(PlayerAbility playerAbility) {
        _playerAbility = playerAbility;
        buttonText.text = _playerAbility.name.ToUpper();
    }
    public void SetCanBeDone(bool state) {
        _canBeDone = state;
    }
    public void OnClickPlayerAbility() {
        //_playerAbility.Activate(PlayerAbilitiesUI.Instance.currentlySelectedInteractable);

        if(buttonText.text == "RECALL") {
            _playerAbility.RecallMinion();
            //Go back home, once home, let the 
        } else {
            //Open minion assignment ui
            MinionAssignmentUI.Instance.SetCurrentAbility(_playerAbility);
            MinionAssignmentUI.Instance.OpenUI();
        }
    }

    public void EnableDisable() {
        if (_playerAbility.isEnabled) {
            if (_playerAbility.assignedMinionsAndTarget.ContainsValue(PlayerAbilitiesUI.Instance.currentlySelectedInteractable)) {
                ChangeToRecallState();
            } else {
                ChangeToActivateState();
                button.interactable = _canBeDone;
            }
        } else {
            button.interactable = false;
        }
    }
    public void UpdateThis(IInteractable interactable) {
        if (gameObject.activeSelf) {
            SetCanBeDone(_playerAbility.CanBeDone(interactable));
            EnableDisable();
        }
    }
    public void ChangeToRecallState() {
        buttonText.text = "RECALL";
    }
    public void ChangeToActivateState() {
        buttonText.text = _playerAbility.name;
    }
}
