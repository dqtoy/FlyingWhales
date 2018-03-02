using UnityEngine;
using System.Collections;
using ECS;

public class AncientVampire : CharacterRole {

    public AncientVampire(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.ANCIENT_VAMPIRE;

        _roleTasks.Add(new DoNothing(this._character));
        _roleTasks.Add(new Hibernate(this._character));
        _defaultRoleTask = _roleTasks[1];
    }
}
