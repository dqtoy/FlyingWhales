using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class DefaultAtHome : CharacterBehaviourComponent {
    public DefaultAtHome() {
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.INSIDE_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        if (character.currentStructure == character.homeStructure) {
            log += "\n-" + character.name + " is in home structure and previous action is not returned home";
            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);

            log += "\n-If it is Early Night, 35% chance to go to the current Inn and then set it as the Base Structure for 2.5 hours";
            if (currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                int chance = UnityEngine.Random.Range(0, 100);
                log += "\n  -RNG roll: " + chance;
                if (chance < 35) {
                    if (character.traitContainer.GetNormalTrait<Trait>("Agoraphobic") != null) {
                        log += "\n  -Character is agoraphobic, nott going to inn";
                    } else {
                        //StartGOAP(INTERACTION_TYPE.DRINK, null, GOAP_CATEGORY.IDLE);
                        LocationStructure structure = character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.INN);
                        if (structure != null) {
                            log += "\n  -Early Night: " + character.name + " will go to Inn and set Base Structure for 2.5 hours";
                            character.PlanIdle(INTERACTION_TYPE.VISIT, character, new object[] { structure });
                            return true;
                        } else {
                            log += "\n  -No Inn Structure in the settlement";
                        }
                    }
                }
            } else {
                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
            }
            log += "\n-Otherwise, if it is Lunch Time or Afternoon, 25% chance to nap if there is still an unoccupied Bed in the house";
            if (currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                int chance = UnityEngine.Random.Range(0, 100);
                log += "\n  -RNG roll: " + chance;
                if (chance < 25) {
                    TileObject bed = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                    if (bed != null) {
                        if (character.traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                            log += "\n  -Character is vampiric, cannot do nap action";
                        } else {
                            log += "\n  -Afternoon: " + character.name + " will do action Nap on " + bed.ToString();
                            character.PlanIdle(INTERACTION_TYPE.NAP, bed);
                            return true;
                        }
                    } else {
                        log += "\n  -No unoccupied bed in the current structure";
                    }
                }
            } else {
                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
            }
            log += "\n-Otherwise, if it is Morning or Afternoon or Early Night, and the character has a positive relationship with someone currently Paralyzed or Catatonic, 30% chance to Check Out one at random";
            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                int chance = UnityEngine.Random.Range(0, 100);
                log += "\n  -RNG roll: " + chance;
                if (chance < 30) {
                    Character chosenCharacter = character.GetParalyzedOrCatatonicCharacterToCheckOut();
                    if (chosenCharacter != null) {
                        log += "\n  -Will Check Out character " + chosenCharacter.name;
                        character.PlanIdle(new GoapEffect(GOAP_EFFECT_CONDITION.IN_VISION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), chosenCharacter);
                        return true;
                    } else {
                        log += "\n  -No available character to check out ";
                    }
                }
            } else {
                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
            }
            log += "\n-Otherwise, if it is Morning or Lunch Time or Afternoon or Early Night, 25% chance to enter Stroll Outside Mode for 1 hour";
            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                int chance = UnityEngine.Random.Range(0, 100);
                log += "\n  -RNG roll: " + chance;
                if (chance < 25) {
                    log += "\n  -Morning, Afternoon, or Early Night: " + character.name + " will enter Stroll Outside Mode";
                    character.PlanIdleStrollOutside(); //character.currentStructure
                    return true;
                }
            } else {
                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
            }
            log += "\n-Otherwise, if it is Morning or Afternoon, 25% chance to someone with a positive relationship in current location and then set it as the Base Structure for 2.5 hours";
            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
                int chance = UnityEngine.Random.Range(0, 100);
                log += "\n  -RNG roll: " + chance;
                if (chance < 25) {
                    List<Character> positiveRelatables = character.opinionComponent.GetCharactersWithPositiveOpinion();
                    if (positiveRelatables.Count > 0) {
                        LocationStructure targetStructure = null;
                        while (positiveRelatables.Count > 0 && targetStructure == null) {
                            int index = UnityEngine.Random.Range(0, positiveRelatables.Count);
                            Character chosenRelatable = positiveRelatables[index];
                            targetStructure = chosenRelatable.currentAlterEgo.owner.homeStructure.GetLocationStructure();
                            if (targetStructure == null) {
                                positiveRelatables.RemoveAt(index);
                            } else if (targetStructure == character.homeStructure) {
                                targetStructure = null;
                                positiveRelatables.RemoveAt(index);
                            }
                        }
                        if (targetStructure != null) {
                            log += "\n  -Morning or Afternoon: " + character.name + " will go to dwelling of character with positive relationship and set Base Structure for 2.5 hours";
                            character.PlanIdle(INTERACTION_TYPE.VISIT, character, new object[] { targetStructure });
                            return true;
                        } else {
                            log += "\n  -No positive relationship with home structure";
                        }
                    } else {
                        log += "\n  -No character with positive relationship";
                    }
                }
            } else {
                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
            }
            log += "\n-Otherwise, sit if there is still an unoccupied Table or Desk";
            TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
            if (deskOrTable != null) {
                log += "\n  -" + character.name + " will do action Sit on " + deskOrTable.ToString();
                character.PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
                return true;
            } else {
                log += "\n  -No unoccupied Table or Desk";
            }

            log += "\n-Otherwise, stand idle";
            log += "\n  -" + character.name + " will do action Stand";
            character.PlanIdle(INTERACTION_TYPE.STAND, character);
            //PlanIdleStroll(currentStructure);
            return true;
        }
        return false;
    }
}
