using UnityEngine;
using System.Collections;
using ECS;

public class Insecure : CharacterAttribute {
    public Insecure() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.INSECURE) {

    }
}
