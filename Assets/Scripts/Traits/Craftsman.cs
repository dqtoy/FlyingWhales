using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Craftsman : Trait {

	//public SPECIAL_TOKEN[] craftedItemNames { get; private set; }
 //   public FURNITURE_TYPE[] craftedFurnitureNames { get; private set; }

    public Craftsman() {
        name = "Craftsman";
        //craftedItemNames = new SPECIAL_TOKEN[] { SPECIAL_TOKEN.TOOL, SPECIAL_TOKEN.HEALING_POTION };
        //craftedFurnitureNames = new FURNITURE_TYPE[] { FURNITURE_TYPE.BED, FURNITURE_TYPE.DESK, FURNITURE_TYPE.GUITAR, FURNITURE_TYPE.TABLE };
        description = "This character can create items.";
        type = TRAIT_TYPE.BUFF;
        effect = TRAIT_EFFECT.POSITIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        //effects = new List<TraitEffect>();
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CRAFT_ITEM_GOAP, INTERACTION_TYPE.CRAFT_FURNITURE };
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourcePOI) {
        //if(sourceCharacter.race == RACE.HUMANS) {
        //    craftedItemName = SPECIAL_TOKEN.HEALING_POTION;
        //}else if (sourceCharacter.race == RACE.ELVES) {
        //    craftedItemName = SPECIAL_TOKEN.GOLDEN_NECTAR;
        //}else if (sourceCharacter.race == RACE.FAERY) {
        //    craftedItemName = SPECIAL_TOKEN.ENCHANTED_AMULET;
        //}else if (sourceCharacter.race == RACE.GOBLIN) {
        //    craftedItemName = SPECIAL_TOKEN.JUNK;
        //}
        base.OnAddTrait(sourcePOI);
        //if (sourcePOI is Character) {
        //    Character character = sourcePOI as Character;
        //    for (int i = 0; i < advertisedInteractions.Count; i++) {
        //        character.AddInteractionType(advertisedInteractions[i]);
        //    }
        //}
    }
    public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
        //if (sourcePOI is Character) {
        //    Character character = sourcePOI as Character;
        //    for (int i = 0; i < advertisedInteractions.Count; i++) {
        //        character.RemoveInteractionType(advertisedInteractions[i]);
        //    }
        //}
        base.OnRemoveTrait(sourcePOI, removedBy);
    }
    #endregion
}
