using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Impulsive : Attribute {
    public Impulsive() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.IMPULSIVE) {
    }

}
