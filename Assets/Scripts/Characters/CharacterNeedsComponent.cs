using System;
using Traits;
using UnityEngine;

public class CharacterNeedsComponent {

    private readonly Character _character;
    public int doNotGetHungry { get; private set; }
    public int doNotGetTired{ get; private set; }
    public int doNotGetLonely{ get; private set; }
    
    public bool isStarving => fullness <= FULLNESS_THRESHOLD_2;
    public bool isExhausted => tiredness <= TIREDNESS_THRESHOLD_2;
    public bool isForlorn => happiness <= HAPPINESS_THRESHOLD_2;
    public bool isHungry => fullness <= FULLNESS_THRESHOLD_1;
    public bool isTired => tiredness <= TIREDNESS_THRESHOLD_1;
    public bool isLonely => happiness <= HAPPINESS_THRESHOLD_1;

    //Tiredness
    public int tiredness { get; private set; }
    public int tirednessDecreaseRate { get; private set; }
    public int tirednessForcedTick { get; private set; }
    public int currentSleepTicks { get; private set; }
    public int sleepScheduleJobID { get; set; }
    public bool hasCancelledSleepSchedule { get; private set; }
    private int tirednessLowerBound; //how low can this characters tiredness go
    public const int TIREDNESS_DEFAULT = 15000;
    public const int TIREDNESS_THRESHOLD_1 = 10000;
    public const int TIREDNESS_THRESHOLD_2 = 5000;
    
    //Fullness
    public int fullness { get; private set; }
    public int fullnessDecreaseRate { get; private set; }
    public int fullnessForcedTick { get; private set; }
    private int fullnessLowerBound; //how low can this characters fullness go
    public const int FULLNESS_DEFAULT = 15000;
    public const int FULLNESS_THRESHOLD_1 = 10000;
    public const int FULLNESS_THRESHOLD_2 = 5000;

    //Happiness
    public int happiness { get; private set; }
    public int happinessDecreaseRate { get; private set; }
    private int happinessLowerBound; //how low can this characters happiness go
    public const int HAPPINESS_DEFAULT = 15000;
    public const int HAPPINESS_THRESHOLD_1 = 10000;
    public const int HAPPINESS_THRESHOLD_2 = 5000;

    public bool hasForcedFullness { get; set; }
    public bool hasForcedTiredness { get; set; }
    public TIME_IN_WORDS forcedFullnessRecoveryTimeInWords { get; private set; }
    public TIME_IN_WORDS forcedTirednessRecoveryTimeInWords { get; private set; }

    public CharacterNeedsComponent(Character character) {
        this._character = character;
        tirednessLowerBound = 0;
        fullnessLowerBound = 0;
        happinessLowerBound = 0;
        SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
        SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
        SetFullnessForcedTick();
        SetTirednessForcedTick();
        
    }
    
    #region Initialization
    public void SubscribeToSignals() {
        Messenger.AddListener(Signals.HOUR_STARTED, DecreaseNeeds);
        Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB, OnCharacterFinishedJob);
    }
    public void UnsubscribeToSignals() {
        Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseNeeds);
        Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB, OnCharacterFinishedJob);
    }
    public void DailyGoapProcesses() {
        hasForcedFullness = false;
        hasForcedTiredness = false;
    }
    public void Initialize() {
        //NOTE: These values will be randomized when this character is placed in his/her area map.
        tiredness = TIREDNESS_DEFAULT;
        fullness = FULLNESS_DEFAULT;
        happiness = HAPPINESS_DEFAULT;
    }
    public void InitialCharacterPlacement() {
        tiredness = TIREDNESS_DEFAULT;
        if (_character.role.roleType != CHARACTER_ROLE.MINION) {
            ////Fullness value between 2600 and full.
            //SetFullness(UnityEngine.Random.Range(2600, FULLNESS_DEFAULT + 1));
            ////Happiness value between 2600 and full.
            //SetHappiness(UnityEngine.Random.Range(2600, HAPPINESS_DEFAULT + 1));
            fullness = FULLNESS_DEFAULT;
            happiness = HAPPINESS_DEFAULT;
        } else {
            fullness = FULLNESS_DEFAULT;
            happiness = HAPPINESS_DEFAULT;
        }
    }
    #endregion

    #region Loading
    public void LoadAllStatsOfCharacter(SaveDataCharacter data) {
        tiredness = data.tiredness;
        fullness = data.fullness;
        happiness = data.happiness;
        fullnessDecreaseRate = data.fullnessDecreaseRate;
        tirednessDecreaseRate = data.tirednessDecreaseRate;
        happinessDecreaseRate = data.happinessDecreaseRate;
        SetForcedFullnessRecoveryTimeInWords(data.forcedFullnessRecoveryTimeInWords);
        SetForcedTirednessRecoveryTimeInWords(data.forcedTirednessRecoveryTimeInWords);
        SetFullnessForcedTick(data.fullnessForcedTick);
        SetTirednessForcedTick(data.tirednessForcedTick);
        currentSleepTicks = data.currentSleepTicks;
        sleepScheduleJobID = data.sleepScheduleJobID;
        hasCancelledSleepSchedule = data.hasCancelledSleepSchedule;
    }
    #endregion

    private bool HasNeeds() {
        return _character.race != RACE.SKELETON && _character.characterClass.className != "Zombie" && !_character.returnedToLife && _character.isAtHomeRegion && 
               _character.homeRegion.area != null; //Characters living on a region without a settlement must not decrease needs
    }
    public void DecreaseNeeds() {
        if (HasNeeds() == false) {
            return;
        }
        if (doNotGetHungry <= 0) {
            AdjustFullness(-(CharacterManager.FULLNESS_DECREASE_RATE + fullnessDecreaseRate));
        }
        if (doNotGetTired <= 0) {
            AdjustTiredness(-(CharacterManager.TIREDNESS_DECREASE_RATE + tirednessDecreaseRate));
        }
        if (doNotGetLonely <= 0) {
            AdjustHappiness(-(CharacterManager.HAPPINESS_DECREASE_RATE + happinessDecreaseRate));
        }
    }
    public string GetNeedsSummary() {
        string summary = "Fullness: " + fullness.ToString() + "/" + FULLNESS_DEFAULT.ToString();
        summary += "\nTiredness: " + tiredness.ToString() + "/" + TIREDNESS_DEFAULT.ToString();
        summary += "\nHappiness: " + happiness.ToString() + "/" + HAPPINESS_DEFAULT.ToString();
        return summary;
    }
    public void AdjustFullnessDecreaseRate(int amount) {
        fullnessDecreaseRate += amount;
    }
    public void AdjustTirednessDecreaseRate(int amount) {
        tirednessDecreaseRate += amount;
    }
    public void AdjustHappinessDecreaseRate(int amount) {
        happinessDecreaseRate += amount;
    }
    public void SetTirednessLowerBound(int amount) {
        tirednessLowerBound = amount;
    }
    public void SetFullnessLowerBound(int amount) {
        fullnessLowerBound = amount;
    }
    public void SetHappinessLowerBound(int amount) {
        happinessLowerBound = amount;
    }

    #region Tiredness
    public void ResetTirednessMeter() {
        tiredness = TIREDNESS_DEFAULT;
        RemoveTiredOrExhausted();
    }
    public void AdjustTiredness(int adjustment) {
        tiredness += adjustment;
        tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            _character.Death("exhaustion");
        } else if (isExhausted) {
            _character.traitContainer.RemoveTrait(_character, "Tired");
            if (_character.traitContainer.AddTrait(_character, "Exhausted")) {
                Messenger.Broadcast<Character, string>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, _character, "exhausted");
            }
        } else if (isTired) {
            _character.traitContainer.RemoveTrait(_character, "Exhausted");
            _character.traitContainer.AddTrait(_character, "Tired");
            //PlanTirednessRecoveryActions();
        } else {
            //tiredness is higher than both thresholds
            _character.needsComponent.RemoveTiredOrExhausted();
        }
    }
    public void ExhaustCharacter(Character character) {
        if (!isExhausted) {
            int diff = tiredness - TIREDNESS_THRESHOLD_2;
            this.AdjustTiredness(-diff);
        }
    }
    public void SetTiredness(int amount) {
        tiredness = amount;
        tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, TIREDNESS_DEFAULT);
        if (tiredness == 0) {
            _character.Death("exhaustion");
        } else if (isExhausted) {
            _character.traitContainer.RemoveTrait(_character, "Tired");
            _character.traitContainer.AddTrait(_character, "Exhausted");
        } else if (isTired) {
            _character.traitContainer.RemoveTrait(_character, "Exhausted");
            _character.traitContainer.AddTrait(_character, "Tired");
        } else {
            //tiredness is higher than both thresholds
            _character.needsComponent.RemoveTiredOrExhausted();
        }
    }
    private void RemoveTiredOrExhausted() {
        if (_character.traitContainer.RemoveTrait(_character, "Tired") == false) {
            _character.traitContainer.RemoveTrait(_character, "Exhausted");
        }
    }
    public void SetTirednessForcedTick() {
        if (!hasForcedTiredness) {
            if (forcedTirednessRecoveryTimeInWords == GameManager.GetCurrentTimeInWordsOfTick()) {
                //If the forced recovery job has not been done yet and the character is already on the time of day when it is supposed to be done,
                //the tick that will be assigned will be ensured that the character will not miss it
                //Example if the time of day is Afternoon, the supposed tick range for it is 145 - 204
                //So if the current tick of the game is already in 160, the range must be adjusted to 161 - 204, so as to ensure that the character will hit it
                //But if the current tick of the game is already in 204, it cannot be 204 - 204, so, it will revert back to 145 - 204 
                int newTick = GameManager.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords, GameManager.Instance.tick + 1);
                TIME_IN_WORDS timeInWords = GameManager.GetTimeInWordsOfTick(newTick);
                if(timeInWords != forcedTirednessRecoveryTimeInWords) {
                    newTick = GameManager.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords);
                }
                tirednessForcedTick = newTick;
                return;
            }
        }
        tirednessForcedTick = GameManager.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords);
    }
    public void SetTirednessForcedTick(int tick) {
        tirednessForcedTick = tick;
    }
    public void SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS timeInWords) {
        forcedTirednessRecoveryTimeInWords = timeInWords;
    }
    public void AdjustDoNotGetTired(int amount) {
        doNotGetTired += amount;
        doNotGetTired = Math.Max(doNotGetTired, 0);
    }
    public bool PlanTirednessRecoveryActions(Character character) {
        if (character.doNotDisturb > 0 || !character.canWitness) {
            return false;
        }
        if (this.isExhausted) {
            if (!character.jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                //If there is already a TIREDNESS_RECOVERY JOB and the character becomes Exhausted, replace TIREDNESS_RECOVERY with TIREDNESS_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem tirednessRecoveryJob = character.jobQueue.GetJob(JOB_TYPE.TIREDNESS_RECOVERY);
                if (tirednessRecoveryJob != null) {
                    //Replace this with Tiredness Recovery Exhausted only if the character is not doing the Tiredness Recovery Job already
                    JobQueueItem currJob = character.currentJob;
                    if (currJob == tirednessRecoveryJob) {
                        return false;
                    } else {
                        tirednessRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                bool triggerSpooked = false;
                Spooked spooked = character.traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerSpooked) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //job.SetCancelOnFail(true);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    spooked.TriggerFeelingSpooked();
                }
                return true;
            }
        }
        return false;
    }
    public void PlanScheduledTirednessRecovery(Character character) {
        if (!hasForcedTiredness && tirednessForcedTick != 0 && GameManager.Instance.tick >= tirednessForcedTick && character.doNotDisturb <= 0 && doNotGetTired <= 0) {
            if (!character.jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
                if (isExhausted) {
                    jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                }

                bool triggerSpooked = false;
                Spooked spooked = character.traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerSpooked) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = this });
                    //job.SetCancelOnFail(true);
                    sleepScheduleJobID = job.id;
                    //bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0 /*|| stateComponent.stateToDo != null*/;
                    character.jobQueue.AddJobInQueue(job); //, !willNotProcess
                } else {
                    spooked.TriggerFeelingSpooked();
                }
            }
            hasForcedTiredness = true;
            SetTirednessForcedTick();
        }
        //If a character current sleep ticks is less than the default, this means that the character already started sleeping but was awaken midway that is why he/she did not finish the allotted sleeping time
        //When this happens, make sure to queue tiredness recovery again so he can finish the sleeping time
        else if ((hasCancelledSleepSchedule || currentSleepTicks < CharacterManager.Instance.defaultSleepTicks) && character.doNotDisturb <= 0) {
            if (!character.jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
                if (isExhausted) {
                    jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                }
                bool triggerSpooked = false;
                Spooked spooked = character.traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerSpooked) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //job.SetCancelOnFail(true);
                    sleepScheduleJobID = job.id;
                    //bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0
                    //    || stateComponent.currentState != null || stateComponent.stateToDo != null;
                    character.jobQueue.AddJobInQueue(job); //!willNotProcess
                } else {
                    spooked.TriggerFeelingSpooked();
                }
            }
            SetHasCancelledSleepSchedule(false);
        }
    }
    public void SetHasCancelledSleepSchedule(bool state) {
        hasCancelledSleepSchedule = state;
    }
    public void ResetSleepTicks() {
        currentSleepTicks = CharacterManager.Instance.defaultSleepTicks;
    }
    public void AdjustSleepTicks(int amount) {
        currentSleepTicks += amount;
        if(currentSleepTicks <= 0) {
            this.ResetSleepTicks();
        }
    }
    #endregion

    #region Happiness
    public void ResetHappinessMeter() {
        happiness = HAPPINESS_DEFAULT;
        OnHappinessAdjusted();
    }
    public void AdjustHappiness(int adjustment) {
        happiness += adjustment;
        happiness = Mathf.Clamp(happiness, happinessLowerBound, HAPPINESS_DEFAULT);
        OnHappinessAdjusted();
    }
    public void SetHappiness(int amount) {
        happiness = amount;
        happiness = Mathf.Clamp(happiness, happinessLowerBound, HAPPINESS_DEFAULT);
        OnHappinessAdjusted();
    }
    private void RemoveLonelyOrForlorn() {
        if (_character.traitContainer.RemoveTrait(_character, "Lonely") == false) {
            _character.traitContainer.RemoveTrait(_character, "Forlorn");
        }
    }
    private void OnHappinessAdjusted() {
        if (isForlorn) {
            _character.traitContainer.RemoveTrait(_character, "Lonely");
            _character.traitContainer.AddTrait(_character, "Forlorn");
        } else if (isLonely) {
            _character.traitContainer.RemoveTrait(_character, "Forlorn");
            _character.traitContainer.AddTrait(_character, "Lonely");
        } else {
            RemoveLonelyOrForlorn();
        }
        JobQueueItem suicideJob = _character.jobQueue.GetJob(JOB_TYPE.SUICIDE);
        if (happiness <= 0 && suicideJob == null) {
            //When Happiness meter is reduced to 0, the character will create a Commit Suicide Job.
            Debug.Log(GameManager.Instance.TodayLogString() + _character.name + "'s happiness is " + happiness.ToString() + ", creating suicide job");
            _character.CreateSuicideJob();
        } else if (happiness > HAPPINESS_THRESHOLD_2 && suicideJob != null) {
            Debug.Log(GameManager.Instance.TodayLogString() + _character.name + "'s happiness is " + happiness.ToString() + ", canceling suicide job");
            suicideJob.CancelJob(false, reason: "no longer forlorn");
        }
    }
    public void AdjustDoNotGetLonely(int amount) {
        doNotGetLonely += amount;
        doNotGetLonely = Math.Max(doNotGetLonely, 0);
    }
    public bool PlanHappinessRecoveryActions(Character character) {
        if (character.doNotDisturb > 0 || !character.canWitness) {
            return false;
        }
        if (this.isForlorn) {
            if (!character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem happinessRecoveryJob = character.jobQueue.GetJob(JOB_TYPE.HAPPINESS_RECOVERY);
                if (happinessRecoveryJob != null) {
                    //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                    JobQueueItem currJob = character.currentJob;
                    if (currJob == happinessRecoveryJob) {
                        return false;
                    } else {
                        happinessRecoveryJob.CancelJob();
                    }
                }
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetNormalTrait<Trait>("Heartbroken") as Heartbroken;
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerBrokenhearted) {
                    Hardworking hardworking = character.traitContainer.GetNormalTrait<Trait>("Hardworking") as Hardworking;
                    if (hardworking != null) {
                        bool isPlanningRecoveryProcessed = false;
                        if (hardworking.ProcessHardworkingTrait(character, ref isPlanningRecoveryProcessed)) {
                            return isPlanningRecoveryProcessed;
                        }
                    }
                    JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY_FORLORN;
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //job.SetCancelOnFail(true);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
                return true;
            }
        } else if (this.isLonely) {
            if (!character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY;
                int chance = UnityEngine.Random.Range(0, 100);
                int value = 0;
                TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick(character);
                if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    value = 30;
                } else if (currentTimeInWords == TIME_IN_WORDS.LUNCH_TIME) {
                    value = 45;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    value = 45;
                } else if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
                    value = 45;
                } else if (currentTimeInWords == TIME_IN_WORDS.LATE_NIGHT) {
                    value = 30;
                }
                if (chance < value) {
                    bool triggerBrokenhearted = false;
                    Heartbroken heartbroken = character.traitContainer.GetNormalTrait<Trait>("Heartbroken") as Heartbroken;
                    if (heartbroken != null) {
                        triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                    }
                    if (!triggerBrokenhearted) {
                        Hardworking hardworking = character.traitContainer.GetNormalTrait<Trait>("Hardworking") as Hardworking;
                        if (hardworking != null) {
                            bool isPlanningRecoveryProcessed = false;
                            if (hardworking.ProcessHardworkingTrait(character, ref isPlanningRecoveryProcessed)) {
                                return isPlanningRecoveryProcessed;
                            }
                        }
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                        //job.SetCancelOnFail(true);
                        character.jobQueue.AddJobInQueue(job);
                    } else {
                        heartbroken.TriggerBrokenhearted();
                    }
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Fullness
    public void ResetFullnessMeter() {
        fullness = FULLNESS_DEFAULT;
        RemoveHungryOrStarving();
    }
    public void AdjustFullness(int adjustment) {
        fullness += adjustment;
        fullness = Mathf.Clamp(fullness, fullnessLowerBound, FULLNESS_DEFAULT);
        if(adjustment > 0) {
            _character.HPRecovery(0.02f);
        }
        if (fullness == 0) {
            _character.Death("starvation");
        } else if (isStarving) {
            _character.traitContainer.RemoveTrait(_character, "Hungry");
            if (_character.traitContainer.AddTrait(_character, "Starving") && _character.traitContainer.GetNormalTrait<Trait>("Vampiric") == null) { //only characters that are not vampires will flee when they are starving
                Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, _character, "starving");
            }
        } else if (isHungry) {
            _character.traitContainer.RemoveTrait(_character, "Starving");
            _character.traitContainer.AddTrait(_character, "Hungry");
        } else {
            //fullness is higher than both thresholds
            RemoveHungryOrStarving();
        }
    }
    public void SetFullness(int amount) {
        fullness = amount;
        fullness = Mathf.Clamp(fullness, fullnessLowerBound, FULLNESS_DEFAULT);
        if (fullness == 0) {
            _character.Death("starvation");
        } else if (isStarving) {
            _character.traitContainer.RemoveTrait(_character, "Hungry");
            _character.traitContainer.AddTrait(_character, "Starving");
        } else if (isHungry) {
            _character.traitContainer.RemoveTrait(_character, "Starving");
            _character.traitContainer.AddTrait(_character, "Hungry");
        } else {
            //fullness is higher than both thresholds
            RemoveHungryOrStarving();
        }
    }
    private void RemoveHungryOrStarving() {
        if (_character.traitContainer.RemoveTrait(_character, "Hungry") == false) {
            _character.traitContainer.RemoveTrait(_character, "Starving");
        }
    }
    public void SetFullnessForcedTick() {
        if (!hasForcedFullness) {
            if (forcedFullnessRecoveryTimeInWords == GameManager.GetCurrentTimeInWordsOfTick()) {
                //If the forced recovery job has not been done yet and the character is already on the time of day when it is supposed to be done,
                //the tick that will be assigned will be ensured that the character will not miss it
                //Example if the time of day is Afternoon, the supposed tick range for it is 145 - 204
                //So if the current tick of the game is already in 160, the range must be adjusted to 161 - 204, so as to ensure that the character will hit it
                //But if the current tick of the game is already in 204, it cannot be 204 - 204, so, it will revert back to 145 - 204 
                int newTick = GameManager.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords, GameManager.Instance.tick + 1);
                TIME_IN_WORDS timeInWords = GameManager.GetTimeInWordsOfTick(newTick);
                if (timeInWords != forcedFullnessRecoveryTimeInWords) {
                    newTick = GameManager.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords);
                }
                fullnessForcedTick = newTick;
                return;
            }
        }
        fullnessForcedTick = GameManager.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords);
    }
    public void SetFullnessForcedTick(int tick) {
        fullnessForcedTick = tick;
    }
    public void SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS timeInWords) {
        forcedFullnessRecoveryTimeInWords = timeInWords;
    }
    public void AdjustDoNotGetHungry(int amount) {
        doNotGetHungry += amount;
        doNotGetHungry = Math.Max(doNotGetHungry, 0);
    }
    public void PlanScheduledFullnessRecovery(Character character) {
        if (!hasForcedFullness && fullnessForcedTick != 0 && GameManager.Instance.tick >= fullnessForcedTick && character.doNotDisturb <= 0 && doNotGetHungry <= 0) {
            if (!character.jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
                JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY;
                if (isStarving) {
                    jobType = JOB_TYPE.HUNGER_RECOVERY_STARVING;
                }
                bool triggerGrieving = false;
                Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
                if (griefstricken != null) {
                    triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerGrieving) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                    //}
                    //else if (GetNormalTrait<Trait>("Cannibal") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                    //}
                    //job.SetCancelOnFail(true);
                    //bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0 /*|| stateComponent.stateToDo != null*/;
                    character.jobQueue.AddJobInQueue(job); //, !willNotProcess
                } else {
                    griefstricken.TriggerGrieving();
                }
            }
            hasForcedFullness = true;
            SetFullnessForcedTick();
        }
    }
    public bool PlanFullnessRecoveryActions(Character character) {
        if (character.doNotDisturb > 0 || !character.canWitness) {
            return false;
        }
        if (this.isStarving) {
            if (!character.jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
                //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem hungerRecoveryJob = character.jobQueue.GetJob(JOB_TYPE.HUNGER_RECOVERY);
                if (hungerRecoveryJob != null) {
                    //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                    JobQueueItem currJob = character.currentJob;
                    if (currJob == hungerRecoveryJob) {
                        return false;
                    } else {
                        hungerRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY_STARVING;
                bool triggerGrieving = false;
                Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
                if (griefstricken != null) {
                    triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerGrieving) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                    //}
                    //else if (GetNormalTrait<Trait>("Cannibal") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                    //}
                    //job.SetCancelOnFail(true);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    griefstricken.TriggerGrieving();
                }
                return true;
            }
        } else if (this.isHungry) {
            if (UnityEngine.Random.Range(0, 2) == 0 && character.traitContainer.GetNormalTrait<Trait>("Glutton") != null) {
                if (!character.jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY)) {
                    JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY;
                    bool triggerGrieving = false;
                    Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
                    if (griefstricken != null) {
                        triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
                    }
                    if (!triggerGrieving) {
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                        //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                        //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                        //}
                        //else if (GetNormalTrait<Trait>("Cannibal") != null) {
                        //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                        //}
                        //job.SetCancelOnFail(true);
                        character.jobQueue.AddJobInQueue(job);
                    } else {
                        griefstricken.TriggerGrieving();
                    }
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Events
    public void OnCharacterLeftLocation(ILocation location) {
        if (location == _character.homeRegion) {
            //character left home region
            AdjustDoNotGetHungry(1);
            AdjustDoNotGetLonely(1);
            AdjustDoNotGetTired(1);
        }
    }
    public void OnCharacterArrivedAtLocation(ILocation location) {
        if (location == _character.homeRegion) {
            //character arrived at home region
            AdjustDoNotGetHungry(-1);
            AdjustDoNotGetLonely(-1);
            AdjustDoNotGetTired(-1);
        }
    }
    private void OnCharacterFinishedJob(Character character, GoapPlanJob job) {
        if (_character == character) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{character.name} has finished job {job.ToString()}");
            //after doing an extreme needs type job, check again if the character needs to recover more of that need.
            if (job.jobType == JOB_TYPE.HAPPINESS_RECOVERY_FORLORN) {
                PlanHappinessRecoveryActions(_character);
            } else if (job.jobType == JOB_TYPE.HUNGER_RECOVERY_STARVING) {
                PlanFullnessRecoveryActions(_character);
            } else if (job.jobType == JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED) {
                PlanTirednessRecoveryActions(_character);
            }
        }
    }
    /// <summary>
    /// Make this character plan a starving fullness recovery job, regardless of actual
    /// fullness level. NOTE: This will also cancel any existing fullness recovery jobs
    /// </summary>
    public void TriggerFlawFullnessRecovery(Character character) {
        //if (jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
        //    jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING);
        //}
        bool triggerGrieving = false;
        Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
        if (griefstricken != null) {
            triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
        }
        if (!triggerGrieving) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
            //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
            //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
            //}
            //else if (GetNormalTrait<Trait>("Cannibal") != null) {
            //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
            //}
            character.jobQueue.AddJobInQueue(job);
        } else {
            griefstricken.TriggerGrieving();
        }
    }
    #endregion
}
