using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenInteractionState : InteractionState {
    protected Token _token;
    protected Character _tokenUser;
    protected object _target;

    #region getters/setters
    public Character tokenUser {
        get { return _tokenUser; }
    }
    public object target {
        get { return _target; }
    }
    #endregion
    public TokenInteractionState(string name, Interaction interaction, Token token) : base (name, interaction) {
        _token = token;
    }

    #region Overrides
    public override void CreateLogs() {
        if (_descriptionLog == null) {
            _descriptionLog = new Log(GameManager.Instance.Today(), "Tokens", _token.GetType().ToString(), _name.ToLower() + "_description");
        }
        otherLogs = new List<Log>();
        List<string> keysForState = LocalizationManager.Instance.GetKeysLike("Tokens", _token.GetType().ToString(), _name.ToLower(), new string[] { "_description", "_special" });
        for (int i = 0; i < keysForState.Count; i++) {
            string currentKey = keysForState[i];
            otherLogs.Add(new Log(GameManager.Instance.Today(), "Tokens", _token.GetType().ToString(), currentKey));
        }
    }
    public override void SetDescription() {
        //TODO: make this more performant
        if (_descriptionLog != null) {
            _descriptionLog.AddToFillers(_tokenUser, _tokenUser.name, LOG_IDENTIFIER.MINION_1);
            if(_target is Character) {
                _descriptionLog.AddToFillers(_target as Character, (_target as Character).name, LOG_IDENTIFIER.TARGET_CHARACTER);
            }
            //if (interaction.characterInvolved != null && !_descriptionLog.HasFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER)) {
            //    _descriptionLog.AddToFillers(interaction.characterInvolved, interaction.characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //}
            if (!_descriptionLog.HasFillerForIdentifier(LOG_IDENTIFIER.LANDMARK_1)) {
                _descriptionLog.AddToFillers(_interaction.interactable.tileLocation.areaOfTile, _interaction.interactable.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_1);
            }
            _description = Utilities.LogReplacer(descriptionLog);
            //InteractionUI.Instance.interactionItem.SetDescription(_description, descriptionLog);
        }
        if (otherLogs != null) {
            AddLogFiller(new LogFiller(_tokenUser, _tokenUser.name, LOG_IDENTIFIER.MINION_1));
            if (_target is Character) {
                AddLogFiller(new LogFiller(_target as Character, (_target as Character).name, LOG_IDENTIFIER.TARGET_CHARACTER));
            }
            //if (interaction.characterInvolved != null) {
            //    logFillers.Add(new LogFiller(interaction.characterInvolved, interaction.characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
            //}
            if (!AlreadyHasLogFiller(LOG_IDENTIFIER.LANDMARK_1)) {
                AddLogFiller(new LogFiller(interaction.interactable.tileLocation.areaOfTile, interaction.interactable.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_1));
            }
            for (int i = 0; i < otherLogs.Count; i++) {
                Log currLog = otherLogs[i];
                currLog.SetFillers(logFillers);
                AddLogToInvolvedObjects(currLog);
            }
        }
    }
    #endregion

    #region Utilities
    public void SetTokenUserAndTarget(Character user, object target) {
        _tokenUser = user;
        _target = target;
    }
    #endregion
}
