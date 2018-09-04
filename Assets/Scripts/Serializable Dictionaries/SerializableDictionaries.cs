using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScheduleTemplateDictionary : SerializableDictionary<CHARACTER_ROLE, CharacterScheduleTemplate> { }

[System.Serializable]
public class ActionCharacterTagListDictionary : SerializableDictionary<ACTION_TYPE, List<CharacterActionTagRequirement>, CharacterTagListStorage> { }


[System.Serializable]
public class CharacterTagListStorage : SerializableDictionary.Storage<List<CharacterActionTagRequirement>> { }