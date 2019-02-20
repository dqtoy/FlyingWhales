using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExploreEventFaction : Interaction {

    private const string Explore_Item_Found = "Explore Item Found";
    private const string Explore_Character_Encountered = "Explore Character Encountered";
    private const string Explore_Landmark_Found = "Explore Landmark Found";
    private const string Explore_Supply_Pile_Found = "Explore Supply Pile Found";
    private const string Explore_Nothing_Found = "Explore Nothing Found";

    private LocationStructure structure;
    private IPointOfInterest poi;

    public ExploreEventFaction(Area interactable) : base(interactable, INTERACTION_TYPE.EXPLORE_EVENT_FACTION, 0) {
        _name = "Explore Event Faction";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState exploreItemFound = new InteractionState(Explore_Item_Found, this);
        InteractionState exploreCharacter = new InteractionState(Explore_Character_Encountered, this);
        InteractionState exploreLandmark = new InteractionState(Explore_Landmark_Found, this);
        InteractionState exploreSupply = new InteractionState(Explore_Supply_Pile_Found, this);
        InteractionState exploreNothing = new InteractionState(Explore_Nothing_Found, this);

        structure = interactable.GetRandomStructureOfType(STRUCTURE_TYPE.DUNGEON);

        CreateActionOptions(startState);
        exploreItemFound.SetEffect(() => ExploreItemRewardEffect(exploreItemFound));
        exploreCharacter.SetEffect(() => ExploreCharacterRewardEffect(exploreCharacter));
        exploreLandmark.SetEffect(() => ExploreLandmarkRewardEffect(exploreLandmark));
        exploreSupply.SetEffect(() => ExploreSupplyRewardEffect(exploreSupply));
        exploreNothing.SetEffect(() => ExploreNothingRewardEffect(exploreNothing));

        _states.Add(startState.name, startState);
        _states.Add(exploreItemFound.name, exploreItemFound);
        _states.Add(exploreCharacter.name, exploreCharacter);
        _states.Add(exploreLandmark.name, exploreLandmark);
        _states.Add(exploreSupply.name, exploreSupply);
        _states.Add(exploreNothing.name, exploreNothing);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override object GetTarget() {
        return poi;
    }
    #endregion

    #region Action Option Effects
    private void DoNothingOptionEffect(InteractionState state) {
        //poi = structure.GetRandomPOI();
        List<IPointOfInterest> choices = new List<IPointOfInterest>(
            structure.pointsOfInterest.Where(x => x.poiType != POINT_OF_INTEREST_TYPE.CHARACTER 
            && x.poiType != POINT_OF_INTEREST_TYPE.LANDMARK 
            && x.poiType != POINT_OF_INTEREST_TYPE.CORPSE 
            && x.poiType != POINT_OF_INTEREST_TYPE.FOOD)
        );
        poi =  choices[Random.Range(0, choices.Count)];
        string nextState = string.Empty;
        if (poi != null) {
            switch (poi.poiType) {
                case POINT_OF_INTEREST_TYPE.ITEM:
                    nextState = Explore_Item_Found;
                    break;
                case POINT_OF_INTEREST_TYPE.LANDMARK:
                    nextState = Explore_Landmark_Found;
                    break;
                case POINT_OF_INTEREST_TYPE.CHARACTER:
                    nextState = Explore_Character_Encountered;
                    break;
                case POINT_OF_INTEREST_TYPE.SUPLY_PILE:
                    nextState = Explore_Supply_Pile_Found;
                    break;
                default:
                    nextState = Explore_Nothing_Found;
                    break;
            }
        } else {
            nextState = Explore_Nothing_Found;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    private void ExploreItemRewardEffect(InteractionState state) {
        SpecialToken item = poi as SpecialToken;
        //**Mechanics**: Give the selected Item to the character
        _characterInvolved.PickUpToken(item, interactable);

        state.descriptionLog.AddToFillers(null, item.nameInBold, LOG_IDENTIFIER.STRING_1);
        state.AddLogFiller(new LogFiller(null, item.nameInBold, LOG_IDENTIFIER.STRING_1));
    }
    private void ExploreCharacterRewardEffect(InteractionState state) {
        Character encounteredCharacter = poi as Character;
        //**Mechanics**: Trigger Character Encounter Handling passing Character Name 1 and Character Name 2 as parameters

        state.descriptionLog.AddToFillers(encounteredCharacter, encounteredCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(encounteredCharacter, encounteredCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void ExploreLandmarkRewardEffect(InteractionState state) {
        //**Mechanics**: Activate Landmark effect (pass the character as a parameter).
    }
    private void ExploreSupplyRewardEffect(InteractionState state) {
        SupplyPile supplyPile = poi as SupplyPile;
        //**Mechanics**: Obtain amount based on the location's range.
        int obtainedSupply = supplyPile.GetAndReduceSuppliesObtained(_characterInvolved.homeArea);
        state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    
    private void ExploreNothingRewardEffect(InteractionState state) {

    }
}
