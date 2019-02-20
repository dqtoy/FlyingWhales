using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEncountered : Interaction {
    public CharacterEncountered(Area interactable) : base(interactable, INTERACTION_TYPE.CHARACTER_ENCOUNTERED, 70) {

    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        if (_characterInvolved.isFactionless) {
            //**Neutral Text Description**: [Minion Name] has encountered a [Race] named [Character Name]. [He/She] is an unaligned [Class].
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-factioned_description", this);
            startStateDescriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(_characterInvolved.race), LOG_IDENTIFIER.STRING_1);
            startStateDescriptionLog.AddToFillers(null, characterInvolved.characterClass.className, LOG_IDENTIFIER.STRING_2);
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        } else {
            //**Factioned Text Description**: [Minion Name] has encountered a [Race] named [Character Name]. [He/She] is a [Class] belonging to [Faction Name].
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-neutral_description", this);
            startStateDescriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(_characterInvolved.race), LOG_IDENTIFIER.STRING_1);
            startStateDescriptionLog.AddToFillers(null, characterInvolved.characterClass.className, LOG_IDENTIFIER.STRING_2);
            startStateDescriptionLog.AddToFillers(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        }

        startState.SetEffect(() => CharacterEncounteredEffect(startState));

        _states.Add(startState.name, startState);
        //SetCurrentState(startState);
    }
    #endregion

    private void CharacterEncounteredEffect(InteractionState state) {
        //**Mechanics**: Unlock Character Intel
        //**Log**: [Minion Name] obtained intel about [Character Name].
        //PlayerManager.Instance.player.AddIntel(_characterInvolved.characterIntel);
        //investigatorCharacter.LevelUp();
    }
}
