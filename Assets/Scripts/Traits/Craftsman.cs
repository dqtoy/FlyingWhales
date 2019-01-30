using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Craftsman : Trait {

	public string craftedItemName { get; private set; }

    public Craftsman() {
        name = "Craftsman";
        craftedItemName = string.Empty;
        description = "This character can create items.";
        type = TRAIT_TYPE.ABILITY;
        effect = TRAIT_EFFECT.POSITIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(Character sourceCharacter) {
        if(sourceCharacter.race == RACE.HUMANS) {
            craftedItemName = "Healing Potion";
        }else if (sourceCharacter.race == RACE.ELVES) {
            craftedItemName = "Golden Nectar";
        }else if (sourceCharacter.race == RACE.FAERY) {
            craftedItemName = "Enchanted Amulet";
        }else if (sourceCharacter.race == RACE.GOBLIN) {
            craftedItemName = "Junk";
        }
    }
    #endregion
}
