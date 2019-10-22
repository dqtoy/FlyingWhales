using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosOrb : Artifact {

    public ChaosOrb() : base(ARTIFACT_TYPE.Chaos_Orb) {
        //poiGoapActions.Add(INTERACTION_TYPE.INSPECT);
    }
    //public Chaos_Orb(SaveDataArtifactSlot data) : base(data) {
    //    //poiGoapActions.Add(INTERACTION_TYPE.INSPECT);
    //}
    public ChaosOrb(SaveDataArtifact data) : base(data) {
        //poiGoapActions.Add(INTERACTION_TYPE.INSPECT);
    }

    public override void OnInspect(Character inspectedBy) { //, out Log result
        base.OnInspect(inspectedBy); //, out result
        //inspectedBy.currentAction.SetEndAction(OnInspectActionDone);
        OnInspectActionDone(inspectedBy);
        //if (LocalizationManager.Instance.HasLocalizedValue("Artifact", this.GetType().ToString(), "on_inspect")) {
        //    Log result = new Log(GameManager.Instance.Today(), "Artifact", this.GetType().ToString(), "on_inspect");
        //    result.AddToFillers(inspectedBy, inspectedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //    //result.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //    inspectedBy.RegisterLogAndShowNotifToThisCharacterOnly(result, onlyClickedCharacter: false);
        //}
    }
    private void OnInspectActionDone(Character inspectedBy) {
        //action.actor.GoapActionResult(result, action);
        //Characters that inspect the Chaos Orb may be permanently berserked.
        CharacterState state = inspectedBy.stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED);
        state.SetIsUnending(true);
    }

    public override string ToString() {
        return "Chaos Orb";
    }

}
