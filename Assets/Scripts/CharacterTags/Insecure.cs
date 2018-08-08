using UnityEngine;
using System.Collections;
using ECS;

public class Insecure : CharacterAttribute {
    public Insecure(Character character) : base(character, ATTRIBUTE.INSECURE) {

    }
}
