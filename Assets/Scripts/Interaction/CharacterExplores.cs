using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterExplores : Interaction {

    private ILocation landmark;
    private ICharacter chosenCharacter;
    private BaseLandmark targetLandmark;

    private const string characterExploreCancelled = "Character Explore Cancelled";
    private const string characterExploreContinues = "Character Explore Continues";
    private const string characterExploreRedirected = "Character Explore Redirected";
    private const string doNothing = "Do nothing";

    //**Role Requirement**: Knight, Archer, Mage, Cleric, Marauder, Rogue, Shaman, Farmer, Miner, Sorcerer
    private List<string> allowedRoles = new List<string>() {
        "Knight",
        "Archer",
        "Mage",
        "Cleric",
        "Marauder",
        "Rogue",
        "Shaman",
        "Farmer",
        "Miner",
        "Sorcerer"
    };

    public CharacterExplores(IInteractable interactable) : base(interactable, INTERACTION_TYPE.CHARACTER_EXPLORES, 70) {
        _name = "Character Explores";
    }

    #region Overrides
    public override void CreateStates() {
        //**Mechanics**: Select a random Human, Elven or Goblin character in the tile. 
        chosenCharacter = _interactable as ICharacter;
        landmark = chosenCharacter.ownParty.specificLocation;
        //Select a different random location not owned by a Hostile faction and set it as the target location.
        targetLandmark = GetTargetLandmark();

        //If you dont have it yet, gain Intel of selected character
        if (chosenCharacter is Character) {
            PlayerManager.Instance.player.AddIntel((chosenCharacter as Character).characterIntel);
        }

        InteractionState startState = new InteractionState("Start", this);

        //action option states
        InteractionState characterExploreCancelledState = new InteractionState(characterExploreCancelled, this);
        InteractionState characterExploreContinuesState = new InteractionState(characterExploreContinues, this);
        InteractionState characterExploreRedirectedState = new InteractionState(characterExploreRedirected, this);
        InteractionState doNothingState = new InteractionState(doNothing, this);

        CreateActionOptions(startState);

        characterExploreCancelledState.SetEndEffect(() => CharacterExploreCancelledRewardEffect(characterExploreCancelledState));
        characterExploreContinuesState.SetEndEffect(() => CharacterExploreContinuesRewardEffect(characterExploreContinuesState));
        characterExploreRedirectedState.SetEndEffect(() => CharacterExploreRedirectedRewardEffect(characterExploreRedirectedState));
        doNothingState.SetEndEffect(() => DoNothingRewardEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(characterExploreCancelledState.name, characterExploreCancelledState);
        _states.Add(characterExploreContinuesState.name, characterExploreContinuesState);
        _states.Add(characterExploreRedirectedState.name, characterExploreRedirectedState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Prevent him/her from leaving.",
                duration = 0,
                needsMinion = false,
                effect = () => PreventFromLeavingEffect(state),
            };
            ActionOption takeUnit = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Convince him/her to visit elsewhere.",
                duration = 0,
                needsMinion = false,
                neededObjects = new List<System.Type>() { typeof(LocationIntel) },
                effect = () => ConvinceToVisitElsewhere(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(prevent);
            state.AddActionOption(takeUnit);
            state.AddActionOption(doNothing);
        }
    }
    #endregion

    #region Action Option Effects
    private void PreventFromLeavingEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(characterExploreCancelled, 20);
        effectWeights.AddElement(characterExploreContinues, 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void ConvinceToVisitElsewhere(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(characterExploreRedirected, 15);
        effectWeights.AddElement(characterExploreContinues, 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[doNothing]);
    }
    #endregion

    #region End Result Effects
    private void CharacterExploreCancelledRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //**Mechanics**: Character will no longer leave.
    }
    private void CharacterExploreContinuesRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //**Mechanics**: Character will start its travel to selected location
        CharacterTravelToLocation(targetLandmark);
    }
    private void CharacterExploreRedirectedRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //**Mechanics**: Character will start its travel to Location Intel assigned by the player
        BaseLandmark targetLandmarkFromArea = GetTargetLandmark(state.chosenOption.assignedLocation.location);
        CharacterTravelToLocation(targetLandmarkFromArea);
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //**Mechanics**: Character will start its travel to selected location
        CharacterTravelToLocation(targetLandmark);
    }
    #endregion

    private void CharacterTravelToLocation(BaseLandmark target) {
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.REST);
        (chosenCharacter.ownParty as CharacterParty).iactionData.AssignAction(characterAction, target.landmarkObj);
    }


    //private Character GetCharacterToMove() {
    //    List<Character> choices = new List<Character>();
    //    for (int i = 0; i < landmark.charactersWithHomeOnLandmark.Count; i++) {
    //        ICharacter currCharacter = landmark.charactersWithHomeOnLandmark[i];
    //        if (currCharacter is Character) {
    //            Character character = currCharacter as Character;
    //            if (allowedRoles.Contains(character.characterClass.className)) {
    //                choices.Add(character);
    //            }
    //        }
    //    }
    //    if (choices.Count > 0) {
    //        return choices[Random.Range(0, choices.Count)];
    //    }
    //    return null;
    //}

    private BaseLandmark GetTargetLandmark() {
        List<BaseLandmark> choices = new List<BaseLandmark>();
        List<Faction> nonHostileFactions = chosenCharacter.faction.GetFactionsWithRelationship(FACTION_RELATIONSHIP_STATUS.NON_HOSTILE);
        nonHostileFactions.Add(chosenCharacter.faction);
        for (int i = 0; i < nonHostileFactions.Count; i++) {
            Faction currFaction = nonHostileFactions[i];
            for (int j = 0; j < currFaction.ownedAreas.Count; j++) {
                Area currArea = currFaction.ownedAreas[j];
                if (chosenCharacter.ownParty.specificLocation.tileLocation.areaOfTile == null || currArea.id != chosenCharacter.ownParty.specificLocation.tileLocation.areaOfTile.id) {
                    choices.AddRange(currArea.landmarks);
                }
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }

    private BaseLandmark GetTargetLandmark(Area currArea) {
        List<BaseLandmark> choices = new List<BaseLandmark>(currArea.landmarks);
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
