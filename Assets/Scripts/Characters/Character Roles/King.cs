using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class King : CharacterRole {

    public King(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.KING;

        SetFullness(1000);
        SetEnergy(1000);
        SetFun(600);
        SetPrestige(400);
        SetSanity(1000);
        UpdateSafety();
        UpdateHappiness();
    }

    #region Overrides
    public override void DeathRole() {
        base.DeathRole();
        _character.SetIsIdle(false);
    }
    public override void ChangedRole() {
        base.ChangedRole();
        _character.SetIsIdle(false);
    }
    public override void OnAssignRole() {
        base.OnAssignRole();
        _character.SetIsIdle(true);
    }
    #endregion
}
