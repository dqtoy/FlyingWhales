using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpansionEvent : Interaction {

    private const string Minion_Killed_Character = "Minion Killed Character";
    private const string Minion_Injured_Character = "Minion Injured Character";
    private const string Character_Killed_Minion = "Character Killed Minion";
    private const string Character_Injured_Minion = "Character Injured Minion";
    private const string Normal_Expansion = "Normal Expansion";

    public ExpansionEvent(Area interactable) : base(interactable, INTERACTION_TYPE.EXPANSION_EVENT, 0) {
        _name = "Expansion Event";
        _jobFilter = new JOB[] { JOB.INSTIGATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState minionKilledCharacter = new InteractionState(Minion_Killed_Character, this);
        InteractionState minionInjuredCharacter = new InteractionState(Minion_Injured_Character, this);
        InteractionState characterKilledMinion = new InteractionState(Character_Killed_Minion, this);
        InteractionState characterInjuredMinion = new InteractionState(Character_Injured_Minion, this);
        InteractionState normalExpansion = new InteractionState(Normal_Expansion, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        minionKilledCharacter.SetEffect(() => MinionKilledCharacterRewardEffect(minionKilledCharacter));
        minionInjuredCharacter.SetEffect(() => MinionInjuredCharacterRewardEffect(minionInjuredCharacter));
        characterKilledMinion.SetEffect(() => CharacterKilledMinionRewardEffect(characterKilledMinion));
        characterInjuredMinion.SetEffect(() => CharacterInjuredMinionRewardEffect(characterInjuredMinion));
        normalExpansion.SetEffect(() => NormalExpansionRewardEffect(normalExpansion));

        _states.Add(startState.name, startState);
        _states.Add(minionKilledCharacter.name, minionKilledCharacter);
        _states.Add(minionInjuredCharacter.name, minionInjuredCharacter);
        _states.Add(characterKilledMinion.name, characterKilledMinion);
        _states.Add(characterInjuredMinion.name, characterInjuredMinion);
        _states.Add(normalExpansion.name, normalExpansion);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption assault = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assault " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.POSSESSIVE, false) + " group. They must be stopped",
                effect = () => AssaultOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Must have instigator minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(assault);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Action Option Effect
    private void AssaultOptionEffect(InteractionState state) {
        int minionWeight = 0;
        int characterWeight = 0;
        CombatManager.Instance.GetCombatWeightsOfTwoLists(investigatorCharacter.currentParty.characters, _characterInvolved.currentParty.characters, out minionWeight, out characterWeight);
        WeightedDictionary<string> combatWeights = new WeightedDictionary<string>();
        combatWeights.AddElement("Minion Won", minionWeight);
        combatWeights.AddElement("Minion Lost", characterWeight);

        string nextState = string.Empty;
        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        switch (combatWeights.PickRandomElementGivenWeights()) {
            case "Minion Won":
                resultWeights.AddElement(Minion_Killed_Character, 20);
                resultWeights.AddElement(Minion_Injured_Character, 40);
                break;
            case "Minion Lost":
                resultWeights.AddElement(Character_Killed_Minion, 20);
                resultWeights.AddElement(Character_Injured_Minion, 40);
                break;
            default:
                break;
        }
        SetCurrentState(_states[resultWeights.PickRandomElementGivenWeights()]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Expansion]);
    }
    #endregion

    #region Reward Effects
    private void MinionKilledCharacterRewardEffect(InteractionState state) {
        //Player Relationship -1 on Expanding Faction
        _characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, -1);
        
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _characterInvolved.characterClass.className, LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, _characterInvolved.characterClass.className, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, 
            Utilities.NormalizeString(_characterInvolved.faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus.ToString()), LOG_IDENTIFIER.STRING_2));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));

        //**Mechanic**: Character Dies, Expansion cancelled
        _characterInvolved.Death();
        //**Level Up**: Instigator Minion +1
        investigatorCharacter.LevelUp();
    }
    private void MinionInjuredCharacterRewardEffect(InteractionState state) {
        //**Mechanic**: Character Injured, Expansion cancelled
        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
        //**Level Up**: Instigator Minion +1
        investigatorCharacter.LevelUp();

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _characterInvolved.characterClass.className, LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, _characterInvolved.characterClass.className, LOG_IDENTIFIER.STRING_1));
    }
    private void CharacterKilledMinionRewardEffect(InteractionState state) {
        //**Mechanic**: Minion Killed, Location becomes part of Character's faction and its Race will be set as Character's Race
        investigatorCharacter.Death();
        OwnArea(_characterInvolved);
        //Migrate Character to the new settlement
        _characterInvolved.MigrateHomeTo(interactable);
        //**Level Up**: Expanding Character +1
        _characterInvolved.LevelUp();
    }
    private void CharacterInjuredMinionRewardEffect(InteractionState state) {
        //**Mechanic**: Minion Injured, Location becomes part of Character's faction and its Race will be set as Character's Race
        investigatorCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
        OwnArea(_characterInvolved);
        //Migrate Character to the new settlement
        _characterInvolved.MigrateHomeTo(interactable);
        //**Level Up**: Expanding Character +1
        _characterInvolved.LevelUp();
    }
    private void NormalExpansionRewardEffect(InteractionState state) {
        //**Mechanic**: Location becomes part of Character's faction and its Race will be set as Character's Race
        OwnArea(_characterInvolved);
        //Migrate Character to the new settlement
        _characterInvolved.MigrateHomeTo(interactable);
        //**Level Up**: Expanding Character +1
        _characterInvolved.LevelUp();
    }
    #endregion

    private void OwnArea(Character character) {
        Area area = interactable;
        if (area.owner == null) {
            FactionManager.Instance.neutralFaction.RemoveFromOwnedAreas(area);
        }
        LandmarkManager.Instance.OwnArea(character.faction, character.race, area);
    }
}
