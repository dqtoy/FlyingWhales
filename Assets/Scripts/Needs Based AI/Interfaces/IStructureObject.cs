using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStructureObject : IObject {
    int maxHP { get; }
    void AdjustHP(int amount);
}