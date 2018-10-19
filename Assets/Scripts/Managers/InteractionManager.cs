using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour {

    public static InteractionManager Instance = null;

    private void Awake() {
        Instance = this;
    }

    public Interaction CreateNewInteraction(INTERACTION_TYPE interactionType, IInteractable interactable) {
        switch (interactionType) {
            case INTERACTION_TYPE.BANDIT_RAID:
                return new BanditRaid(interactable);
            case INTERACTION_TYPE.INVESTIGATE:
                return new InvestigateInteraction(interactable);
            case INTERACTION_TYPE.POI_1:
                return new PointOfInterest1(interactable);
            case INTERACTION_TYPE.POI_2:
                return new PointOfInterest2(interactable);
            default:
                return null;
        }
    }
}
