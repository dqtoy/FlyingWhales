using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job {
    INTERACTION_TYPE[] explorerEvents = new INTERACTION_TYPE[] { //TODO: Put this somwhere else
        INTERACTION_TYPE.INDUCE_WAR,
        INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS,
    };

    protected string _name;
    protected JOB _jobType;
    protected int _actionDuration; //-1 means no limits and no progress
    protected bool _hasCaptureEvent;
    protected bool _useInteractionTimer;
    protected bool _startedJobAction;
    protected Character _character;
    protected Interaction _createdInteraction;
    protected Token _attachedToken;
    protected Dictionary<TOKEN_TYPE, INTERACTION_TYPE> _tokenInteractionTypes;
    //protected INTERACTION_TYPE[] _characterInteractions; //For non-minion characters only
    private WeightedDictionary<RESULT> rateWeights;

    private int _currentTick;
    private bool _isJobActionPaused;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public int actionDuration {
        get { return _actionDuration; }
    }
    public JOB jobType {
        get { return _jobType; }
    }
    public Character character {
        get { return _character; }
    }
    public Interaction createdInteraction {
        get { return _createdInteraction; }
    }
    public Token attachedToken {
        get { return _attachedToken; }
    }
    #endregion

    public Job (Character character, JOB jobType) {
        _jobType = jobType;
        _name = Utilities.NormalizeString(_jobType.ToString());
        _character = character;
        //_characterInteractions = new INTERACTION_TYPE[] { INTERACTION_TYPE.RETURN_HOME };
        rateWeights = new WeightedDictionary<RESULT>();
        _useInteractionTimer = true;
    }

    #region Virtuals
    public virtual void OnAssignJob() {}
    public virtual void CaptureRandomLandmarkEvent() {}
    public virtual void CheckTokenTriggeredEvent() {
        if (_attachedToken != null) {
            //Area area = _character.specificLocation.tileLocation.areaOfTile;
            //SetJobActionPauseState(true);
            //area.SetStopDefaultInteractionsState(true);
            Interaction interaction = null;
            if (_attachedToken.tokenType != TOKEN_TYPE.SPECIAL) {
                interaction = InteractionManager.Instance.CreateNewInteraction(_tokenInteractionTypes[_attachedToken.tokenType], _character.specificLocation as BaseLandmark);
                //interaction.AddEndInteractionAction(() => SetJobActionPauseState(false));
                //interaction.AddEndInteractionAction(() => ForceDefaultAllExistingInteractions());

                if (_attachedToken.tokenType == TOKEN_TYPE.CHARACTER) {
                    CharacterToken characterToken = _attachedToken as CharacterToken;
                    characterToken.character.AddInteraction(interaction);
                } else {
                    _character.specificLocation.tileLocation.landmarkOnTile.AddInteraction(interaction);
                }
            } else {
                interaction = CreateSpecialTokenInteraction(_attachedToken as SpecialToken);
            }
            if(interaction != null) {
                interaction.SetTokenTrigger(_attachedToken);
                SetCreatedInteraction(interaction);
                _attachedToken = null;
                InteractionUI.Instance.OpenInteractionUI(_createdInteraction);
            }

        }
    }
    public virtual void ApplyActionDuration() {}
    public virtual void DoJobAction() {
        Debug.Log(GameManager.Instance.TodayLogString() + " Doing job action: " + character.name + "(" + jobType.ToString() + ")");
    }
    protected virtual bool IsTokenCompatibleWithJob(Token token) {
        if (token.tokenType == TOKEN_TYPE.SPECIAL) {
            SpecialToken specialToken = token as SpecialToken;
            if (specialToken.specialTokenType == SPECIAL_TOKEN.BOOK_OF_THE_DEAD) {
                //location is Gloomhollow Crypts, location is not owned by any Faction
                if (_character.specificLocation.tileLocation.areaOfTile.name == "Gloomhollow Crypts" && _character.specificLocation.tileLocation.areaOfTile.owner == null) {
                    for (int i = 0; i < _character.specificLocation.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
                        //location has a male Human, Goblin or Elven character that is part of a Faction
                        Character characterAtLocation = _character.specificLocation.tileLocation.areaOfTile.charactersAtLocation[i];
                        if (characterAtLocation.IsInOwnParty() && character.doNotDisturb <=0 && !characterAtLocation.currentParty.icon.isTravelling && characterAtLocation.faction != FactionManager.Instance.neutralFaction && !characterAtLocation.isLeader && characterAtLocation.gender == GENDER.MALE &&
                            (characterAtLocation.race == RACE.HUMANS || characterAtLocation.race == RACE.GOBLIN || characterAtLocation.race == RACE.ELVES)) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public virtual Interaction CreateSpecialTokenInteraction(SpecialToken specialToken) {
        if(specialToken.specialTokenType == SPECIAL_TOKEN.BOOK_OF_THE_DEAD) {
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CREATE_NECROMANCER, _character.specificLocation as BaseLandmark);
            for (int i = 0; i < _character.specificLocation.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
                //location has a male Human, Goblin or Elven character that is part of a Faction
                Character characterAtLocation = _character.specificLocation.tileLocation.areaOfTile.charactersAtLocation[i];
                if (characterAtLocation.IsInOwnParty() && character.doNotDisturb <= 0 && !characterAtLocation.currentParty.icon.isTravelling && characterAtLocation.faction != FactionManager.Instance.neutralFaction && !characterAtLocation.isLeader && characterAtLocation.gender == GENDER.MALE &&
                    (characterAtLocation.race == RACE.HUMANS || characterAtLocation.race == RACE.GOBLIN || characterAtLocation.race == RACE.ELVES)) {
                    characterAtLocation.AddInteraction(interaction);
                    break;
                }
            }
            return interaction;
        }
        return null;
    }
    protected virtual void PassiveEffect(Area area) {}
    public virtual int GetSuccessRate() { return 0; }
    public virtual int GetFailRate() { return 40; }
    public virtual int GetCritFailRate() { return 10; }
    public virtual WeightedDictionary<RESULT> GetJobRateWeights() {
        rateWeights.Clear();
        rateWeights.AddElement(RESULT.SUCCESS, GetSuccessRate());
        rateWeights.AddElement(RESULT.FAIL, GetFailRate());
        rateWeights.AddElement(RESULT.CRITICAL_FAIL, GetCritFailRate());
        return rateWeights;
    }
    #endregion

    #region Utilities
    public void SetToken(Token token) {
        _attachedToken = token;
    }
    public bool CanTokenBeAttached(Token token) {
        return token != null && IsTokenCompatibleWithJob(token);
    }
    public void StartJobAction() {
        _startedJobAction = true;
        ApplyActionDuration();
        _currentTick = 0;
        SetJobActionPauseState(false);
        //if(_actionDuration != -1) {
        //    Messenger.AddListener(Signals.DAY_STARTED, CheckJobAction);
        //}
        Messenger.AddListener(Signals.DAY_STARTED, CheckTokenTriggeredEvent);
        if (_hasCaptureEvent) {
            Messenger.AddListener(Signals.DAY_ENDED, CatchRandomEvent);
        }
        if (_useInteractionTimer) {
            _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.SetAndStartInteractionTimerJob(_actionDuration);
            _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.ShowInteractionTimerJob();
        }
    }

    //Stops Job Action entirely
    //Uses - when a minion is recalled, when job action duration ends
    public void StopJobAction() {
        _startedJobAction = false;
        if (_useInteractionTimer) {
            _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.StopInteractionTimerJob();
            _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.HideInteractionTimerJob();
        }
        //if (_actionDuration != -1) {
        //    Messenger.RemoveListener(Signals.DAY_STARTED, CheckJobAction);
        //}
        Messenger.RemoveListener(Signals.DAY_STARTED, CheckTokenTriggeredEvent);
        if (_hasCaptureEvent) {
            Messenger.RemoveListener(Signals.DAY_ENDED, CatchRandomEvent);
        }
    }
    public void StopCreatedInteraction() {
        if(_createdInteraction != null) {
            _createdInteraction.interactable.landmarkVisual.StopInteractionTimer();
            _createdInteraction.interactable.landmarkVisual.HideInteractionTimer();
            string summary = string.Empty;
            _createdInteraction.TimedOutRunDefault(ref summary);
        }
    }
    private void CheckJobAction() {
        if (_isJobActionPaused) { return; }
        if (_currentTick >= _actionDuration) {
            StopJobAction();
            DoJobAction();
            return;
        }
        _currentTick++;

    }

    protected void SetJobActionPauseState(bool state) {
        _isJobActionPaused = state;
        if (_useInteractionTimer) {
            _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.SetTimerPauseStateJob(_isJobActionPaused);
        }
    }
    public void SetCreatedInteraction(Interaction interaction) {
        _createdInteraction = interaction;
        if(_createdInteraction != null) {
            if (!_createdInteraction.hasInitialized) {
                _createdInteraction.Initialize();
            }
            _createdInteraction.SetJobAssociated(this);
        }
    }
    private void CatchRandomEvent() {
        if (_isJobActionPaused) { return; }
        CaptureRandomLandmarkEvent();
    }
    //public void CreateRandomInteractionForNonMinionCharacters() {
        //if(_characterInteractions != null) {
        //    INTERACTION_TYPE type = _characterInteractions[UnityEngine.Random.Range(0, _characterInteractions.Length)];
        //    if (InteractionManager.Instance.CanCreateInteraction(type, character)) {
        //        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(type, character.specificLocation as BaseLandmark);
        //        character.AddInteraction(interaction);
        //    }
        //}
    //}
    public void ForceDefaultAllExistingInteractions() {
        _character.specificLocation.tileLocation.areaOfTile.SetStopDefaultInteractionsState(false);
        string summary = "";
        InteractionManager.Instance.DefaultInteractionsInArea(_character.specificLocation.tileLocation.areaOfTile, ref summary);
        //_character.specificLocation.tileLocation.areaOfTile.DefaultAllExistingInteractions();
    }
    public int GetSupplyObtained(Area targetArea) {
        //When a raid succeeds, the amount of Supply obtained is based on character level.
        //5% to 15% of location's supply 
        //+1% every other level starting at level 6
        if (character.homeLandmark == null) {
            throw new System.Exception(GameManager.Instance.TodayLogString() + character.name + " does not have a home, but GetSupplyObtained needs one to function.");
        }
        Area characterHomeArea = character.homeLandmark.tileLocation.areaOfTile;
        //Area targetArea = character.specificLocation.tileLocation.areaOfTile;
        int supplyObtainedPercent = Random.Range(5, 16);
        supplyObtainedPercent += (character.level - 5);

        return Mathf.FloorToInt(targetArea.suppliesInBank * (supplyObtainedPercent / 100f));
        //characterHomeArea.AdjustSuppliesInBank(obtainedSupply);
    }
    public Interaction CreateExplorerEvent() {
        List<INTERACTION_TYPE> choices = GetValidExplorerEvents();
        if (choices.Count > 0) {
            Area area = _character.specificLocation.tileLocation.areaOfTile;
            INTERACTION_TYPE chosenType = choices[Random.Range(0, choices.Count)];
            //Get Random Explorer Event
            return InteractionManager.Instance.CreateNewInteraction(chosenType, area.coreTile.landmarkOnTile);
        }
        return null;
    }
    private List<INTERACTION_TYPE> GetValidExplorerEvents() {
        List<INTERACTION_TYPE> validTypes = new List<INTERACTION_TYPE>();
        for (int i = 0; i < explorerEvents.Length; i++) {
            INTERACTION_TYPE type = explorerEvents[i];
            if (InteractionManager.Instance.CanCreateInteraction(type, _character)) {
                validTypes.Add(type);
            }
        }
        return validTypes;
    }
    public void DoPassiveEffect(Area area) {
        if(!_startedJobAction) { return; }
        PassiveEffect(area);
    }
    #endregion
}