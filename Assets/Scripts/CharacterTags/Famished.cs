using UnityEngine;
using System.Collections;
using ECS;

public class Famished : Attribute {
    public Famished() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.FAMISHED) {

    }
}
