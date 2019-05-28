using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatCharacter : GoapAction {
    public string chatResult { get; private set; }

    public ChatCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CHAT_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Social_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        onlyShowNotifOfDescriptionLog = true;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Chat Success");
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    private void PreChatSuccess() {
        Character targetCharacter = poiTarget as Character;
        actor.SetIsChatting(true);
        targetCharacter.SetIsChatting(true);
        actor.marker.UpdateActionIcon();
        targetCharacter.marker.UpdateActionIcon();
        CHARACTER_MOOD thisCharacterMood = actor.currentMoodType;
        CHARACTER_MOOD targetCharacterMood = targetCharacter.currentMoodType;

        WeightedFloatDictionary<string> weights = new WeightedFloatDictionary<string>();
        weights.AddElement("normal", 400);

        CharacterRelationshipData relData = actor.GetCharacterRelationshipData(targetCharacter);
        RELATIONSHIP_EFFECT relationshipEffectWithTarget = actor.GetRelationshipEffectWith(targetCharacter);
        //**if no relationship yet, may become friends**
        if (relData == null) {
            int weight = 0;
            if (thisCharacterMood == CHARACTER_MOOD.DARK) {
                weight += -50;
            } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                weight += -20;
            } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                weight += 20;
            } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                weight += 50;
            }
            if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                weight += -50;
            } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                weight += -20;
            } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                weight += 20;
            } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                weight += 50;
            }
            if (weight > 0) {
                weights.AddElement("no rel", weight);
            }
        } else {
            //**if no relationship other than relative, may become enemies**
            List<RelationshipTrait> relTraits = actor.GetAllRelationshipTraitWith(targetCharacter);
            if (relTraits.Count == 1 && relTraits[0] is Relative) {
                int weight = 0;
                if (thisCharacterMood == CHARACTER_MOOD.DARK) {
                    weight += 50;
                } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                    weight += 20;
                } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                    weight += -20;
                } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                    weight += -50;
                }
                if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                    weight += 50;
                } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                    weight += 20;
                } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                    weight += -20;
                } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                    weight += -50;
                }
                if (weight > 0) {
                    weights.AddElement("no rel relative", weight);
                }
            }

            //**if already has a positive relationship, knowledge may be transferred**
            if (relationshipEffectWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                weights.AddElement("knowledge transfer", 200);
            }

            //**if already has a negative relationship, argument may occur**
            if (relationshipEffectWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                weights.AddElement("argument", 500);

                //**if already has a negative relationship, relationship may be resolved**
                int weight = 0;
                if (thisCharacterMood == CHARACTER_MOOD.DARK) {
                    weight += -50;
                } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                    weight += -20;
                } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                    weight += 20;
                } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                    weight += 50;
                }
                if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                    weight += -50;
                } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                    weight += -20;
                } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                    weight += 20;
                } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                    weight += 50;
                }
                if (weight > 0) {
                    weights.AddElement("resolve", weight);
                }
            }
        }

        //flirtation
        float flirtationWeight = actor.GetFlirtationWeightWith(targetCharacter, relData, thisCharacterMood, targetCharacterMood);
        if (flirtationWeight > 0f) {
            weights.AddElement("flirt", flirtationWeight);
        }

        //become lovers weight
        float becomeLoversWeight = actor.GetBecomeLoversWeightWith(targetCharacter, relData, thisCharacterMood, targetCharacterMood);
        if (becomeLoversWeight > 0f) {
            weights.AddElement("become lovers", becomeLoversWeight);
        }

        //become paramours
        float becomeParamoursWeight = actor.GetBecomeParamoursWeightWith(targetCharacter, relData, thisCharacterMood, targetCharacterMood);
        if (becomeParamoursWeight > 0f) {
            weights.AddElement("become paramours", becomeParamoursWeight);
        }

        string result = weights.PickRandomElementGivenWeights();
        if (result == "no rel") {
            //may become friends
            CharacterManager.Instance.CreateNewRelationshipBetween(actor, targetCharacter, RELATIONSHIP_TRAIT.FRIEND);
        } else if (result == "no rel relative") {
            //may become enemies
            CharacterManager.Instance.CreateNewRelationshipBetween(actor, targetCharacter, RELATIONSHIP_TRAIT.ENEMY);
        } else if (result == "knowledge transfer") {
            //TODO: mechanics to be added later
        } else if (result == "resolve") {
            //relationship may be resolved
            List<RelationshipTrait> negativeTraits = actor.GetAllRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.NEGATIVE);
            RelationshipTrait chosenTrait = negativeTraits[UnityEngine.Random.Range(0, negativeTraits.Count)];
            CharacterManager.Instance.RemoveOneWayRelationship(actor, targetCharacter, chosenTrait.relType);
        } else if (result == "flirt") {
            //store flirtation count in both characters
            //Log: "[Character Name 1] and [Character Name 2] flirted."
            actor.FlirtWith(targetCharacter);
        } else if (result == "become lovers") {
            //Log: "[Character Name 1] and [Character Name 2] have become lovers."
            CharacterManager.Instance.CreateNewRelationshipBetween(actor, targetCharacter, RELATIONSHIP_TRAIT.LOVER);
        } else if (result == "become paramours") {
            //Log: "[Character Name 1] and [Character Name 2] have developed an affair!"
            CharacterManager.Instance.CreateNewRelationshipBetween(actor, targetCharacter, RELATIONSHIP_TRAIT.PARAMOUR);
        }

        Log chatLog = new Log(GameManager.Instance.Today(), "GoapAction", "ChatCharacter", result, this);
        chatLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        chatLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //chatLog.AddLogToInvolvedObjects();
        currentState.OverrideDescriptionLog(chatLog);

        //if (!PlayerManager.Instance.player.ShowNotificationFrom(actor, chatLog)) {
        //    PlayerManager.Instance.player.ShowNotificationFrom(targetCharacter, chatLog);
        //}

        weights.LogDictionaryValues("Chat Weights of " + actor.name + " and " + targetCharacter.name);
        Debug.Log(actor.name + " and " + targetCharacter.name + "'s chat result is " + result);

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(2);
        SchedulingManager.Instance.AddEntry(dueDate, () => actor.EndChatCharacter(targetCharacter));
    }
    #endregion

}
