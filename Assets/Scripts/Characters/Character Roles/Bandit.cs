using UnityEngine;
using System.Collections;
using ECS;
using System.Collections.Generic;

public class Bandit : CharacterRole {
    public Bandit(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.BANDIT;

        _roleTasks.Add(new Rest(this._character));
        _roleTasks.Add(new MoveTo(this._character));
        _roleTasks.Add(new ExploreTile(this._character, 5));
        _roleTasks.Add(new UpgradeGear(this._character));
        _roleTasks.Add(new Pillage(this._character));
        _roleTasks.Add(new DoNothing(this._character));
		_roleTasks.Add(new Steal(this._character, 5));
    }
}
