using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidEventFaction : Interaction {

    private const string Supply_Raided = "Supply Raided";
    private const string No_Supply = "No Supply";

    private LocationStructure structure;

    public override LocationStructure actionStructureLocation {
        get { return structure; }
    }

    public RaidEventFaction(Area interactable) : base(interactable, INTERACTION_TYPE.RAID_EVENT_FACTION, 0) {
        _name = "Raid Event";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalRaidSuccess = new InteractionState(Supply_Raided, this);
        InteractionState normalRaidFail = new InteractionState(No_Supply, this);

        structure = interactable.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        _characterInvolved.MoveToAnotherStructure(structure);

        AddToDebugLog(_characterInvolved.name + " will scavenge " + structure.ToString());

        CreateActionOptions(startState);
        normalRaidSuccess.SetEffect(() => NormalRaidSuccessRewardEffect(normalRaidSuccess));
        normalRaidFail.SetEffect(() => NormalRaidFailRewardEffect(normalRaidFail));

        _states.Add(startState.name, startState);
        _states.Add(normalRaidSuccess.name, normalRaidSuccess);
        _states.Add(normalRaidFail.name, normalRaidFail);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (interactable.owner == null
            || interactable.owner.id == character.faction.id) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Action Option Effect
    private void DoNothingEffect(InteractionState state) {
        string nextState = string.Empty;
        SupplyPile pile = structure.GetSupplyPile();
        if (pile != null && pile.HasSupply()) {
            nextState = Supply_Raided;
        } else {
            nextState = No_Supply;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    private void NormalRaidSuccessRewardEffect(InteractionState state) {
        SupplyPile pile = structure.GetSupplyPile();
        int obtainedSupply = pile.GetSuppliesObtained();
        //_characterInvolved.homeArea.GetSuppliesFrom(interactable, obtainedSupply);
        pile.TransferSuppliesTo(_characterInvolved.homeArea, obtainedSupply);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void NormalRaidFailRewardEffect(InteractionState state) {
        
    }

    //private void TransferSupplies(int amount, Area recieving, Area source) {
    //    recieving.AdjustSuppliesInBank(amount);
    //    source.AdjustSuppliesInBank(-amount);
    //}
}
