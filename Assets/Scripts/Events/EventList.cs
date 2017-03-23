using UnityEngine.Events;

public class NewKingdomEvent : UnityEvent<Kingdom>{}
public class CitizenTurnActions: UnityEvent{}
public class CityEverydayTurnActions: UnityEvent{}
public class MassChangeSupportedCitizen: UnityEvent<Citizen, Citizen>{}

