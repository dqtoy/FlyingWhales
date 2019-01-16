using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionDiscovered : Interaction {

    public FactionDiscovered(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.FACTION_DISCOVERED, 0) {
        _name = "Faction Discovered";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Minion Name] has discovered a new faction called [Faction Name] which owns [Location Name].
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        startState.SetEffect(() => FactionDiscoveredEffect(startState));

        _states.Add(startState.name, startState);
        SetCurrentState(startState);
    }
    #endregion

    private void FactionDiscoveredEffect(InteractionState state) {
        //**Mechanics**: Unlock Faction Intel
        //**Log**: [Minion Name] obtained intel about [Faction Name].
        state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
        PlayerManager.Instance.player.AddToken(interactable.owner.factionToken);
        investigatorCharacter.LevelUp();
    }
}
