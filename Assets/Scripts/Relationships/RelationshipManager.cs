using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class RelationshipManager : MonoBehaviour {

    public static RelationshipManager Instance = null;


    void Awake() {
        Instance = this;
        //TODO: Use Reflection.
        //validators
        new CharacterRelationshipValidator();
        //processors
        new CharacterRelationshipProcessor();
    }

    #region Containers
    public IRelationshipContainer CreateRelationshipContainer(Relatable relatable) {
        if (relatable is IPointOfInterest || relatable is AlterEgoData) {
            return new POIRelationshipContainer();
        }
        return null;
    }
    #endregion

    #region Validators
    public IRelationshipValidator GetValidator(Relatable obj) {
        if (obj is AlterEgoData) {
            return CharacterRelationshipValidator.Instance;
        }
        return null;
    }
    public bool CanHaveRelationship(Relatable rel1, Relatable rel2, RELATIONSHIP_TRAIT rel) {
        IRelationshipValidator validator = GetValidator(rel1);
        if (validator != null) {
            return validator.CanHaveRelationship(rel1, rel2, rel);
        }
        return false; //if no validator, then do not allow
    }
    #endregion

    #region Relationship Effects
    public RELATIONSHIP_EFFECT GetRelationshipEffect(RELATIONSHIP_TRAIT relType) {
        switch (relType) {
            case RELATIONSHIP_TRAIT.ENEMY:
                return RELATIONSHIP_EFFECT.NEGATIVE;
            case RELATIONSHIP_TRAIT.FRIEND:
                return RELATIONSHIP_EFFECT.POSITIVE;
            case RELATIONSHIP_TRAIT.RELATIVE:
                return RELATIONSHIP_EFFECT.POSITIVE;
            case RELATIONSHIP_TRAIT.LOVER:
                return RELATIONSHIP_EFFECT.POSITIVE;
            case RELATIONSHIP_TRAIT.PARAMOUR:
                return RELATIONSHIP_EFFECT.POSITIVE;
            default:
                return RELATIONSHIP_EFFECT.NONE;
        }
    }
    #endregion

    /// <summary>
    /// Add a one way relationship to a character.
    /// </summary>
    /// <param name="currCharacter">The character that will gain the relationship.</param>
    /// <param name="targetCharacter">The character that the new relationship is targetting.</param>
    /// <param name="rel">The type of relationship to create.</param>
    /// <param name="triggerOnAdd">Should this trigger the trait's OnAdd Function.</param>
    /// <returns>The created relationship data.</returns>
    private RELATIONSHIP_TRAIT GetPairedRelationship(RELATIONSHIP_TRAIT rel) {
        switch (rel) {
            case RELATIONSHIP_TRAIT.ENEMY:
                return RELATIONSHIP_TRAIT.ENEMY;
            case RELATIONSHIP_TRAIT.FRIEND:
                return RELATIONSHIP_TRAIT.FRIEND;
            case RELATIONSHIP_TRAIT.RELATIVE:
                return RELATIONSHIP_TRAIT.RELATIVE;
            case RELATIONSHIP_TRAIT.LOVER:
                return RELATIONSHIP_TRAIT.LOVER;
            case RELATIONSHIP_TRAIT.PARAMOUR:
                return RELATIONSHIP_TRAIT.PARAMOUR;
            case RELATIONSHIP_TRAIT.MASTER:
                return RELATIONSHIP_TRAIT.SERVANT;
            case RELATIONSHIP_TRAIT.SERVANT:
                return RELATIONSHIP_TRAIT.MASTER;
            case RELATIONSHIP_TRAIT.SAVER:
                return RELATIONSHIP_TRAIT.SAVE_TARGET;
            case RELATIONSHIP_TRAIT.SAVE_TARGET:
                return RELATIONSHIP_TRAIT.SAVER;
            default:
                return RELATIONSHIP_TRAIT.NONE;
        }
    }
    //public void GenerateRelationships() {
    //    int maxInitialRels = 4;
    //    RELATIONSHIP_TRAIT[] relsInOrder = new RELATIONSHIP_TRAIT[] { RELATIONSHIP_TRAIT.RELATIVE, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.ENEMY, RELATIONSHIP_TRAIT.FRIEND };

    //    // Loop through all characters in the world
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        Character currCharacter = CharacterManager.Instance.allCharacters[i];
    //        if (currCharacter.isFactionless) {
    //            continue; //skip factionless characters
    //        }
    //        int currentRelCount = currCharacter.relationshipContainer.relationships.Count;
    //        if (currentRelCount >= maxInitialRels) {
    //            continue; //skip
    //        }
    //        int totalCreatedRels = currentRelCount;
    //        string summary = currCharacter.name + "(" + currCharacter.sexuality.ToString() + ") relationship generation summary:";

    //        //  Loop through all relationship types
    //        for (int k = 0; k < relsInOrder.Length; k++) {
    //            RELATIONSHIP_TRAIT currRel = relsInOrder[k];
    //            if (totalCreatedRels >= maxInitialRels) {
    //                summary += "\nMax Initial Relationships reached, stopping relationship generation for " + currCharacter.name;
    //                break; //stop generating more relationships for this character
    //            }
    //            int relsToCreate = 0;
    //            int chance = Random.Range(0, 100);

    //            // Compute the number of relations to create per relationship type
    //            switch (currRel) {
    //                case RELATIONSHIP_TRAIT.RELATIVE:
    //                    if (currCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no relatives
    //                    else {
    //                        //- a non-beast character may have either zero (75%), one (20%) or two (5%) relatives from characters of the same race
    //                        if (chance < 75) relsToCreate = 0;
    //                        else if (chance >= 75 && chance < 95) relsToCreate = 1;
    //                        else relsToCreate = 2;
    //                    }
    //                    break;
    //                case RELATIONSHIP_TRAIT.LOVER:
    //                    //- a character has a 20% chance to have a lover
    //                    if (chance < 20) relsToCreate = 1;
    //                    //relsToCreate = 1;
    //                    break;
    //                case RELATIONSHIP_TRAIT.ENEMY:
    //                    //- a character may have either zero (75%), one (20%) or two (5%) enemies
    //                    if (chance < 75) relsToCreate = 0;
    //                    else if (chance >= 75 && chance < 95) relsToCreate = 1;
    //                    else relsToCreate = 2;
    //                    //relsToCreate = 2;
    //                    break;
    //                case RELATIONSHIP_TRAIT.FRIEND:
    //                    //- a character may have either zero (65%), one (25%) or two (10%) friends
    //                    if (chance < 65) relsToCreate = 0;
    //                    else if (chance >= 65 && chance < 90) relsToCreate = 1;
    //                    else relsToCreate = 2;
    //                    break;
    //                    //case RELATIONSHIP_TRAIT.PARAMOUR:
    //                    //    //- only valid for non-beast characters with lovers
    //                    //    if (currCharacter.role.roleType == CHARACTER_ROLE.BEAST || currCharacter.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER) == null) { continue; }
    //                    //    //- a character may have either zero (85%) or one (15%) paramour
    //                    //    //if (chance < 85) relsToCreate = 0;
    //                    //    //else relsToCreate = 1;
    //                    //    relsToCreate = 1;
    //                    //    break;
    //            }
    //            summary += "\n===========Creating " + relsToCreate + " " + currRel.ToString() + " Relationships...==========";


    //            if (relsToCreate > 0) {
    //                WeightedFloatDictionary<Character> relWeights = new WeightedFloatDictionary<Character>();
    //                // Loop through all characters that are in the same faction as the current character
    //                for (int l = 0; l < currCharacter.faction.characters.Count; l++) {
    //                    Character otherCharacter = currCharacter.faction.characters[l];
    //                    if (currCharacter.id != otherCharacter.id) { //&& currCharacter.faction == otherCharacter.faction
    //                        List<RELATIONSHIP_TRAIT> existingRelsOfCurrentCharacter = currCharacter.relationshipContainer.GetRelationshipDataWith(otherCharacter.currentAlterEgo)?.relationships ?? null;
    //                        List<RELATIONSHIP_TRAIT> existingRelsOfOtherCharacter = otherCharacter.relationshipContainer.GetRelationshipDataWith(currCharacter.currentAlterEgo)?.relationships ?? null ;
    //                        //if the current character already has a relationship of the same type with the other character, skip
    //                        if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(currRel)) {
    //                            continue; //skip
    //                        }
    //                        float weight = 0;

    //                        // Compute the weight that determines how likely this character will have the current relationship type with current character
    //                        switch (currRel) {
    //                            case RELATIONSHIP_TRAIT.RELATIVE:
    //                                if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no relatives
    //                                else {
    //                                    if (otherCharacter.specificLocation.id == currCharacter.specificLocation.id) {
    //                                        // character is in same location: +50 Weight
    //                                        weight += 50;
    //                                    } else {
    //                                        //character is in different location: +10 Weight
    //                                        weight += 10;
    //                                    }

    //                                    if (currCharacter.race != otherCharacter.race) weight *= 0; //character is a different race: Weight x0
    //                                    if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
    //                                        //Disabled possiblity that relatives can be friends
    //                                        weight *= 0;
    //                                    }
    //                                    if (currCharacter.faction != otherCharacter.faction) {
    //                                        weight *= 0; //disabled different faction positive relationships
    //                                    }
    //                                }
    //                                break;
    //                            case RELATIONSHIP_TRAIT.LOVER:
    //                                if (GetValidator(currCharacter).CanHaveRelationship(currCharacter, otherCharacter, currRel) && GetValidator(otherCharacter).CanHaveRelationship(otherCharacter, currCharacter, currRel)) {
    //                                    if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
    //                                        //- if non beast, from valid characters, choose based on these weights
    //                                        if (otherCharacter.specificLocation.id == currCharacter.specificLocation.id) {
    //                                            //- character is in same location: +500 Weight
    //                                            weight += 500;
    //                                        } else {
    //                                            //- character is in different location: +5 Weight
    //                                            weight += 5;
    //                                        }
    //                                        if (currCharacter.race == otherCharacter.race) {
    //                                            //- character is the same race: Weight x5
    //                                            weight *= 5;
    //                                        }
    //                                        if (!IsSexuallyCompatible(currCharacter, otherCharacter)) {
    //                                            //- character is sexually incompatible: Weight x0.1
    //                                            weight *= 0.05f;
    //                                        }
    //                                        if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
    //                                            //- character is a beast: Weight x0
    //                                            weight *= 0;
    //                                        }
    //                                        if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
    //                                            //- character is a relative: Weight x0.1    
    //                                            weight *= 0.1f;
    //                                        }
    //                                        if (currCharacter.faction != otherCharacter.faction) {
    //                                            weight *= 0; //disabled different faction positive relationships
    //                                        }
    //                                    } else {
    //                                        //- if beast, from valid characters, choose based on these weights
    //                                        if (otherCharacter.specificLocation.id == currCharacter.specificLocation.id) {
    //                                            //- character is in same location: +50 Weight
    //                                            weight += 50;
    //                                        } else {
    //                                            // - character is in different location: +5 Weight
    //                                            weight += 5;
    //                                        }
    //                                        if (currCharacter.race != otherCharacter.race) {
    //                                            //- character is a different race: Weight x0
    //                                            weight *= 0;
    //                                        }
    //                                        if (currCharacter.gender != otherCharacter.gender) {
    //                                            //- character is the opposite gender: Weight x6
    //                                            weight *= 6;
    //                                        }
    //                                    }
    //                                }
    //                                break;
    //                            case RELATIONSHIP_TRAIT.ENEMY:
    //                                if (GetValidator(currCharacter).CanHaveRelationship(currCharacter, otherCharacter, currRel) && GetValidator(otherCharacter).CanHaveRelationship(otherCharacter, currCharacter, currRel)) {
    //                                    if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
    //                                        //- if non beast, from valid characters, choose based on these weights
    //                                        if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
    //                                            // - character is non-beast: +50 Weight
    //                                            weight += 50;
    //                                        } else {
    //                                            //- character is a beast: +5 Weight
    //                                            weight += 5;
    //                                        }
    //                                        if (currCharacter.faction.id == otherCharacter.faction.id) {
    //                                            //- character is from same faction: Weight x6
    //                                            weight *= 6;
    //                                        }
    //                                        if (currCharacter.race != otherCharacter.race) {
    //                                            //- character is a different race: Weight x2
    //                                            weight *= 2;
    //                                        }

    //                                        if (existingRelsOfCurrentCharacter != null) {
    //                                            //DISABLED, because of Ask for Help
    //                                            //if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
    //                                            //    //- character is a relative: Weight x0.5
    //                                            //    weight *= 0.5f;
    //                                            //}
    //                                            if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.LOVER)) {
    //                                                //- character is a lover: Weight x0
    //                                                weight *= 0;
    //                                            }
    //                                        }
    //                                        if (existingRelsOfOtherCharacter != null) {
    //                                            //- character considers this one as an Enemy: Weight x6
    //                                            if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
    //                                                weight *= 6;
    //                                            }
    //                                            //- character considers this one as a Friend: Weight x0.3
    //                                            if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
    //                                                weight *= 0.3f;
    //                                            }
    //                                        }
    //                                    }
    //                                    //REMOVED: Beast
    //                                    //else {
    //                                    //    /*
    //                                    //- if beast, from valid characters, choose based on these weights
    //                                    //   - character is non-beast: +10 Weight
    //                                    //   - character is a beast: +50 Weight
    //                                    //   - character is a relative: Weight x0
    //                                    //   - character is a different race: Weight x2
    //                                    // */
    //                                    //    if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
    //                                    //        weight += 10;
    //                                    //    } else {
    //                                    //        weight += 50;
    //                                    //    }
    //                                    //    if (existingRels != null) {
    //                                    //        if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
    //                                    //            weight *= 0;
    //                                    //        }
    //                                    //    }
    //                                    //    if (currCharacter.race != otherCharacter.race) {
    //                                    //        weight *= 2;
    //                                    //    }
    //                                    //}
    //                                }
    //                                break;
    //                            case RELATIONSHIP_TRAIT.FRIEND:
    //                                if (GetValidator(currCharacter).CanHaveRelationship(currCharacter, otherCharacter, currRel) && GetValidator(otherCharacter).CanHaveRelationship(otherCharacter, currCharacter, currRel)) {
    //                                    if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
    //                                        //- if non beast, from valid characters, choose based on these weights:
    //                                        if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
    //                                            //- character is non-beast: +50 Weight
    //                                            weight += 50;
    //                                        } else {
    //                                            //- character is a beast: +5 Weight
    //                                            weight += 5;
    //                                        }
    //                                        if (currCharacter.faction.id == otherCharacter.faction.id) {
    //                                            // - character is from same faction: Weight x6
    //                                            weight *= 6;
    //                                        }
    //                                        if (currCharacter.race == otherCharacter.race) {
    //                                            //- character is from same race: Weight x2
    //                                            weight *= 2;
    //                                        }
    //                                        if (existingRelsOfCurrentCharacter != null) {
    //                                            if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.RELATIVE)
    //                                                || existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.LOVER)
    //                                                || existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
    //                                                //- character is a relative: Weight x0
    //                                                //- character is a lover: Weight x0
    //                                                //- this one considers the character an enemy: Weight x0
    //                                                weight *= 0;
    //                                            }
    //                                        }
    //                                        if (existingRelsOfOtherCharacter != null) {
    //                                            //- character considers this one as an Enemy: Weight x0.3
    //                                            if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
    //                                                weight *= 0.3f;
    //                                            }
    //                                            //- character considers this one as a Friend: Weight x6
    //                                            if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
    //                                                weight *= 6;
    //                                            }
    //                                        }
    //                                        if (currCharacter.faction != otherCharacter.faction) {
    //                                            weight *= 0; //disabled different faction positive relationships
    //                                        }
    //                                    }
    //                                }
    //                                //REMOVED : Beast
    //                                //    else {
    //                                //        /*
    //                                //    - if beast, from valid characters, choose based on these weights
    //                                //       - character is beast: +50 Weight
    //                                //       - character is non-beast: +5 Weight
    //                                //       - character is from same faction: Weight x4
    //                                //       - character is from same race: Weight x2
    //                                //       - character is a relative: Weight x0
    //                                //       - character is a lover: Weight x0
    //                                //       - character is a master/servant: Weight x0
    //                                //       - character is an enemy: Weight x0
    //                                //     */
    //                                //        if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
    //                                //            weight += 50;
    //                                //        } else {
    //                                //            weight += 5;
    //                                //        }

    //                                //        if (currCharacter.faction.id == otherCharacter.faction.id) {
    //                                //            weight *= 4;
    //                                //        }
    //                                //        if (currCharacter.race == otherCharacter.race) {
    //                                //            weight *= 2;
    //                                //        }

    //                                //        if (existingRels != null) {
    //                                //            if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)
    //                                //                || existingRels.Contains(RELATIONSHIP_TRAIT.LOVER)
    //                                //                || existingRels.Contains(RELATIONSHIP_TRAIT.MASTER)
    //                                //                || existingRels.Contains(RELATIONSHIP_TRAIT.SERVANT)
    //                                //                || existingRels.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
    //                                //                weight *= 0;
    //                                //            }
    //                                //        }
    //                                //    }
    //                                //}
    //                                break;
    //                                //case RELATIONSHIP_TRAIT.PARAMOUR:
    //                                //    if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no paramours
    //                                //    if (currCharacter.CanHaveRelationshipWith(currRel, otherCharacter) && otherCharacter.CanHaveRelationshipWith(currRel, currCharacter)) {
    //                                //        //- if non beast, from valid characters, choose based on these weights:
    //                                //        if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
    //                                //            //- character is non-beast: +500 Weight
    //                                //            weight += 500;
    //                                //        }
    //                                //        if (!IsSexuallyCompatible(currCharacter, otherCharacter)) {
    //                                //            //- character is sexually incompatible: Weight x0.1
    //                                //            weight *= 0.1f;
    //                                //        }
    //                                //        if (currCharacter.race == otherCharacter.race) {
    //                                //            // - character is from same race: Weight x5
    //                                //            weight *= 5;
    //                                //        }
    //                                //        if (currCharacter.faction.id == otherCharacter.faction.id) {
    //                                //            //- character is from same faction: Weight x3
    //                                //            weight *= 3;
    //                                //        }

    //                                //        if (existingRelsOfCurrentCharacter != null) {
    //                                //            if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
    //                                //                //- character is a relative: Weight x0.1
    //                                //                weight *= 0.1f;
    //                                //            }
    //                                //            if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.LOVER)
    //                                //                || existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
    //                                //                //- character is a lover: Weight x0
    //                                //                //- character is an enemy: Weight x0
    //                                //                weight *= 0;
    //                                //            }
    //                                //            //REMOVED - character is a master/servant: Weight x0
    //                                //        }
    //                                //        if (currCharacter.faction != otherCharacter.faction) {
    //                                //            weight *= 0; //disabled different faction positive relationships
    //                                //        }
    //                                //    }
    //                                //    break;
    //                        }
    //                        if (weight > 0f) {
    //                            relWeights.AddElement(otherCharacter, weight);
    //                        }
    //                    }
    //                }
    //                if (relWeights.GetTotalOfWeights() > 0) {
    //                    summary += "\n" + relWeights.GetWeightsSummary("Weights are: ");
    //                } else {
    //                    summary += "\nThere are no valid characters to have a relationship with.";
    //                }


    //                for (int j = 0; j < relsToCreate; j++) {
    //                    if (relWeights.GetTotalOfWeights() > 0) {
    //                        Character chosenCharacter = relWeights.PickRandomElementGivenWeights();
    //                        CreateNewRelationshipBetween(currCharacter, chosenCharacter, currRel);
    //                        totalCreatedRels++;
    //                        summary += "\nCreated new relationship " + currRel.ToString() + " between " + currCharacter.name + " and " + chosenCharacter.name + ". Total relationships created for " + currCharacter.name + " are " + totalCreatedRels.ToString();
    //                        relWeights.RemoveElement(chosenCharacter);
    //                    } else {
    //                        break;
    //                    }
    //                    if (totalCreatedRels >= maxInitialRels) {
    //                        //summary += "\nMax Initial Relationships reached, stopping relationship generation for " + currCharacter.name;
    //                        break; //stop generating more relationships for this character
    //                    }
    //                }
    //            }
    //        }
    //        Debug.Log(summary);
    //    }
    //}
    public void GenerateRelationships(List<Character> characters) {
        int maxInitialRels = 4;
        RELATIONSHIP_TRAIT[] relsInOrder = new RELATIONSHIP_TRAIT[] { RELATIONSHIP_TRAIT.RELATIVE, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.ENEMY, RELATIONSHIP_TRAIT.FRIEND };

        // Loop through all characters in the world
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            if (currCharacter.isFactionless) {
                continue; //skip factionless characters
            }
            int currentRelCount = currCharacter.relationshipContainer.relationships.Count;
            if (currentRelCount >= maxInitialRels) {
                continue; //skip
            }
            int totalCreatedRels = currentRelCount;
            string summary = currCharacter.name + "(" + currCharacter.sexuality.ToString() + ") relationship generation summary:";

            //  Loop through all relationship types
            for (int k = 0; k < relsInOrder.Length; k++) {
                RELATIONSHIP_TRAIT currRel = relsInOrder[k];
                if (totalCreatedRels >= maxInitialRels) {
                    summary += "\nMax Initial Relationships reached, stopping relationship generation for " + currCharacter.name;
                    break; //stop generating more relationships for this character
                }
                int relsToCreate = 0;
                int chance = Random.Range(0, 100);

                // Compute the number of relations to create per relationship type
                switch (currRel) {
                    case RELATIONSHIP_TRAIT.RELATIVE:
                        if (currCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no relatives
                        else {
                            //- a non-beast character may have either zero (75%), one (20%) or two (5%) relatives from characters of the same race
                            if (chance < 75) relsToCreate = 0;
                            else if (chance >= 75 && chance < 95) relsToCreate = 1;
                            else relsToCreate = 2;
                        }
                        break;
                    case RELATIONSHIP_TRAIT.LOVER:
                        //- a character has a 20% chance to have a lover
                        if (chance < 20) relsToCreate = 1;
                        //relsToCreate = 1;
                        break;
                    case RELATIONSHIP_TRAIT.ENEMY:
                        //- a character may have either zero (75%), one (20%) or two (5%) enemies
                        if (chance < 75) relsToCreate = 0;
                        else if (chance >= 75 && chance < 95) relsToCreate = 1;
                        else relsToCreate = 2;
                        //relsToCreate = 2;
                        break;
                    case RELATIONSHIP_TRAIT.FRIEND:
                        //- a character may have either zero (65%), one (25%) or two (10%) friends
                        if (chance < 65) relsToCreate = 0;
                        else if (chance >= 65 && chance < 90) relsToCreate = 1;
                        else relsToCreate = 2;
                        break;
                }
                summary += "\n===========Creating " + relsToCreate + " " + currRel.ToString() + " Relationships...==========";


                if (relsToCreate > 0) {
                    WeightedFloatDictionary<Character> relWeights = new WeightedFloatDictionary<Character>();
                    // Loop through all characters that are in the same faction as the current character
                    for (int l = 0; l < currCharacter.faction.characters.Count; l++) {
                        Character otherCharacter = currCharacter.faction.characters[l];
                        if (currCharacter.id != otherCharacter.id) { //&& currCharacter.faction == otherCharacter.faction
                            List<RELATIONSHIP_TRAIT> existingRelsOfCurrentCharacter = currCharacter.relationshipContainer.GetRelationshipDataWith(otherCharacter.currentAlterEgo)?.relationships ?? null;
                            List<RELATIONSHIP_TRAIT> existingRelsOfOtherCharacter = otherCharacter.relationshipContainer.GetRelationshipDataWith(currCharacter.currentAlterEgo)?.relationships ?? null;
                            //if the current character already has a relationship of the same type with the other character, skip
                            if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(currRel)) {
                                continue; //skip
                            }
                            float weight = 0;

                            // Compute the weight that determines how likely this character will have the current relationship type with current character
                            switch (currRel) {
                                case RELATIONSHIP_TRAIT.RELATIVE:
                                    if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no relatives
                                    else {
                                        if (otherCharacter.currentRegion == currCharacter.currentRegion) {
                                            // character is in same location: +50 Weight
                                            weight += 50;
                                        } else {
                                            //character is in different location: +10 Weight
                                            weight += 10;
                                        }

                                        if (currCharacter.race != otherCharacter.race) weight *= 0; //character is a different race: Weight x0
                                        if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
                                            //Disabled possiblity that relatives can be friends
                                            weight *= 0;
                                        }
                                        if (currCharacter.faction != otherCharacter.faction) {
                                            weight *= 0; //disabled different faction positive relationships
                                        }
                                    }
                                    break;
                                case RELATIONSHIP_TRAIT.LOVER:
                                    if (GetValidator(currCharacter).CanHaveRelationship(currCharacter, otherCharacter, currRel) && GetValidator(otherCharacter).CanHaveRelationship(otherCharacter, currCharacter, currRel)) {
                                        if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                            //- if non beast, from valid characters, choose based on these weights
                                            if (otherCharacter.currentRegion == currCharacter.currentRegion) {
                                                //- character is in same location: +500 Weight
                                                weight += 500;
                                            } else {
                                                //- character is in different location: +5 Weight
                                                weight += 5;
                                            }
                                            if (currCharacter.race == otherCharacter.race) {
                                                //- character is the same race: Weight x5
                                                weight *= 5;
                                            }
                                            if (!IsSexuallyCompatible(currCharacter, otherCharacter)) {
                                                //- character is sexually incompatible: Weight x0.1
                                                weight *= 0.05f;
                                            }
                                            if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
                                                //- character is a beast: Weight x0
                                                weight *= 0;
                                            }
                                            if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
                                                //- character is a relative: Weight x0.1    
                                                weight *= 0.1f;
                                            }
                                            if (currCharacter.faction != otherCharacter.faction) {
                                                weight *= 0; //disabled different faction positive relationships
                                            }
                                        } else {
                                            //- if beast, from valid characters, choose based on these weights
                                            if (otherCharacter.currentRegion == currCharacter.currentRegion) {
                                                //- character is in same location: +50 Weight
                                                weight += 50;
                                            } else {
                                                // - character is in different location: +5 Weight
                                                weight += 5;
                                            }
                                            if (currCharacter.race != otherCharacter.race) {
                                                //- character is a different race: Weight x0
                                                weight *= 0;
                                            }
                                            if (currCharacter.gender != otherCharacter.gender) {
                                                //- character is the opposite gender: Weight x6
                                                weight *= 6;
                                            }
                                        }
                                    }
                                    break;
                                case RELATIONSHIP_TRAIT.ENEMY:
                                    if (GetValidator(currCharacter).CanHaveRelationship(currCharacter, otherCharacter, currRel) && GetValidator(otherCharacter).CanHaveRelationship(otherCharacter, currCharacter, currRel)) {
                                        if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                            //- if non beast, from valid characters, choose based on these weights
                                            if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                                // - character is non-beast: +50 Weight
                                                weight += 50;
                                            } else {
                                                //- character is a beast: +5 Weight
                                                weight += 5;
                                            }
                                            if (currCharacter.faction.id == otherCharacter.faction.id) {
                                                //- character is from same faction: Weight x6
                                                weight *= 6;
                                            }
                                            if (currCharacter.race != otherCharacter.race) {
                                                //- character is a different race: Weight x2
                                                weight *= 2;
                                            }

                                            if (existingRelsOfCurrentCharacter != null) {
                                                if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.LOVER)) {
                                                    //- character is a lover: Weight x0
                                                    weight *= 0;
                                                }
                                            }
                                            if (existingRelsOfOtherCharacter != null) {
                                                //- character considers this one as an Enemy: Weight x6
                                                if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                                    weight *= 6;
                                                }
                                                //- character considers this one as a Friend: Weight x0.3
                                                if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
                                                    weight *= 0.3f;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case RELATIONSHIP_TRAIT.FRIEND:
                                    if (GetValidator(currCharacter).CanHaveRelationship(currCharacter, otherCharacter, currRel) && GetValidator(otherCharacter).CanHaveRelationship(otherCharacter, currCharacter, currRel)) {
                                        if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                            //- if non beast, from valid characters, choose based on these weights:
                                            if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                                //- character is non-beast: +50 Weight
                                                weight += 50;
                                            } else {
                                                //- character is a beast: +5 Weight
                                                weight += 5;
                                            }
                                            if (currCharacter.faction.id == otherCharacter.faction.id) {
                                                // - character is from same faction: Weight x6
                                                weight *= 6;
                                            }
                                            if (currCharacter.race == otherCharacter.race) {
                                                //- character is from same race: Weight x2
                                                weight *= 2;
                                            }
                                            if (existingRelsOfCurrentCharacter != null) {
                                                if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.RELATIVE)
                                                    || existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.LOVER)
                                                    || existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                                    //- character is a relative: Weight x0
                                                    //- character is a lover: Weight x0
                                                    //- this one considers the character an enemy: Weight x0
                                                    weight *= 0;
                                                }
                                            }
                                            if (existingRelsOfOtherCharacter != null) {
                                                //- character considers this one as an Enemy: Weight x0.3
                                                if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                                    weight *= 0.3f;
                                                }
                                                //- character considers this one as a Friend: Weight x6
                                                if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
                                                    weight *= 6;
                                                }
                                            }
                                            if (currCharacter.faction != otherCharacter.faction) {
                                                weight *= 0; //disabled different faction positive relationships
                                            }
                                        }
                                    }
                                    break;
                            }
                            if (weight > 0f) {
                                relWeights.AddElement(otherCharacter, weight);
                            }
                        }
                    }
                    if (relWeights.GetTotalOfWeights() > 0) {
                        summary += "\n" + relWeights.GetWeightsSummary("Weights are: ");
                    } else {
                        summary += "\nThere are no valid characters to have a relationship with.";
                    }


                    for (int j = 0; j < relsToCreate; j++) {
                        if (relWeights.GetTotalOfWeights() > 0) {
                            Character chosenCharacter = relWeights.PickRandomElementGivenWeights();
                            CreateNewRelationshipBetween(currCharacter, chosenCharacter, currRel);
                            totalCreatedRels++;
                            summary += "\nCreated new relationship " + currRel.ToString() + " between " + currCharacter.name + " and " + chosenCharacter.name + ". Total relationships created for " + currCharacter.name + " are " + totalCreatedRels.ToString();
                            relWeights.RemoveElement(chosenCharacter);
                        } else {
                            break;
                        }
                        if (totalCreatedRels >= maxInitialRels) {
                            //summary += "\nMax Initial Relationships reached, stopping relationship generation for " + currCharacter.name;
                            break; //stop generating more relationships for this character
                        }
                    }
                }
            }
            Debug.Log(summary);
        }
    }
    public bool IsSexuallyCompatible(Character character1, Character character2) {
        bool sexuallyCompatible = IsSexuallyCompatibleOneSided(character1, character2);
        if (!sexuallyCompatible) {
            return false; //if they are already sexually incompatible in one side, return false
        }
        sexuallyCompatible = IsSexuallyCompatibleOneSided(character2, character1);
        return sexuallyCompatible;
    }
    /// <summary>
    /// Is a character sexually compatible with another.
    /// </summary>
    /// <param name="character1">The character whose sexuality will be taken into account.</param>
    /// <param name="character2">The character that character 1 is checking.</param>
    /// <returns></returns>
    public bool IsSexuallyCompatibleOneSided(Character character1, Character character2) {
        switch (character1.sexuality) {
            case SEXUALITY.STRAIGHT:
                return character1.gender != character2.gender;
            case SEXUALITY.BISEXUAL:
                return true; //because bisexuals are attracted to both genders.
            case SEXUALITY.GAY:
                return character1.gender == character2.gender;
            default:
                return false;
        }
    }

    #region Adding
    public IRelationshipData CreateNewOneWayRelationship(Relatable rel1, Relatable rel2, RELATIONSHIP_TRAIT rel) {
        if (!rel1.relationshipContainer.HasRelationshipWith(rel2, rel)) {
            //TODO: Move this somewhere else
            //if (rel == RELATIONSHIP_TRAIT.ENEMY && currCharacter.traitContainer.GetNormalTrait<Trait>("Diplomatic") != null) {
            //    return currCharacter.relationshipContainer.GetRelationshipDataWith(alterEgo.owner);
            //}
            //if (rel == RELATIONSHIP_TRAIT.FRIEND && currCharacter.traitContainer.GetNormalTrait<Trait>("Serial Killer") != null) {
            //    return currCharacter.relationshipContainer.GetRelationshipDataWith(alterEgo.owner);
            //}
            rel1.relationshipContainer.AddRelationship(rel2, rel);
            rel1.relationshipProcessor?.OnRelationshipAdded(rel1, rel2, rel);
            Messenger.Broadcast(Signals.RELATIONSHIP_ADDED, rel1, rel2);
        }
        return rel1.relationshipContainer.GetRelationshipDataWith(rel2);
    }
    public IRelationshipData CreateNewRelationshipBetween(Relatable rel1, Relatable rel2, RELATIONSHIP_TRAIT rel) {
        RELATIONSHIP_TRAIT pair = GetPairedRelationship(rel);
        //TODO: Move this somewhere else
        //!(rel == RELATIONSHIP_TRAIT.ENEMY && currCharacter.traitContainer.GetNormalTrait<Trait>("Diplomatic") != null)
        //&& !(rel == RELATIONSHIP_TRAIT.FRIEND && currCharacter.traitContainer.GetNormalTrait<Trait>("Serial Killer") != null)
        if (CanHaveRelationship(rel1, rel2, rel)) {
            rel1.relationshipContainer.AddRelationship(rel2, rel);
            rel1.relationshipProcessor?.OnRelationshipAdded(rel1, rel2, rel);
        }
        //TODO:Move this somewhere else
        //!(rel == RELATIONSHIP_TRAIT.ENEMY && targetCharacter.traitContainer.GetNormalTrait<Trait>("Diplomatic") != null)
        //&& !(rel == RELATIONSHIP_TRAIT.FRIEND && targetCharacter.traitContainer.GetNormalTrait<Trait>("Serial Killer") != null)
        if (CanHaveRelationship(rel2, rel1, rel)) {
            rel2.relationshipContainer.AddRelationship(rel1, pair);
            rel2.relationshipProcessor?.OnRelationshipAdded(rel2, rel1, pair);
        }
        Messenger.Broadcast(Signals.RELATIONSHIP_ADDED, rel1, rel2);
        return rel1.relationshipContainer.GetRelationshipDataWith(rel2);
    }
    #endregion

    #region Removing
    public void RemoveOneWayRelationship(Relatable rel1, Relatable rel2, RELATIONSHIP_TRAIT rel) {
        rel1.relationshipContainer.RemoveRelationship(rel2, rel);
        rel1.relationshipProcessor?.OnRelationshipRemoved(rel1, rel2, rel);
        Messenger.Broadcast(Signals.RELATIONSHIP_REMOVED, rel1, rel, rel2);
    }
    public void RemoveRelationshipBetween(Relatable rel1, Relatable rel2, RELATIONSHIP_TRAIT rel) {
        if (!rel1.relationshipContainer.relationships.ContainsKey(rel2)
            || !rel2.relationshipContainer.relationships.ContainsKey(rel1)) {
            return;
        }
        RELATIONSHIP_TRAIT pair = GetPairedRelationship(rel);
        if (rel1.relationshipContainer.relationships[rel2].HasRelationship(rel)
            && rel2.relationshipContainer.relationships[rel1].HasRelationship(pair)) {

            rel1.relationshipContainer.RemoveRelationship(rel2, rel);
            rel1.relationshipProcessor?.OnRelationshipRemoved(rel1, rel2, rel);
            rel2.relationshipContainer.RemoveRelationship(rel1, pair);
            rel2.relationshipProcessor?.OnRelationshipRemoved(rel2, rel1, pair);
            Messenger.Broadcast(Signals.RELATIONSHIP_REMOVED, rel1, rel, rel2);
        }
    }
    #endregion

    #region Relationship Improvement
    public bool RelationshipImprovement(Character actor, Character target, GoapAction cause = null) {
        if (actor.race == RACE.DEMON || target.race == RACE.DEMON || actor is Summon || target is Summon) {
            return false; //do not let demons and summons have relationships
        }
        if (actor.returnedToLife || target.returnedToLife) {
            return false; //do not let zombies or skeletons develop other relationships
        }
        string summary = "Relationship improvement between " + actor.name + " and " + target.name;
        bool hasImproved = false;
        Log log = null;
        if (target.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //If Actor and Target are Enemies, 25% chance to remove Enemy relationship. If so, Target now considers Actor a Friend.
            summary += "\n" + target.name + " considers " + actor.name + " an enemy. Rolling for chance to consider as a friend...";
            int roll = UnityEngine.Random.Range(0, 100);
            summary += "\nRoll is " + roll.ToString();
            if (roll < 25) {
                if (target.traitContainer.GetNormalTrait<Trait>("Serial Killer") == null) {
                    log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "enemy_now_friend");
                    summary += target.name + " now considers " + actor.name + " an enemy.";
                    RemoveOneWayRelationship(target, actor, RELATIONSHIP_TRAIT.ENEMY);
                    CreateNewOneWayRelationship(target, actor, RELATIONSHIP_TRAIT.FRIEND);
                    hasImproved = true;
                }
            }
        }
        //If character is already a Friend, will not change actual relationship but will consider it improved
        else if (target.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
            hasImproved = true;
        } else if (!target.relationshipContainer.HasRelationshipWith(actor)) {
            if (target.traitContainer.GetNormalTrait<Trait>("Serial Killer") == null) {
                log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "now_friend");
                summary += "\n" + target.name + " has no relationship with " + actor.name + ". " + target.name + " now considers " + actor.name + " a friend.";
                //If Target has no relationship with Actor, Target now considers Actor a Friend.
                CreateNewOneWayRelationship(target, actor, RELATIONSHIP_TRAIT.FRIEND);
                hasImproved = true;
            }
        }
        Debug.Log(summary);
        if (log != null) {
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            PlayerManager.Instance.player.ShowNotificationFrom(log, target, actor);
        }
        return hasImproved;
    }
    #endregion

    #region Relationship Degradation
    /// <summary>
    /// Unified way of degrading a relationship of a character with a target character.
    /// </summary>
    /// <param name="actor">The character that did something to degrade the relationship.</param>
    /// <param name="target">The character that will change their relationship with the actor.</param>
    public bool RelationshipDegradation(Character actor, Character target, ActualGoapNode cause = null) {
        return RelationshipDegradation(actor.currentAlterEgo, target, cause);
    }
    public bool RelationshipDegradation(AlterEgoData actorAlterEgo, Character target, ActualGoapNode cause = null) {
        if (actorAlterEgo.owner.race == RACE.DEMON || target.race == RACE.DEMON || actorAlterEgo.owner is Summon || target is Summon) {
            return false; //do not let demons and summons have relationships
        }
        if (actorAlterEgo.owner.returnedToLife || target.returnedToLife) {
            return false; //do not let zombies or skeletons develop other relationships
        }

        bool hasDegraded = false;
        if (actorAlterEgo.owner.isFactionless || target.isFactionless) {
            Debug.LogWarning("Relationship degredation was called and one or both of those characters is factionless");
            return hasDegraded;
        }
        if (actorAlterEgo.owner == target) {
            Debug.LogWarning("Relationship degredation was called and provided same characters " + target.name);
            return hasDegraded;
        }
        if (target.traitContainer.GetNormalTrait<Trait>("Diplomatic") != null) {
            Debug.LogWarning("Relationship degredation was called but " + target.name + " is Diplomatic");
            hasDegraded = true;
            return hasDegraded;
        }
        string summary = "Relationship degradation between " + actorAlterEgo.owner.name + " and " + target.name;
        //TODO:
        //if (cause != null && cause.IsFromApprehendJob()) {
        //    //If this has been triggered by an Action's End Result that is part of an Apprehend Job, skip processing.
        //    summary += "Relationship degradation was caused by an action in an apprehend job. Skipping degredation...";
        //    Debug.Log(summary);
        //    return hasDegraded;
        //}
        //If Actor and Target are Lovers, 25% chance to create a Break Up Job with the Lover.
        //if (target.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
        //    summary += "\n" + actorAlterEgo.owner.name + " and " + target.name + " are  lovers. Rolling for chance to create break up job...";
        //    int roll = UnityEngine.Random.Range(0, 100);
        //    summary += "\nRoll is " + roll.ToString();
        //    if (roll < 25) {
        //        summary += "\n" + target.name + " created break up job targetting " + actorAlterEgo.owner.name;
        //        target.CreateBreakupJob(actorAlterEgo.owner);

        //        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "break_up");
        //        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //        PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
        //        hasDegraded = true;
        //    }
        //}
        ////If Actor and Target are Paramours, 25% chance to create a Break Up Job with the Paramour.
        //else if (target.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
        //    summary += "\n" + actorAlterEgo.owner.name + " and " + target.name + " are  paramours. Rolling for chance to create break up job...";
        //    int roll = UnityEngine.Random.Range(0, 100);
        //    summary += "\nRoll is " + roll.ToString();
        //    if (roll < 25) {
        //        summary += "\n" + target.name + " created break up job targetting " + actorAlterEgo.owner.name;
        //        target.CreateBreakupJob(actorAlterEgo.owner);

        //        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "break_up");
        //        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //        PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
        //        hasDegraded = true;
        //    }
        //}

        //If Target considers Actor a Friend, remove that. If Target is in Bad or Dark Mood, Target now considers Actor an Enemy. Otherwise, they are just no longer friends.
        if (target.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
            summary += "\n" + target.name + " considers " + actorAlterEgo.name + " as a friend. Removing friend and replacing with enemy";
            RemoveOneWayRelationship(target, actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND);
            if (target.currentMoodType == CHARACTER_MOOD.BAD || target.currentMoodType == CHARACTER_MOOD.DARK) {
                CreateNewOneWayRelationship(target, actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY);
                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "friend_now_enemy");
                log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
                hasDegraded = true;
            } else {
                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "no_longer_friend");
                log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
                hasDegraded = true;
            }
        }
        //If character is already an Enemy, will not change actual relationship but will consider it degraded
        else if (target.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            hasDegraded = true;
        }
        //If Target is only Relative of Actor(no other relationship) or has no relationship with Actor, Target now considers Actor an Enemy.
        else if (!target.relationshipContainer.HasRelationshipWith(actorAlterEgo) || (target.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.RELATIVE) && target.relationshipContainer.GetRelationshipDataWith(actorAlterEgo).relationships.Count == 1)) {
            summary += "\n" + target.name + " and " + actorAlterEgo.owner.name + " has no relationship or only has relative relationship. " + target.name + " now considers " + actorAlterEgo.owner.name + " an enemy.";
            CreateNewOneWayRelationship(target, actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY);

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "now_enemy");
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
            hasDegraded = true;
        }


        Debug.Log(summary);
        return hasDegraded;
    }
    #endregion

    #region Processors
    public IRelationshipProcessor GetProcessor(Relatable relatable) {
        if (relatable is AlterEgoData) {
            return CharacterRelationshipProcessor.Instance;
        }
        return null;
    }
    #endregion
}
