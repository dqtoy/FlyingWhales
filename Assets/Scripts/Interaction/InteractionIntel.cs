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

    public InteractionIntel(Interaction interaction) {
        SetConnectedInteraction(interaction);
    }

    public void SetConnectedInteraction(Interaction connectedInteraction) {
        this.connectedInteraction = connectedInteraction;
    }
}
