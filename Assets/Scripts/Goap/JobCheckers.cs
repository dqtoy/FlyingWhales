public partial class InteractionManager {
    
    public bool CanDoPatrolAndExplore(Character character) {
        return character.canCombat;
    }
    public bool IsSuicideJobStillValid(Character character) {
        return character.traitContainer.GetNormalTrait("Forlorn") != null;
    }
    public bool CanMoveOut(Character character) {
        TIME_IN_WORDS time = TIME_IN_WORDS.MORNING;
        if (character.traitContainer.GetNormalTrait("Nocturnal") != null) {
            //if nocturnal get after midnight
            time = TIME_IN_WORDS.AFTER_MIDNIGHT;
        }
        return character.role.roleType != CHARACTER_ROLE.LEADER &&
               GameManager.GetTimeInWordsOfTick(GameManager.Instance.tick) ==
               time; //Only non-leaders can take move out job, and it must also be in the morning time.
    }
    public bool CanDoCraftFurnitureJob(Character character, JobQueueItem item) {
        TILE_OBJECT_TYPE furnitureToCreate = ((item as GoapPlanJob).targetPOI as TileObject).tileObjectType;
        return furnitureToCreate.CanBeCraftedBy(character);
    }
    public bool CanDoDestroyProfaneJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoCombatJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoObtainFoodOutsideJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool CanDoObtainSupplyOutsideJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool CanDoHolyIncantationJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanDoExploreJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanDoCleanseRegionJob(Character character) {
        return character.traitContainer.GetNormalTrait("Purifier") != null;
//        return true;
    }
    public bool CanDoClaimRegionJob(Character character) {
        return character.traitContainer.GetNormalTrait("Royalty") != null;
    }
    public bool CanDoInvadeRegionJob(Character character) {
        return character.traitContainer.GetNormalTrait("Raider") != null;
    }
    public bool CanDoAttackNonDemonicRegionJob(Character character) {
        return true;
    }
    public bool CanDoAttackDemonicRegionJob(Character character) {
        return true;
    }
    public bool CanDoJudgementJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.LEADER;
    }
    public bool CanDoSabotageFactionJob(Character character) {
        return character.traitContainer.GetNormalTrait("Cultist") != null;
    }
    public bool CanCraftTool(Character character) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.TOOL);
        return SPECIAL_TOKEN.TOOL.CanBeCraftedBy(character);
    }
    public bool CanDoObtainSupplyJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanCharacterTakeBuildGoddessStatueJob(Character character) {
        return character.traitContainer.GetNormalTrait("Builder") != null;
    }
    public bool CanBrewPotion(Character character) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.HEALING_POTION);
        return SPECIAL_TOKEN.HEALING_POTION.CanBeCraftedBy(character);
    }
    public bool CanTakeBuryJob(Character character) {
        if (!character.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL) && character.isAtHomeRegion &&
            character.isPartOfHomeFaction
            && character.role.roleType != CHARACTER_ROLE.BEAST) {
            return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
                   character.role.roleType == CHARACTER_ROLE.CIVILIAN;
        }
        return false;
    }
    public bool CanCharacterTakeRemoveTraitJob(Character character, Character targetCharacter) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeRegion) {
            //if(job != null) {
            //    GoapPlanJob goapJob = job as GoapPlanJob;
            //    if (targetCharacter.traitContainer.GetNormalTrait((string) goapJob.goal.conditionKey).IsResponsibleForTrait(character)) {
            //        return false;
            //    }
            //}
            if (character.isFactionless) {
                return character.race == targetCharacter.race && character.homeArea == targetCharacter.homeArea &&
                       !targetCharacter.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TRAIT.ENEMY);
            }
            return !character.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo,
                RELATIONSHIP_TRAIT.ENEMY);
        }
        return false;
    }
    public bool CanCharacterTakeRemoveIllnessesJob(Character character, Character targetCharacter) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeRegion) {
            //if (job != null) {
            //    GoapPlanJob goapJob = job as GoapPlanJob;
            //    if (targetCharacter.traitContainer.GetNormalTrait((string) goapJob.goal.conditionKey).IsResponsibleForTrait(character)) {
            //        return false;
            //    }
            //    //try {
            //    //} catch {
            //    //    throw new Exception("Problem with CanCharacterTakeRemoveIllnessesJob of " + character.name + ". Target character is " + (targetCharacter?.name ?? "Null") + ". Job is " + (goapJob?.name ?? "Null"));
            //    //}
            //}
            if (character.isFactionless) {
                return character.race == targetCharacter.race && character.homeArea == targetCharacter.homeArea &&
                       !targetCharacter.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TRAIT.ENEMY);
            }
            return !character.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo,
                RELATIONSHIP_TRAIT.ENEMY); //&& character.traitContainer.GetNormalTrait("Healer") != null;
        }
        return false;
    }
    public bool CanCharacterTakeRemoveSpecialIllnessesJob(Character character, Character targetCharacter) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeRegion) {
            //if (job != null) {
            //    GoapPlanJob goapJob = job as GoapPlanJob;
            //    if (targetCharacter.traitContainer.GetNormalTrait((string) goapJob.goal.conditionKey).IsResponsibleForTrait(character)) {
            //        return false;
            //    }
            //    //try {
            //    //} catch {
            //    //    return false;
            //    //    //throw new Exception("Problem with CanCharacterTakeRemoveSpecialIllnessesJob of " + character.name + ". Target character is " + (targetCharacter?.name ?? "Null") + ". Job is " + (goapJob?.name ?? "Null"));
            //    //}

            //}
            if (character.isFactionless) {
                return character.race == targetCharacter.race && character.homeArea == targetCharacter.homeArea &&
                       !targetCharacter.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TRAIT.ENEMY);
            }
            return !character.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo,
                       RELATIONSHIP_TRAIT.ENEMY) && character.traitContainer.GetNormalTrait("Healer") != null;
        }
        return false;
    }
    public bool CanCharacterTakeApprehendJob(Character character, Character targetCharacter) {
        if (character.isAtHomeRegion && !character.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL) &&
            character.traitContainer.GetNormalTrait("Coward") == null && character.specificLocation.prison != null) {
            return character.role.roleType == CHARACTER_ROLE.SOLDIER &&
                   character.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) !=
                   RELATIONSHIP_EFFECT.POSITIVE;
        }
        return false;
    }
    public bool CanCharacterTakeRestrainJob(Character character, Character targetCharacter) {
        return targetCharacter.faction != character.faction && character.isAtHomeRegion &&
               character.isPartOfHomeFaction && character.specificLocation.prison != null
               && (character.role.roleType == CHARACTER_ROLE.SOLDIER ||
                   character.role.roleType == CHARACTER_ROLE.CIVILIAN ||
                   character.role.roleType == CHARACTER_ROLE.ADVENTURER)
               && character.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) !=
               RELATIONSHIP_EFFECT.POSITIVE && !character.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL);
    }
    public bool CanCharacterTakeRepairJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
               character.role.roleType == CHARACTER_ROLE.CIVILIAN ||
               character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanCharacterTakeReplaceTileObjectJob(Character character, JobQueueItem job) {
        object[] otherData = (job as GoapPlanJob).otherData[INTERACTION_TYPE.REPLACE_TILE_OBJECT];
        TileObject removedObj = otherData[0] as TileObject;
        return removedObj.tileObjectType.CanBeCraftedBy(character);
    }
    public bool CanCharacterTakeParalyzedFeedJob(Character sourceCharacter, Character character) {
        return sourceCharacter != character && sourceCharacter.faction == character.faction &&
               sourceCharacter.relationshipContainer.GetRelationshipEffectWith(character.currentAlterEgo) !=
               RELATIONSHIP_EFFECT.NEGATIVE;
    }
    public bool CanCharacterTakeRestrainedFeedJob(Character sourceCharacter, Character character) {
        if (sourceCharacter.specificLocation.region.IsResident(character)) {
            if (!character.isFactionless) {
                return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
                       character.role.roleType == CHARACTER_ROLE.CIVILIAN;
            }
            else {
                return character.role.roleType != CHARACTER_ROLE.BEAST &&
                       sourceCharacter.currentStructure.structureType.IsOpenSpace();
            }
        }
        return false;
    }
    public bool CanCharacterTakeDropJob(Character sourceCharacter, Character character) {
        return sourceCharacter != character && sourceCharacter.faction == character.faction &&
               character.relationshipContainer.GetRelationshipEffectWith(sourceCharacter.currentAlterEgo) !=
               RELATIONSHIP_EFFECT.NEGATIVE;
    }
    public bool CanCharacterTakeKnockoutJob(Character character, Character targetCharacter) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
               character.role.roleType ==
               CHARACTER_ROLE.ADVENTURER; // && !HasRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.POSITIVE)
    }
    public bool CanCharacterTakeBuildJob(Character character) {
        return character.traitContainer.GetNormalTrait("Builder") != null;
    }
    public bool CanCharacterTakeRepairStructureJob(Character character) {
        return character.traitContainer.GetNormalTrait("Builder") != null;
    }
}