using UnityEngine;
using System.Collections;
using ECS;

public class Successor : CharacterTag {
    public Successor(Character character) : base(character, CHARACTER_TAG.SUCCESSOR) {
    }
}
