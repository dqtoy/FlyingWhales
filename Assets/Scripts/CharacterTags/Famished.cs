using UnityEngine;
using System.Collections;
using ECS;

public class Famished : CharacterAttribute {
    public Famished() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.FAMISHED) {

    }
}
