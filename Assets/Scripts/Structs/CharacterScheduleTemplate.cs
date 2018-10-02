using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScheduleTemplate : MonoBehaviour {
    //public CharacterSchedule schedule;
    public List<CharacterScheduleTemplatePhase> phases;


    public bool IsValid() {
        int totalDays = 0;
        for (int i = 0; i < phases.Count; i++) {
            CharacterScheduleTemplatePhase phase = phases[i];
            totalDays += phase.days;
        }
        return totalDays == CharacterScheduleManager.MAX_DAYS_IN_SCHEDULE;
    }


}
