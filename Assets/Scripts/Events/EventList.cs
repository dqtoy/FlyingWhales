using UnityEngine.Events;

//Generic Events
public class WeekEndedEvent : UnityEvent{}

//Kingdom Events
public class NewKingdomEvent : UnityEvent<Kingdom>{}

//Citizen Events
public class CitizenTurnActions: UnityEvent{}
public class CitizenDiedEvent : UnityEvent{}

//City Events
public class CityEverydayTurnActions: UnityEvent{}
public class CitizenMove: UnityEvent{}

//Campaign
public class RegisterOnCampaign: UnityEvent<Campaign>{}
public class DeathArmy: UnityEvent{}
public class UnsupportCitizen: UnityEvent<Citizen>{}

//Game Events
public class GameEventAction: UnityEvent<GameEvent, int>{}


