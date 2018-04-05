using UnityEngine;
using System.Collections;

public static class Signals {

    public static string OBTAIN_ITEM = "OnObtainItem"; //Parameters (Character characterThatObtainedItem, Item obtainedItem)
    public static string DAY_END = "OnDayEnd";
    public static string DAY_START = "OnDayStart";
    public static string FOUND_ITEM = "OnItemFound"; //Parameters (Character characterThatFoundItem, Item foundItem)
    public static string FOUND_TRACE = "OnTraceFound"; //Parameters (Character characterThatFoundTrace, string traceFound)
    public static string TASK_SUCCESS = "OnTaskSuccess"; //Parameters (Character characterThatFinishedTask, CharacterTask succeededTask)
    public static string ITEM_PLACED_LANDMARK = "OnItemPlacedAtLandmark"; //Parameters (Item item, BaseLandmark landmark)
    public static string ITEM_PLACED_INVENTORY = "OnItemPlacedAtInventory"; //Parameters (Item item, Character character)
    public static string CHARACTER_DEATH = "OnCharacterDied"; //Parameters (Character characterThatDied)
    public static string CHARACTER_KILLED = "OnCharacterKilled"; //Parameters (ICombatInitializer killer, Character characterThatDied)
    public static string COLLIDED_WITH_CHARACTER = "OnCollideWithCharacter"; //Parameters (ICombatInitializer character1, ICombatInitializer character2)
}
