using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
            _descriptionLog = new Log(GameManager.Instance.Today(), "Tokens", _token.GetType().ToString(), _name.ToLower() + "_description", this.interaction);
        }
        otherLogs = new List<Log>();
        if (interaction.characterInvolved == null || interaction.characterInvolved.isTracked) {
            List<string> keysForState = LocalizationManager.Instance.GetKeysLike("Tokens", _token.GetType().ToString(), _name.ToLower(), new string[] { "_description", "_special" });
            for (int i = 0; i < keysForState.Count; i++) {
                string currentKey = keysForState[i];
                otherLogs.Add(new Log(GameManager.Instance.Today(), "Tokens", _token.GetType().ToString(), currentKey, this.interaction));
            }
        } else {
            //If character involved is untracked
            if (interaction.name.ToLower().Contains("move to")) {
                //If move to, use vague left log
                //TODO: left structure log
                otherLogs.Add(new Log(GameManager.Instance.Today(), "Character", "Generic", "left_area", this.interaction));
            } else {
                if (interaction.targetCharacter != null) {
                    InteractionAttributes attributes = InteractionManager.Instance.GetCategoryAndAlignment(interaction.type, interaction.characterInvolved);
                    if (attributes.categories.Contains(INTERACTION_CATEGORY.SOCIAL) || attributes.categories.Contains(INTERACTION_CATEGORY.ROMANTIC)) {
                        //Log "did something with"
                        otherLogs.Add(new Log(GameManager.Instance.Today(), "Character", "Generic", "did_with", this.interaction));
                    } else {
                        //Log "did something to"
                        otherLogs.Add(new Log(GameManager.Instance.Today(), "Character", "Generic", "did_to", this.interaction));
                    }
                } else {
                    //No target character log
                    //TODO: Something happened to Character at Structure
                    otherLogs.Add(new Log(GameManager.Instance.Today(), "Character", "Generic", "did_something", this.interaction));
                }
            }
        }
    }
    public override void SetDescription() {
        //TODO: make this more performant
        if (_descriptionLog != null) {
            _descriptionLog.AddToFillers(_tokenUser, _tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            _descriptionLog.AddToFillers(_tokenUser.currentStructure, _tokenUser.currentStructure.ToString(), LOG_IDENTIFIER.STRUCTURE_1);

            if (_target != null && _target is Character) {
                Character targetCharacter = _target as Character;
                _descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                _descriptionLog.AddToFillers(targetCharacter.currentStructure, targetCharacter.currentStructure.ToString(), LOG_IDENTIFIER.STRUCTURE_2);
            }
            _descriptionLog.AddToFillers(_interaction.interactable, _interaction.interactable.name, LOG_IDENTIFIER.LANDMARK_1);

            _description = Utilities.LogReplacer(descriptionLog);
        }
        if (otherLogs != null) {
            AddLogFiller(new LogFiller(_tokenUser, _tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
            AddLogFiller(new LogFiller(_tokenUser.currentStructure, _tokenUser.currentStructure.ToString(), LOG_IDENTIFIER.STRUCTURE_1));

            if (_target != null && _target is Character) {
                Character targetCharacter = _target as Character;
                AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
                AddLogFiller(new LogFiller(targetCharacter.currentStructure, targetCharacter.currentStructure.ToString(), LOG_IDENTIFIER.STRUCTURE_2));
            }
            AddLogFiller(new LogFiller(interaction.interactable, interaction.interactable.name, LOG_IDENTIFIER.LANDMARK_1));

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
