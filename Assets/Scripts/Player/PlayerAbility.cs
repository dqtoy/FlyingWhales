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
    protected Dictionary<Minion, IInteractable> _assignedMinionsAndTarget;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public bool isEnabled {
        get { return _isEnabled; }
    }
    public ABILITY_TYPE type {
        get { return _type; }
    }
    public PlayerAbilityButton playerAbilityButton {
        get { return _playerAbilityButton; }
    }
    public Dictionary<Minion, IInteractable> assignedMinionsAndTarget {
        get { return _assignedMinionsAndTarget; }
    }
    #endregion

    public PlayerAbility(ABILITY_TYPE type) {
        _type = type;
        _isEnabled = true;
        _assignedMinionsAndTarget = new Dictionary<Minion, IInteractable>();
    }

    #region Virtuals
    public virtual void Activate(IInteractable interactable, Minion assignedMinion) {
        _assignedMinionsAndTarget.Add(assignedMinion, interactable);
        assignedMinion.SetEnabledState(false);
        assignedMinion.SendMinionToPerformAbility(interactable);

        PayPowerCost(interactable);
        ThreatGain();
        //GoOnCooldown();
        //_playerAbilityButton.SetCanBeDone(CanBeDone(interactable));
        _playerAbilityButton.UpdateThis(interactable);
        PlayerUI.Instance.UpdateUI();
    }
    public virtual bool CanBeDone(IInteractable interactable) {
        return CanBeActivated(interactable);
    }
    public virtual bool CanBeActivated(IInteractable interactable) {
        if (interactable.isBeingInspected || GameManager.Instance.inspectAll) {
            int magicUsed = 0;
            if (interactable is Character) {
                magicUsed = PlayerManager.Instance.player.blueMagic;
            } else if (interactable is BaseLandmark) {
                magicUsed = PlayerManager.Instance.player.greenMagic;
            } else if (interactable is Monster) {
                magicUsed = PlayerManager.Instance.player.redMagic;
            }
            if (magicUsed >= _powerCost) {
                return true;
            }
        }
        return false;
    }
    public virtual void DoAbility(IInteractable interactable) {
        
    }
    public virtual void CancelAbility(IInteractable interactable) {

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
        _playerAbilityButton.EnableDisable();
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
    public string GetMagicCostString(IInteractable interactable) {
        if (interactable is Character) {
            return "Blue Magic: " + _powerCost;
        } else if (interactable is BaseLandmark) {
            return "Green Magic: " + _powerCost;
        } else if (interactable is Monster) {
            return "Red Magic: " + _powerCost;
        }
        return string.Empty;
    }
    public void RecallMinion() {
        Minion currentMinion = null;
        foreach (Minion minion in _assignedMinionsAndTarget.Keys) {
            if(_assignedMinionsAndTarget[minion] == PlayerAbilitiesUI.Instance.currentlySelectedInteractable) {
                currentMinion = minion;
                break;
            }
        }
        _assignedMinionsAndTarget.Remove(currentMinion);
        _playerAbilityButton.EnableDisable();
        CancelAbility(PlayerAbilitiesUI.Instance.currentlySelectedInteractable);

        //Go home minion, when reached home, enable the minion again
    }
    #endregion

    #region UI
    public void SetPlayerAbilityButton(PlayerAbilityButton playerAbilityButton) {
        _playerAbilityButton = playerAbilityButton;
    }
    #endregion
}
