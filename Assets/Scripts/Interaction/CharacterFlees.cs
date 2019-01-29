using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFlees : Interaction {

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }

    public CharacterFlees(Area interactable) : base(interactable, INTERACTION_TYPE.CHARACTER_FLEES, 0) {
        _name = "Character Flees";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        startState.SetEffect(() => StartStateRewardEffect(startState));

        _targetArea = GetTargetArea();

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        _states.Add(startState.name, startState);
        SetCurrentState(startState);
    }
    #endregion

    private void StartStateRewardEffect(InteractionState state) {
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
        StartMoveToAction();
    }

    private Area GetTargetArea() {
        List<Area> choices = new List<Area>(LandmarkManager.Instance.allAreas);
        choices.Remove(_characterInvolved.specificLocation);

        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
