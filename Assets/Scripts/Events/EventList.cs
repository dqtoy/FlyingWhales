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

//Campaign
public class RegisterOnCampaign: UnityEvent<Campaign>{}
public class DeathArmy: UnityEvent{}
public class UnsupportCitizen: UnityEvent<Citizen>{}
public class RemoveSuccessionWarCity: UnityEvent<City>{}
public class LookForLostArmies: UnityEvent<General>{}
public class DeathToGhost: UnityEvent<City>{}
public class CheckGeneralEligibility: UnityEvent<Citizen, HexTile>{}


//Game Events
public class GameEventAction: UnityEvent<GameEvent, int>{}
public class GameEventEnded: UnityEvent<GameEvent>{}

//UI
public class UpdateUI: UnityEvent{}

//Combat
public class UpdatePath: UnityEvent<HexTile>{}