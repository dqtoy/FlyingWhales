using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderTargetLocation : Interaction {

    private const string Start = "Start";
    private const string Induce_Raid = "Induce Raid";
    private const string Induce_Scavenge = "Induce Scavenge";
    private const string Do_Nothing = "Do Nothing";

    private LocationToken _targetLocationToken;
    private Character _raiderOrScavenger;

    public RaiderTargetLocation(Area interactable) : base(interactable, INTERACTION_TYPE.RAIDER_TARGET_LOCATION, 0) {
        _name = "Raider Target Location";
        _jobFilter = new JOB[] { JOB.RAIDER };
    }

    #region Overrides
    public override void CreateStates() {
        _targetLocationToken = _tokenTrigger as LocationToken;
        SetRaider();

        InteractionState startState = new InteractionState(Start, this);
        InteractionState induceRaidState = new InteractionState(Induce_Raid, this);
        InteractionState induceScavengeState = new InteractionState(Induce_Scavenge, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(null, _targetLocationToken.ToString(), LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(_targetLocationToken.location, _targetLocationToken.location.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        induceRaidState.SetEffect(() => InduceRaidEffect(induceRaidState));
        induceScavengeState.SetEffect(() => InduceScavengeEffect(induceScavengeState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(induceRaidState.name, induceRaidState);
        _states.Add(induceScavengeState.name, induceScavengeState);
        _states.Add(doNothingState.name, doNothingState);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption induceRaidOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Induce a raid on " + _targetLocationToken.nameInBold + ".",
                enabledTooltipText = "This location will send someone to raid " + _targetLocationToken.location.name + ".",
                effect = () => InduceRaidOption(state),
            };
            induceRaidOption.canBeDoneAction = () => CanInduceRaid(induceRaidOption);

            ActionOption induceScavengeOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Induce someone to scavenge at " + _targetLocationToken.nameInBold + ".",
                enabledTooltipText = "This location will send someone to scavenge at " + _targetLocationToken.location.name + ".",
                effect = () => InduceScavengeOption(state),
            };
            induceScavengeOption.canBeDoneAction = () => CanInduceScavenge(induceScavengeOption);

            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do Nothing.",
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(induceRaidOption);
            state.AddActionOption(induceScavengeOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private bool CanInduceRaid(ActionOption option) {
        if (_raiderOrScavenger == null) {
            option.disabledTooltipText = "This location does not have anyone capable of raiding.";
            return false;
        } else if (_targetLocationToken.location.owner == null){
            option.disabledTooltipText = "A factionless location cannot be raided.";
            return false;
        }
        return true;
    }
    private bool CanInduceScavenge(ActionOption option) {
        if (_raiderOrScavenger == null) {
            option.disabledTooltipText = "This location does not have anyone capable of scavenging.";
            return false;
        } else if (_targetLocationToken.location.owner != null) {
            option.disabledTooltipText = "A location owned by a faction cannot be scavenged.";
            return false;
        }
        return true;
    }
    private void InduceRaidOption(InteractionState state) {
        SetCurrentState(_states[Induce_Raid]);
    }
    private void InduceScavengeOption(InteractionState state) {
        SetCurrentState(_states[Induce_Scavenge]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void InduceRaidEffect(InteractionState state) {
        //investigatorCharacter.LevelUp();

        MoveToRaid moveToRaid = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RAID_EVENT, interactable) as MoveToRaid;
        moveToRaid.SetTargetArea(_targetLocationToken.location);
        _raiderOrScavenger.InduceInteraction(moveToRaid);

        state.descriptionLog.AddToFillers(_raiderOrScavenger, _raiderOrScavenger.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(_targetLocationToken.location, _targetLocationToken.location.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_raiderOrScavenger, _raiderOrScavenger.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetLocationToken.location, _targetLocationToken.location.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void InduceScavengeEffect(InteractionState state) {
        //investigatorCharacter.LevelUp();

        MoveToScavenge moveToScavenge = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT, interactable) as MoveToScavenge;
        moveToScavenge.SetTargetArea(_targetLocationToken.location);
        _raiderOrScavenger.InduceInteraction(moveToScavenge);

        state.descriptionLog.AddToFillers(_raiderOrScavenger, _raiderOrScavenger.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(_targetLocationToken.location, _targetLocationToken.location.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(_raiderOrScavenger, _raiderOrScavenger.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetLocationToken.location, _targetLocationToken.location.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void DoNothingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(null, _targetLocationToken.ToString(), LOG_IDENTIFIER.STRING_1);
        state.AddLogFiller(new LogFiller(null, _targetLocationToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    #endregion

    private void SetRaider() {
        List<Character> raiders = new List<Character>();
        for (int i = 0; i < interactable.areaResidents.Count; i++) {
            Character resident = interactable.areaResidents[i];
            if(resident.isIdle && !resident.isDefender && resident.job.jobType == JOB.RAIDER && resident.faction.id == interactable.owner.id 
                && resident.specificLocation.id == interactable.id) {
                raiders.Add(resident);
            }
        }
        if(raiders.Count > 0) {
            _raiderOrScavenger = raiders[UnityEngine.Random.Range(0, raiders.Count)];
        }
    }
}
