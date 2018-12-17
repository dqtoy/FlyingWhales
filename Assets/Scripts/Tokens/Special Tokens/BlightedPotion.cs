using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlightedPotion : SpecialToken {

    public BlightedPotion() : base(SPECIAL_TOKEN.BLIGHTED_POTION) {
    }

    #region Overrides
    public override void CreateJointInteractionStates(Interaction interaction) {
        InteractionState inflictIllnessState = new InteractionState("Illness Inflicted", interaction);

        inflictIllnessState.SetEffect(() => InflictIllnessEffect(inflictIllnessState));

        interaction.AddState(inflictIllnessState);

        interaction.SetCurrentState(inflictIllnessState);
    }
    #endregion

    private void InflictIllnessEffect(InteractionState state) {
        string chosenIllnessName = AttributeManager.Instance.GetRandomIllness();
        state.interaction.characterInvolved.AddTrait(AttributeManager.Instance.allIllnesses[chosenIllnessName]);
        state.interaction.investigatorMinion.LevelUp();

        state.descriptionLog.AddToFillers(null, chosenIllnessName, LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(null, chosenIllnessName, LOG_IDENTIFIER.STRING_1));
    }
}
