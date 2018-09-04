using UnityEngine;
using System.Collections;
using ECS;

public class Famished : CharacterAttribute {
    public Famished(Character character) : base(character, ATTRIBUTE.FAMISHED) {

    }
}
