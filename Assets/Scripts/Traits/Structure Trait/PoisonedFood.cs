using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonedFood : StructureTrait {

    public PoisonedFood(LocationStructure owner) : base(owner) {
        name = "Poisoned Food";
    }
}
