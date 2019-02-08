using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureTrait {

	public string name { get; protected set; }
    public LocationStructure owner { get; protected set; }

    public StructureTrait(LocationStructure owner) {
        this.owner = owner;
    }

    #region virtuals
    public virtual void OnCharacterEnteredStructure(Character character) { }
    #endregion
}
