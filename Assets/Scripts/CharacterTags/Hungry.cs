using UnityEngine;
using System.Collections;
using ECS;

public class Hungry : CharacterTag {
    public Hungry(Character character) : base(character, CHARACTER_TAG.HUNGRY) {

    }
}