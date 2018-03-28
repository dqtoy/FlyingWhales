using UnityEngine;
using System.Collections;
using ECS;

public class Beast : CharacterRole {
    public Beast(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.BEAST;

        _roleTasks.Add(new Rest(this._character));
        _roleTasks.Add(new MoveToBeast(this._character));
        _roleTasks.Add(new DoNothing(this._character));
        _roleTasks.Add(new Prowl(this._character));
    }
}
