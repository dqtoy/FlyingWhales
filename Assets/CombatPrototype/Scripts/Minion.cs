using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Minion {

    private PlayerAbility _ability;
    private ICharacter _icharacter;
    private IInteractable _target;

    #region getters/setters
    public PlayerAbility ability {
        get { return _ability; }
    }
    public ICharacter icharacter {
        get { return _icharacter; }
    }
    #endregion

    public Minion(ICharacter icharacter, PlayerAbility ability) {
        _icharacter = icharacter;
        _ability = ability;
        _icharacter.ownParty.DetachActionData();
    }

    public void SendMinionToPerformAbility(IInteractable target) {
        _target = target;
        _icharacter.ownParty.GoToLocation(target.specificLocation, PATHFINDING_MODE.PASSABLE, () => ActivateAbility());
    }

    private void ActivateAbility() {
        _ability.Activate(_target);

        //Change activate button to recall button
    }
}
