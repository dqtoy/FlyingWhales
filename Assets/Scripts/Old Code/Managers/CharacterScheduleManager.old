
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScheduleManager : MonoBehaviour {

    public static CharacterScheduleManager Instance = null;

    public static int MAX_DAYS_IN_SCHEDULE = 5;

    [SerializeField] private ScheduleTemplateDictionary scheduleTemplates;

    private void Awake() {
        Instance = this;
    }

    public void Initialize() {
        ValidateSchedules();
    }

    private void ValidateSchedules() {
        foreach (KeyValuePair<string, CharacterScheduleTemplate> kvp in scheduleTemplates) {
            if (!kvp.Value.IsValid()) {
                throw new System.Exception("Schedule template for " + kvp.Key + " is invalid!");
            }
        }
    }

    public CharacterSchedule GetScheduleForCharacter(Character character) {
        CharacterScheduleTemplate template = GetScheduleTemplate(character.characterClass.className); //get template for role
        if (template != null) {
            return new CharacterSchedule(template, character);
        }
        return null;
    }

    private CharacterScheduleTemplate GetScheduleTemplate(string className) {
        if (scheduleTemplates.ContainsKey(className)) {
            return scheduleTemplates[className];
        }
        return null; //change to error message, when all roles are expected to have a schedule
        //throw new System.Exception("There is no schedule template for " + className.ToString());
    }


}
