using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionIntel {

    public Interaction connectedInteraction { get; private set; }

    public Character actor { get { return connectedInteraction.characterInvolved; } }
    public object target { get { return connectedInteraction.GetTarget(); } }
    public INTERACTION_CATEGORY[] categories {
        get {
            if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.type)) {
                return InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.type).categories;
            } else if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.pairedInteractionType)) {
                return InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.pairedInteractionType).categories;
            }
            return null;
        }
    }
    public INTERACTION_ALIGNMENT alignment {
        get {
            if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.type)) {
                return InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.type).alignment;
            } else if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.pairedInteractionType)) {
                return InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.pairedInteractionType).alignment;
            }
            return INTERACTION_ALIGNMENT.NEUTRAL;
        }
    }
    public bool isCompleted {
        get {
            if (connectedInteraction.name.Contains("Move To")) {
                return false; //might be a better solution to this, but for now it'll do
            }
            return connectedInteraction.isDone;
        }
    }
    public int actionDeadline {
        get {
            if (isCompleted) {
                return connectedInteraction.dayCompleted;
            } else {
                if (connectedInteraction.name.Contains("Move To")) {
                    return connectedInteraction.dayStarted + PathGenerator.Instance.GetTravelTime(actor.specificLocation.coreTile, actionLocation.coreTile);
                }
                return actor.currentInteractionTick; //TODO: this should be travel time from characters current location to the target location
            }
        }
    }
    public InteractionCharacterEffect[] effectsOnActor {
        get {
            //if (isCompleted) {
            //    return connectedInteraction.actualEffectsOnActor.ToArray();
            //} else {
                if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.type)) {
                    return InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.type).actorEffect;
                } else if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.pairedInteractionType)) {
                    return InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.pairedInteractionType).actorEffect;
                }
                return null;
            //}
        }
    }
    public InteractionCharacterEffect[] effectsOnTarget {
        get {
            //if (isCompleted) {
            //    return connectedInteraction.actualEffectsOnTarget.ToArray();
            //} else {
                if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.type)) {
                    return InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.type).targetCharacterEffect;
                } else if (InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.pairedInteractionType)) {
                    return InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.pairedInteractionType).targetCharacterEffect;
                }
                return null;
            //}
        }
    }
    public Area actionLocation {
        get {
            if (connectedInteraction.name.Contains("Move To") && connectedInteraction.targetArea != null) {
                return connectedInteraction.targetArea;
            }
            return connectedInteraction.interactable;
        }
    }
    public LocationStructure actionLocationStructure {
        get {
            if (connectedInteraction.name.Contains("Move To")) {
                return null; //because move to usually doesn't know what target location
            }
            return connectedInteraction.actionStructureLocation;
        }
    }

    public Log obtainedFromLog { get; private set; }

    public InteractionIntel(Interaction interaction) {
        SetConnectedInteraction(interaction);
    }
    public void SetConnectedInteraction(Interaction connectedInteraction) {
        this.connectedInteraction = connectedInteraction;
    }

    public void SetLog(Log log) {
        this.obtainedFromLog = log;
    }

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
        text += "\n<b>isCompleted?:</b> " + isCompleted.ToString();
        text += "\n<b>Action Deadline:</b> " + actionDeadline.ToString();
        text += "\n<b>Action Location:</b> " + actionLocation.name;
        text += "\n<b>Action Structure Location:</b> " + actionLocationStructure?.ToString() ?? "None";
        text += "\n<b>Effects on Actor:</b> ";
        if (effectsOnActor != null) {
            for (int i = 0; i < effectsOnActor.Length; i++) {
                InteractionCharacterEffect currEffect = effectsOnActor[i];
                text += "\n\t" + i.ToString() + ". " + currEffect.effect.ToString();
                if (currEffect.effectString != null) {
                    text += " - ";
                    for (int j = 0; j < currEffect.effectString.Length; j++) {
                        text += "|" + currEffect.effectString[j] + "|";
                    }
                }
                
            }
        } else {
            text += "None";
        }
        text += "\n<b>Effects on Target:</b> ";
        if (effectsOnTarget != null) {
            for (int i = 0; i < effectsOnTarget.Length; i++) {
                InteractionCharacterEffect currEffect = effectsOnTarget[i];
                text += "\n\t" + i.ToString() + ". " + currEffect.effect.ToString();
                if (currEffect.effectString != null) {
                    text += " - ";
                    for (int j = 0; j < currEffect.effectString.Length; j++) {
                        text += "|" + currEffect.effectString[j] + "|";
                    }
                }
            }
        } else {
            text += "None";
        }
        return text;
    }
}
