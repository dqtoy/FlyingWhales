using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNecromancer : Interaction {
    
    private const string Start = "Start";
    private const string Necromancer_Created = "Necromancer Created";
    private const string Do_Nothing = "Do Nothing";

    public CreateNecromancer(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.CREATE_NECROMANCER, 0) {
        _name = "Create Necromancer";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState necromancerCreatedState = new InteractionState(Necromancer_Created, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(characterInvolved.race.ToString()), LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        necromancerCreatedState.SetEffect(() => NecromancerCreatedEffect(necromancerCreatedState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(necromancerCreatedState.name, necromancerCreatedState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption ritualOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Begin the ritual.",
                duration = 0,
                effect = () => RitualOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " alone.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(ritualOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion
    #region Action Options
    private void RitualOption() {
        SetCurrentState(_states[Necromancer_Created]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void NecromancerCreatedEffect(InteractionState state) {
        _characterInvolved.ChangeClass("Necromancer");
        _characterInvolved.SetForcedInteraction(null);

        Faction oldFaction = _characterInvolved.faction;
        _characterInvolved.faction.RemoveCharacter(_characterInvolved);

        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Ziranna");
        newFaction.SetLeader(_characterInvolved);
        newFaction.AddNewCharacter(_characterInvolved);
        FactionManager.Instance.neutralFaction.UnownArea(interactable.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, newFaction.raceType, interactable.tileLocation.areaOfTile);
        newFaction.GetRelationshipWith(oldFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.ENEMY);
        newFaction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.ALLY);
        newFaction.SetFactionActiveState(true);
    }
    private void DoNothingEffect(InteractionState state) {
    }
    #endregion
}
