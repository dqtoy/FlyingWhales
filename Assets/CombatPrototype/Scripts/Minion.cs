using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Minion {

    private MinionItem _minionItem;
    private PlayerAbility _ability;
    private ICharacter _icharacter;
    private IInteractable _target;

    private bool _isEnabled;


    #region getters/setters
    public PlayerAbility ability {
        get { return _ability; }
    }
    public ICharacter icharacter {
        get { return _icharacter; }
    }
    public bool isEnabled {
        get { return _isEnabled; }
    }
    #endregion

    public Minion(ICharacter icharacter, PlayerAbility ability) {
        _icharacter = icharacter;
        _ability = ability;
        _icharacter.ownParty.DetachActionData();
        PlayerManager.Instance.player.demonicPortal.AddCharacterHomeOnLandmark(_icharacter);
        PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(_icharacter.ownParty);
    }

    public void SendMinionToPerformAbility(IInteractable target) {
        _target = target;
        _icharacter.ownParty.GoToLocation(target.specificLocation, PATHFINDING_MODE.PASSABLE, () => DoAbility());
    }

    private void DoAbility() {
        _ability.DoAbility(_target);

        //Change activate button to recall button
    }

    public void SetEnabledState(bool state) {
        _isEnabled = state;
        _minionItem.SetEnabledState(state);
    }
    public void SetMinionItem(MinionItem minionItem) {
        _minionItem = minionItem;
    }
}
