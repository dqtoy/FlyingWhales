using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Craftsman : Trait {

	public SPECIAL_TOKEN[] craftedItemNames { get; private set; }

    public Craftsman() {
        name = "Craftsman";
        craftedItemNames = new SPECIAL_TOKEN[] { SPECIAL_TOKEN.TOOL };
        description = "This character can create items.";
        type = TRAIT_TYPE.ABILITY;
        effect = TRAIT_EFFECT.POSITIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CRAFT_ITEM };
    }

    #region Overrides
    public override void OnAddTrait(Character sourceCharacter) {
        //if(sourceCharacter.race == RACE.HUMANS) {
        //    craftedItemName = SPECIAL_TOKEN.HEALING_POTION;
        //}else if (sourceCharacter.race == RACE.ELVES) {
        //    craftedItemName = SPECIAL_TOKEN.GOLDEN_NECTAR;
        //}else if (sourceCharacter.race == RACE.FAERY) {
        //    craftedItemName = SPECIAL_TOKEN.ENCHANTED_AMULET;
        //}else if (sourceCharacter.race == RACE.GOBLIN) {
        //    craftedItemName = SPECIAL_TOKEN.JUNK;
        //}
        sourceCharacter.AddInteractionType(INTERACTION_TYPE.CRAFT_ITEM);
    }
    public override void OnRemoveTrait(Character sourceCharacter) {
        sourceCharacter.RemoveInteractionType(INTERACTION_TYPE.CRAFT_ITEM);
    }
    #endregion
}
