using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSchedule {
    public List<CharacterSchedulePhase> phases; //NOTE: This must be setup in order! 

    public Character owner { get; private set; }
    public CharacterSchedulePhase currentPhase { get; private set; }
    private CharacterSchedulePhase nextPhase { get { return GetNextPhase(); } }

    public CharacterSchedule() {
        phases = new List<CharacterSchedulePhase>();
    }

    public CharacterSchedule Clone() {
        CharacterSchedule clone = new CharacterSchedule();
        for (int i = 0; i < this.phases.Count; i++) {
            clone.phases.Add(this.phases[i].Clone()); //create a new clone of each phase, and add it to the phases list of the CharacterSchedule clone
        }
        return clone;
    }

    /*
     This is called when a schedule is assigned to a character.
         */
    public void Initialize(Character character) {
        owner = character;
        Messenger.AddListener(Signals.HOUR_STARTED, WaitForPhaseStart);
        Messenger.AddListener(Signals.HOUR_ENDED, WaitForPhaseEnd);
        StartNextPhase(phases[0]);
    }

    private void SetCurrentPhase(CharacterSchedulePhase phase) {
        currentPhase = phase;
    }
    
    private void StartNextPhase(CharacterSchedulePhase nextPhase) {
        SetCurrentPhase(nextPhase);
        owner.OnDailySchedulePhaseStarted(nextPhase);
        //this.nextPhase = GetNextPhase(); //set next phase to the next phase
        //TODO: Change this to use scheduling instead, when the scheduling manager has been converted to handle functions at start of ticks too
        //if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
        //    Messenger.RemoveListener(Signals.HOUR_STARTED, WaitForPhaseStart);
        //}
        //Messenger.AddListener(Signals.HOUR_ENDED, WaitForPhaseEnd);
        //GameDate phaseEnd = GameManager.Instance.Today();
        //phaseEnd.AddHours(currentPhase.phaseLength);
        //SchedulingManager.Instance.AddEntry(phaseEnd, () => EndCurrentPhase());
    }

    private void WaitForPhaseEnd() {
        if (GameManager.Instance.Today().hour == Mathf.Clamp(nextPhase.startTick - 1, 1, GameManager.hoursPerDay)) {
            EndCurrentPhase();
        }
    }
    private void EndCurrentPhase() {
        owner.OnDailySchedulePhaseEnded(currentPhase);
        //TODO: Change this to use scheduling instead, when the scheduling manager has been converted to handle functions at start of ticks too
        //if (Messenger.eventTable.ContainsKey(Signals.HOUR_ENDED)) {
        //    Messenger.RemoveListener(Signals.HOUR_ENDED, WaitForPhaseEnd);
        //}
        //Messenger.AddListener(Signals.HOUR_STARTED, WaitForPhaseStart);
        //GameDate nextPhaseStart = GameManager.Instance.Today();
        //nextPhaseStart.AddHours(1);
        //SchedulingManager.Instance.AddEntry(nextPhaseStart, () => StartNextPhase(GetNextPhase()));
    }

    private void WaitForPhaseStart() {
        if (GameManager.Instance.Today().hour == nextPhase.startTick) {
            StartNextPhase(nextPhase);
        }
    }

    private CharacterSchedulePhase GetNextPhase() {
        int currentPhaseIndex = phases.IndexOf(currentPhase);
        if (phases.Count-1 == currentPhaseIndex) {
            //current phase is last phase of the day, return the first phase
            return phases[0];
        } else {
            return phases[currentPhaseIndex + 1];
        }
    }
}
