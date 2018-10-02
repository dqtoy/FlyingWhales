using ECS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CharacterSchedule {
    public SCHEDULE_ACTION_CATEGORY[] phases; //only up to 5


    //public List<CharacterScheduleTemplatePhase> phases; //NOTE: This must be setup in order! 

    public Character owner { get; private set; }
    public SCHEDULE_ACTION_CATEGORY currentPhase { get { return phases[currentPhaseIndex]; } }
    public SCHEDULE_ACTION_CATEGORY previousPhase {
        get {
            if (previousPhaseIndex == -1) {
                return SCHEDULE_ACTION_CATEGORY.NONE;
            }
            return phases[previousPhaseIndex];
        }
    }
    private int currentPhaseIndex;
    private int previousPhaseIndex;
    //private CharacterScheduleTemplatePhase nextPhase { get { return GetNextPhase(); } }

    public CharacterSchedule(CharacterScheduleTemplate template, Character owner) {
        this.owner = owner;
        ConstructSchedule(template);
    }

    #region Schedule Construction
    private void ConstructSchedule(CharacterScheduleTemplate template) {
        phases = new SCHEDULE_ACTION_CATEGORY[CharacterScheduleManager.MAX_DAYS_IN_SCHEDULE];

        List<CharacterScheduleTemplatePhase> templatePhases = new List<CharacterScheduleTemplatePhase>(template.phases);
        List<CharacterScheduleTemplatePhase> consecutivePhases = templatePhases.Where(x => x.mustBeConsecutive).OrderByDescending(x => x.days).ToList();
        List<CharacterScheduleTemplatePhase> otherPhases = templatePhases.Where(x => !x.mustBeConsecutive).ToList();
        for (int i = 0; i < consecutivePhases.Count; i++) {
            CharacterScheduleTemplatePhase currTemplatePhase = consecutivePhases[i];
            List<int> freeSlots = GetFreeSlots();
            List<int> validSlots = new List<int>();

            for (int j = 0; j < freeSlots.Count; j++) {
                int freeSlot = freeSlots[j];
                bool isSlotValid = true;
                SCHEDULE_ACTION_CATEGORY[] testSchedule = Utilities.CreateCopyOfArray(phases);
                if (CanFitPhase(testSchedule, freeSlot, currTemplatePhase)) {
                    PlacePhase(testSchedule, freeSlot, currTemplatePhase);
                    for (int k = 0; k < consecutivePhases.Count; k++) {
                        CharacterScheduleTemplatePhase otherPhase = consecutivePhases[k]; //check all other remaining consecutive phases
                        if (otherPhase != currTemplatePhase) {
                            //if at least one of them cannot fit in the test schedule, then the free slot is invalid!
                            if (!HasSlotForConsecutive(testSchedule, otherPhase.days)) {
                                isSlotValid = false;
                                break;
                            }
                        }
                    }
                } else {
                    isSlotValid = false;
                }
                if (isSlotValid) {
                    validSlots.Add(freeSlot);
                }
            }
            if (validSlots.Count > 0) {
                int chosenSlot = validSlots[Random.Range(0, validSlots.Count)];
                PlacePhase(phases, chosenSlot, currTemplatePhase);
            } else {
                throw new System.Exception("There is no valid slot for consecutive schedule!");
            }
        }

        for (int i = 0; i < otherPhases.Count; i++) {
            CharacterScheduleTemplatePhase currTemplatePhase = otherPhases[i];
            for (int j = 0; j < currTemplatePhase.days; j++) {
                List<int> freeSlots = GetFreeSlots();
                if (freeSlots.Count > 0) {
                    int chosenSlot = freeSlots[Random.Range(0, freeSlots.Count)];
                    PlacePhase(phases, chosenSlot, currTemplatePhase.category);
                } else {
                    throw new System.Exception("Random schedule construction problem at other phases!");
                }
            }
        }
        string phaseSummary = string.Empty;
        for (int i = 0; i < phases.Length; i++) {
            phaseSummary += "[" + phases[i].ToString() + "]";
        }
        Debug.Log(this.owner.name + "'s schedule has been constructed - " + phaseSummary);
    }
    private bool CanFitPhase(SCHEDULE_ACTION_CATEGORY[] schedule, int startIndex, CharacterScheduleTemplatePhase templatePhase) {
        if ((schedule.Length - startIndex) < templatePhase.days) {
            return false;
        }
        return true;
    }
    private void PlacePhase(SCHEDULE_ACTION_CATEGORY[] schedule, int startIndex, CharacterScheduleTemplatePhase templatePhase) {
        for (int i = startIndex; i < startIndex + templatePhase.days; i++) {
            PlacePhase(schedule, i, templatePhase.category);
        }
    }
    private void PlacePhase(SCHEDULE_ACTION_CATEGORY[] schedule, int index, SCHEDULE_ACTION_CATEGORY category) {
        schedule[index] = category;
    }
    private List<int> GetFreeSlots() {
        List<int> freeSlots = new List<int>();
        for (int i = 0; i < phases.Length; i++) {
            if (phases[i] == SCHEDULE_ACTION_CATEGORY.NONE) {
                freeSlots.Add(i);
            }
        }
        return freeSlots;
    }
    private bool HasSlotForConsecutive(SCHEDULE_ACTION_CATEGORY[] phases, int range) {
        for (int i = 0; i < phases.Length; i++) {
            SCHEDULE_ACTION_CATEGORY currSlot = phases[i];
            bool isSlotValid = true;
            if (currSlot == SCHEDULE_ACTION_CATEGORY.NONE) {
                for (int j = 1; j < range; j++) {
                    int nextIndex = i+j;
                    if (nextIndex < phases.Length) {
                        if (phases[nextIndex] != SCHEDULE_ACTION_CATEGORY.NONE) {
                            //the phase slot is not empty!
                            isSlotValid = false;
                            break;
                        }
                    }
                }
            } else {
                isSlotValid = false;
            }

            if (isSlotValid) {
                return true;
            } else {
                continue; //next slot
            }
        }
        return false;
    }
    #endregion

    public void Initialize() {
        currentPhaseIndex = 0;
        previousPhaseIndex = -1;
        owner.OnSchedulePhaseStarted(currentPhase);
        Messenger.AddListener(Signals.DAY_START, OnDayStarted);
    }

    private void OnDayStarted() {
        //move to next phase
        previousPhaseIndex = currentPhaseIndex;
        currentPhaseIndex = GetNextPhaseIndex();
        owner.OnSchedulePhaseStarted(currentPhase);
    }

    private int GetNextPhaseIndex() {
        if (currentPhaseIndex + 1 >= phases.Length) {
            return 0;
        } else {
            return currentPhaseIndex + 1;
        }
    }
    //public CharacterSchedule Clone() {
    //    CharacterSchedule clone = new CharacterSchedule();
    //    //for (int i = 0; i < this.phases.Count; i++) {
    //    //    clone.phases.Add(this.phases[i].Clone()); //create a new clone of each phase, and add it to the phases list of the CharacterSchedule clone
    //    //}
    //    return clone;
    //}

    ///*
    // This is called when a schedule is assigned to a character.
    //     */
    //public void Initialize(Character character) {
    //    owner = character;
    //    ValidateSchedule();
    //    Messenger.AddListener(Signals.HOUR_STARTED, WaitForPhaseStart);
    //    Messenger.AddListener(Signals.HOUR_ENDED, WaitForPhaseEnd);
    //    StartNextPhase(phases[0]);
    //}

    //private void ValidateSchedule() {
    //    int lastStartTick = 0;
    //    for (int i = 0; i < phases.Count; i++) {
    //        CharacterScheduleTemplatePhase currentPhase = phases[i];
    //        //if (lastStartTick == 0) {
    //        //    lastStartTick = currentPhase.startTick;
    //        //} else {
    //        //    if (lastStartTick > currentPhase.startTick) {
    //        //        throw new System.Exception("There was an error while trying to validate the daily scedule of " + owner.characterClass.className + " " + owner.name + ". Please check schedule for any inconsistencies.");
    //        //    }
    //        //}
    //    }
    //}
    //private void SetCurrentPhase(CharacterScheduleTemplatePhase phase) {
    //    currentPhase = phase;
    //}
    //private void StartNextPhase(CharacterScheduleTemplatePhase nextPhase) {
    //    SetCurrentPhase(nextPhase);
    //    owner.OnDailySchedulePhaseStarted(nextPhase);
    //    //this.nextPhase = GetNextPhase(); //set next phase to the next phase
    //    //TODO: Change this to use scheduling instead, when the scheduling manager has been converted to handle functions at start of ticks too
    //    //if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
    //    //    Messenger.RemoveListener(Signals.HOUR_STARTED, WaitForPhaseStart);
    //    //}
    //    //Messenger.AddListener(Signals.HOUR_ENDED, WaitForPhaseEnd);
    //    //GameDate phaseEnd = GameManager.Instance.Today();
    //    //phaseEnd.AddHours(currentPhase.phaseLength);
    //    //SchedulingManager.Instance.AddEntry(phaseEnd, () => EndCurrentPhase());
    //}
    //private void WaitForPhaseEnd() {
    //    //if (GameManager.Instance.Today().hour == Mathf.Clamp(nextPhase.startTick - 1, 1, GameManager.hoursPerDay)) {
    //    //    EndCurrentPhase();
    //    //}
    //}
    //private void EndCurrentPhase() {
    //    owner.OnDailySchedulePhaseEnded(currentPhase);
    //    //TODO: Change this to use scheduling instead, when the scheduling manager has been converted to handle functions at start of ticks too
    //    //if (Messenger.eventTable.ContainsKey(Signals.HOUR_ENDED)) {
    //    //    Messenger.RemoveListener(Signals.HOUR_ENDED, WaitForPhaseEnd);
    //    //}
    //    //Messenger.AddListener(Signals.HOUR_STARTED, WaitForPhaseStart);
    //    //GameDate nextPhaseStart = GameManager.Instance.Today();
    //    //nextPhaseStart.AddHours(1);
    //    //SchedulingManager.Instance.AddEntry(nextPhaseStart, () => StartNextPhase(GetNextPhase()));
    //}
    //private void WaitForPhaseStart() {
    //    //if (GameManager.Instance.Today().hour == nextPhase.startTick) {
    //    //    StartNextPhase(nextPhase);
    //    //}
    //}
    //private CharacterScheduleTemplatePhase GetNextPhase() {
    //    int currentPhaseIndex = phases.IndexOf(currentPhase);
    //    if (phases.Count-1 == currentPhaseIndex) {
    //        //current phase is last phase of the day, return the first phase
    //        return phases[0];
    //    } else {
    //        return phases[currentPhaseIndex + 1];
    //    }
    //}

    public void OnOwnerDied() {
        Messenger.RemoveListener(Signals.DAY_START, OnDayStarted);
    }
}
