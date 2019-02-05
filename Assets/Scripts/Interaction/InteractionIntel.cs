using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionIntel {

    public Interaction connectedInteraction { get; private set; }

    public Character actor { get { return connectedInteraction.characterInvolved; } }
    public object target { get { return connectedInteraction.GetTarget(); } }
    public INTERACTION_CATEGORY[] categories {
        get {
            if (!InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.type)) {
                return null;
            }
            return InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.type).categories;
        }
    }
    public INTERACTION_ALIGNMENT alignment {
        get {
            if (!InteractionManager.Instance.interactionCategoryAndAlignment.ContainsKey(connectedInteraction.type)) {
                return INTERACTION_ALIGNMENT.NEUTRAL;
            }
            return InteractionManager.Instance.GetCategoryAndAlignment(connectedInteraction.type).alignment;
        }
    }
    public bool isCompleted { get { return connectedInteraction.isDone; } }

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
        return text;
    }
}
