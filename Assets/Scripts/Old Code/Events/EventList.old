using UnityEngine.Events;

//Generic Events
public class WeekEndedEvent : UnityEvent{}

//Kingdom Events
public class NewKingdomEvent : UnityEvent<Kingdom>{}
public class KingdomDiedEvent : UnityEvent<Kingdom>{}

//Citizen Events
public class CitizenTurnActions: UnityEvent{}
public class CitizenDiedEvent : UnityEvent{}

//City Events
public class CityEverydayTurnActions: UnityEvent{}
public class CitizenMove: UnityEvent<bool>{}

//Game Events
public class GameEventAction: UnityEvent<GameEvent, int>{}
public class GameEventEnded: UnityEvent<GameEvent>{}

//UI
public class UpdateUI: UnityEvent{}

//Combat
public class UpdatePath: UnityEvent<HexTile>{}