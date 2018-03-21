using UnityEngine;
using System.Collections;
using ECS;

public class Hermit : CharacterRole {
    public Hermit(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.HERMIT;

        _roleTasks.Add(new DoNothing(this._character));
        _roleTasks.Add(new Rest(this._character));
        _roleTasks.Add(new MoveTo(this._character));
        _roleTasks.Add(new ExploreTile(this._character));
        _defaultRoleTask = _roleTasks[2];
    }
}
