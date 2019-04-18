using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionIntel {

    public Interaction connectedInteraction;
    public Log obtainedFromLog;
    public Character actor;
    public object target;
    public Area actionLocation;
    public LocationStructure actionLocationStructure;
    public int actionDeadline;
    public INTERACTION_CATEGORY[] categories;
    public INTERACTION_ALIGNMENT alignment;
    public InteractionCharacterEffect[] effectsOnActor;
    public InteractionCharacterEffect[] effectsOnTarget;

    public Character targetCharacter {
        get { return target as Character; }
    }

    public InteractionIntel(Interaction interaction, Log log) {
        obtainedFromLog = log;
        connectedInteraction = interaction;
        actor = interaction.characterInvolved;
        target = interaction.GetTarget();

        //action location
        if (connectedInteraction.name.Contains("Move To") && connectedInteraction.targetArea != null) {
            actionLocation = connectedInteraction.targetArea;
        } else {
            actionLocation = connectedInteraction.interactable;
        }

        //action structure
        actionLocationStructure = null;
        //if (!connectedInteraction.name.Contains("Move To")) {
        actionLocationStructure = connectedInteraction.actionStructureLocation; //because move to usually doesn't know what target location
        //}

        //action deadline
        if (connectedInteraction.name.Contains("Move To")) {
            actionDeadline = connectedInteraction.dayStarted + PathGenerator.Instance.GetTravelTime(actor.specificLocation.coreTile, actionLocation.coreTile);
        } else {
            if (connectedInteraction.isDone) {
                actionDeadline = connectedInteraction.dayCompleted;
            } else {
                actionDeadline = actor.currentInteractionTick; //TODO: this should be travel time from characters current location to the target location
            }
        }

        categories = null;
        effectsOnActor = null;
        effectsOnTarget = null;
        alignment = INTERACTION_ALIGNMENT.NEUTRAL;

        //tracked attributes
        if (actor.isTracked || (target is Character && (target as Character).isTracked)) {
            //categories
            InteractionAttributes interactionAttributes = InteractionManager.Instance.GetCategoryAndAlignment(interaction.type, actor);
            if (interactionAttributes != null) {
                categories = interactionAttributes.categories;
            } else {
                interactionAttributes = InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.pairedInteractionType, actor);
                if (interactionAttributes != null) {
                    categories = interactionAttributes.categories;
                }
            }

            //alignment
            
            if (interactionAttributes != null) {
                alignment = interactionAttributes.alignment;
            } else {
                interactionAttributes = InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.pairedInteractionType, actor);
                if (interactionAttributes != null) {
                    alignment = interactionAttributes.alignment;
                }
            }
            //actor effect
            if (connectedInteraction.isDone && !connectedInteraction.name.Contains("Move To")) {
                effectsOnActor = connectedInteraction.actualEffectsOnActor.ToArray();
            } else {
                if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.type)) {
                    effectsOnActor = InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.type, actor).actorEffect;
                } else if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.pairedInteractionType)) {
                    effectsOnActor = InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.pairedInteractionType, actor).actorEffect;
                }
            }
            //target effect
            if (connectedInteraction.isDone && !connectedInteraction.name.Contains("Move To")) {
                effectsOnTarget = connectedInteraction.actualEffectsOnActor.ToArray();
            } else {
                if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.type)) {
                    effectsOnTarget = InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.type, actor).actorEffect;
                } else if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.pairedInteractionType)) {
                    effectsOnTarget = InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.pairedInteractionType, actor).actorEffect;
                }
            }
        }
    }
    public bool IsOfCategory(INTERACTION_CATEGORY category) {
        if (categories != null) {
            for (int i = 0; i < categories.Length; i++) {
                if (categories[i] == category) {
                    return true;
                }
            }
        }
        return false;
    }

    #region Actor Effects
    public bool HasActorEffect(INTERACTION_CHARACTER_EFFECT effect, string effectString) {
        if (effectsOnActor != null) {
            for (int i = 0; i < effectsOnActor.Length; i++) {
                InteractionCharacterEffect currEffect = effectsOnActor[i];
                if (currEffect.effect == effect) {
                    if (currEffect.effectString == effectString) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public bool HasActorEffect(TRAIT_EFFECT traitEffect, TRAIT_TYPE type) {
        if (effectsOnActor != null) {
            for (int i = 0; i < effectsOnActor.Length; i++) {
                InteractionCharacterEffect currEffect = effectsOnActor[i];
                if (currEffect.effect == INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN) {
                    if (AttributeManager.Instance.allTraits.ContainsKey(currEffect.effectString)
                        && AttributeManager.Instance.allTraits[currEffect.effectString].effect == traitEffect
                        && AttributeManager.Instance.allTraits[currEffect.effectString].type == type) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public InteractionCharacterEffect GetActorEffect(TRAIT_EFFECT traitEffect, TRAIT_TYPE type) {
        if (effectsOnActor != null) {
            for (int i = 0; i < effectsOnActor.Length; i++) {
                InteractionCharacterEffect currEffect = effectsOnActor[i];
                if (currEffect.effect == INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN) {
                    if (AttributeManager.Instance.allTraits.ContainsKey(currEffect.effectString)
                        && AttributeManager.Instance.allTraits[currEffect.effectString].effect == traitEffect
                        && AttributeManager.Instance.allTraits[currEffect.effectString].type == type) {
                        return currEffect;
                    }
                }
            }
        }
        return new InteractionCharacterEffect();
    }
    #endregion

    #region Target Effects
    public bool HasTargetEffect(INTERACTION_CHARACTER_EFFECT effect, string effectString) {
        if (effectsOnTarget != null) {
            for (int i = 0; i < effectsOnTarget.Length; i++) {
                InteractionCharacterEffect currEffect = effectsOnTarget[i];
                if (currEffect.effect == effect) {
                    if (currEffect.effectString == effectString) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public bool HasTargetEffect(TRAIT_EFFECT traitEffect, TRAIT_TYPE type) {
        if (effectsOnTarget != null) {
            for (int i = 0; i < effectsOnTarget.Length; i++) {
                InteractionCharacterEffect currEffect = effectsOnTarget[i];
                if (currEffect.effect == INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN) {
                    if (AttributeManager.Instance.allTraits.ContainsKey(currEffect.effectString)
                        && AttributeManager.Instance.allTraits[currEffect.effectString].effect == traitEffect
                        && AttributeManager.Instance.allTraits[currEffect.effectString].type == type) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public InteractionCharacterEffect GetTargetEffect(TRAIT_EFFECT traitEffect, TRAIT_TYPE type) {
        if (effectsOnTarget != null) {
            for (int i = 0; i < effectsOnTarget.Length; i++) {
                InteractionCharacterEffect currEffect = effectsOnTarget[i];
                if (currEffect.effect == INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN) {
                    if (AttributeManager.Instance.allTraits.ContainsKey(currEffect.effectString)
                        && AttributeManager.Instance.allTraits[currEffect.effectString].effect == traitEffect
                        && AttributeManager.Instance.allTraits[currEffect.effectString].type == type) {
                        return currEffect;
                    }
                }
            }
        }
        return new InteractionCharacterEffect();
    }
    #endregion


    public string GetDebugInfo() {
        string text = connectedInteraction.ToString() + " Intel Data: ";
        text += "\n<b>Interaction:</b> " + connectedInteraction.ToString();
        text += "\n<b>Actor:</b> " + actor.name;
        text += "\n<b>Target:</b> " + target?.ToString() ?? "None";
        text += "\n<b>Categories:</b> ";
        if (categories == null) {
            text += "None";
        } else {
            for (int i = 0; i < categories.Length; i++) {
                text += "|" + categories[i].ToString() + "|";
            }
        }
        text += "\n<b>Alignment:</b> " + alignment.ToString();
        text += "\n<b>Action Deadline:</b> " + GameManager.ConvertTickToTime(actionDeadline);
        text += "\n<b>Action Location:</b> " + actionLocation.name;
        text += "\n<b>Action Structure Location:</b> " + actionLocationStructure?.ToString() ?? "None";
        text += "\n<b>Effects on Actor:</b> ";
        if (effectsOnActor != null) {
            for (int i = 0; i < effectsOnActor.Length; i++) {
                InteractionCharacterEffect currEffect = effectsOnActor[i];
                text += "\n\t" + i.ToString() + ". " + currEffect.effect.ToString() + " - |" + currEffect.effectString + "|";
            }
        } else {
            text += "None";
        }
        text += "\n<b>Effects on Target:</b> ";
        if (effectsOnTarget != null) {
            for (int i = 0; i < effectsOnTarget.Length; i++) {
                InteractionCharacterEffect currEffect = effectsOnTarget[i];
                text += "\n\t" + i.ToString() + ". " + currEffect.effect.ToString() + " - |" + currEffect.effectString + "|";
            }
        } else {
            text += "None";
        }
        return text;
    }
}
