using UnityEngine;
using System.Collections;
using ECS;

public class AncientVampire : CharacterRole {

    public AncientVampire(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.ANCIENT_VAMPIRE;

        _roleTasks.Clear();
        _roleTasks.Add(new MoveTo(this._character));
        _roleTasks.Add(new DoNothing(this._character));
        _roleTasks.Add(new Hibernate(this._character));
        _roleTasks.Add(new HuntMagicUser(this._character));
        _roleTasks.Add(new Rest(this._character));
        _defaultRoleTask = _roleTasks[2];
    }

    #region overrides
    public override void OnAssignRole() {
        base.OnAssignRole();
        _character.SetTaskToDoNext(_defaultRoleTask); //Set ancient vampire to be initially hibernating
    }
    #endregion
}
