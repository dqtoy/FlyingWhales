using UnityEngine;
using System.Collections;
using ECS;

public class Follower : CharacterRole {
    public Follower(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.FOLLOWER;
        //character.SetFollowerState(true);
    }
}
