using UnityEngine.Events;

//Kingdom Events
public class NewKingdomEvent : UnityEvent<Kingdom>{}

//Citizen Events
public class CitizenTurnActions: UnityEvent{}
public class CitizenDiedEvent : UnityEvent{}

//City Events
public class CityEverydayTurnActions: UnityEvent{}
public class MassChangeSupportedCitizen: UnityEvent<Citizen, Citizen>{}

