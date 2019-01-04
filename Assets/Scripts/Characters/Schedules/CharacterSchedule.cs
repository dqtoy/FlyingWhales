
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
        //ConstructSchedule(template);
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
        //if (owner.homeLandmark != null) {
        //    owner.party.actionData.ForceDoAction(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.REST), owner.homeLandmark.landmarkObj);
        //}
        //Messenger.AddListener(Signals.DAY_START, OnDayStarted);
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

    public void OnOwnerDied() {
        Messenger.RemoveListener(Signals.MONTH_START, OnDayStarted);
    }
}
