using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adventurer : CharacterRole {
    public override int reservedSupply { get { return 50; } }

    public Adventurer() : base(CHARACTER_ROLE.ADVENTURER, "Normal", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY, INTERACTION_CATEGORY.RECRUITMENT, INTERACTION_CATEGORY.EXPANSION, INTERACTION_CATEGORY.DEFENSE }) {
    }

    #region Overrides
    public override void AddRoleWorkPlansToCharacterWeights(WeightedDictionary<INTERACTION_TYPE> weights) {
        base.AddRoleWorkPlansToCharacterWeights(weights);
        weights.AddElement(INTERACTION_TYPE.EXPLORE, 5);
    }
    #endregion
}
