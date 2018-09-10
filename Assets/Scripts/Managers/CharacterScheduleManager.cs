using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScheduleManager : MonoBehaviour {

    public static CharacterScheduleManager Instance = null;

    [SerializeField] private ScheduleTemplateDictionary scheduleTemplates;

    private void Awake() {
        Instance = this;
    }

    public CharacterSchedule GetScheduleForCharacter(Character character) {
        CharacterScheduleTemplate template = GetScheduleTemplate(character.characterClass.className); //get template for role
        if (template != null) {
            return template.schedule.Clone();
        }
        return null;
    }

    private CharacterScheduleTemplate GetScheduleTemplate(string className) {
        if (scheduleTemplates.ContainsKey(className)) {
            return scheduleTemplates[className];
        }
        //return null; //change to error message, when all roles are expected to have a schedule
        throw new System.Exception("There is no schedule template for " + className.ToString());
    }


}
