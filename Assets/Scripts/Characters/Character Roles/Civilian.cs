using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Civilian : CharacterRole {

    public Civilian(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.CIVILIAN;

        SetFullness(100);
        SetEnergy(100);
        SetFun(60);
        SetPrestige(40);
        SetSanity(100);
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
