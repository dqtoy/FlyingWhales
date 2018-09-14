using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAbilityButton : MonoBehaviour {
    private PlayerAbility _playerAbility;

    public Text buttonText;
    public Text cooldownText;
    public Button button;

    #region getters/setters
    public PlayerAbility playerAbility {
        get { return _playerAbility; }
    }
    #endregion
    public void SetPlayerAbility(PlayerAbility playerAbility) {
        _playerAbility = playerAbility;
        buttonText.text = _playerAbility.name.ToUpper();
    }

    public void OnClickPlayerAbility() {
        _playerAbility.Activate(PlayerAbilitiesUI.Instance.currentlySelectedInteractable);
    }
}
