using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PlayerAbility {
    protected string _name;
    protected string _description;
    protected int _powerCost;
    protected int _threatGain;
    protected int _cooldown;

    protected int _cooldownCount;
    protected bool _isEnabled;
    protected ABILITY_TYPE _type;
    protected PlayerAbilityButton _playerAbilityButton;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public ABILITY_TYPE type {
        get { return _type; }
    }
    #endregion

    public PlayerAbility(ABILITY_TYPE type) {
        _type = type;
    }

    #region Virtuals
    public virtual void Activate(IInteractable interactable) {
        PayPowerCost(interactable);
        ThreatGain();
        GoOnCooldown();
        PlayerUI.Instance.UpdateUI();
    }
    public virtual void CanBeDone() {

    }
    #endregion

    #region Utilities
    public void GoOnCooldown() {
        _cooldownCount = _cooldown;
        _playerAbilityButton.cooldownText.text = "(" + _cooldownCount + ")";
        _playerAbilityButton.cooldownText.gameObject.SetActive(true);
        SetIsEnabled(false);
        Messenger.AddListener(Signals.HOUR_STARTED, Cooldown);
    }
    private void Cooldown() {
        _cooldownCount--;
        _playerAbilityButton.cooldownText.text = "(" + _cooldownCount + ")";
        if (_cooldownCount <= 0) {
            StopCooldownAndEnableAbility();
        }
    }
    private void StopCooldownAndEnableAbility() {
        Messenger.RemoveListener(Signals.HOUR_STARTED, Cooldown);
        SetIsEnabled(true);
        _playerAbilityButton.cooldownText.gameObject.SetActive(false);
    }
    public void SetIsEnabled(bool state) {
        _isEnabled = state;
        _playerAbilityButton.button.interactable = state;
    }
    private void PayPowerCost(IInteractable interactable) {
        if(interactable is Character) {
            PlayerManager.Instance.player.AdjustBlueMagic(-_powerCost);
        }else if (interactable is BaseLandmark) {
            PlayerManager.Instance.player.AdjustGreenMagic(-_powerCost);
        }else if (interactable is Monster) {
            PlayerManager.Instance.player.AdjustRedMagic(-_powerCost);
        }
    }
    private void ThreatGain() {
        PlayerManager.Instance.player.AdjustThreatLevel(_threatGain);
    }
    public bool CanBeActivated(IInteractable interactable) {
        int magicUsed = 0;
        if (interactable is Character) {
            magicUsed = PlayerManager.Instance.player.blueMagic;
        } else if (interactable is BaseLandmark) {
            magicUsed = PlayerManager.Instance.player.greenMagic;
        } else if (interactable is Monster) {
            magicUsed = PlayerManager.Instance.player.redMagic;
        }
        if(magicUsed >= _powerCost) {
            return true;
        }
        return false;
    }
    #endregion

    #region UI
    public void SetPlayerAbilityButton(PlayerAbilityButton playerAbilityButton) {
        _playerAbilityButton = playerAbilityButton;
    }
    #endregion
}
