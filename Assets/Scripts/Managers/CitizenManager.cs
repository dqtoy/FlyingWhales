using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CitizenManager : MonoBehaviour {

    public static CitizenManager Instance = null;

    private Dictionary<MONTH, Dictionary<int, HashSet<Citizen>>> citizenBirthdays;
    private HashSet<Citizen> allCitizens;

    private void Awake() {
        Instance = this;
        allCitizens = new HashSet<Citizen>();
        citizenBirthdays = new Dictionary<MONTH, Dictionary<int, HashSet<Citizen>>>();
        Messenger.AddListener("OnDayEnd", AgeCitizens);
    }

    internal void RegisterCitizen(Citizen citizen) {
        allCitizens.Add(citizen);
        if (!citizenBirthdays.ContainsKey(citizen.birthMonth)) {
            citizenBirthdays.Add(citizen.birthMonth, new Dictionary<int, HashSet<Citizen>>());
        }
        Dictionary<int, HashSet<Citizen>> currMonthDict = citizenBirthdays[citizen.birthMonth];

        if (!currMonthDict.ContainsKey(citizen.birthDay)) {
            currMonthDict.Add(citizen.birthDay, new HashSet<Citizen>());
        }

        HashSet<Citizen> citizensWithBday = currMonthDict[citizen.birthDay];
        citizensWithBday.Add(citizen);
    }

    internal void UnregisterCitizen(Citizen citizen) {
        allCitizens.Remove(citizen);
        HashSet<Citizen> citizensWithBday = citizenBirthdays[citizen.birthMonth][citizen.birthDay];
        citizensWithBday.Remove(citizen);
        if(citizensWithBday.Count <= 0) {
            citizenBirthdays[citizen.birthMonth].Remove(citizen.birthDay);
        }

    }

    protected void AgeCitizens() {
        MONTH currMonth = (MONTH)GameManager.Instance.month;
        int currDay = GameManager.Instance.days;
        if (citizenBirthdays.ContainsKey(currMonth)) {
            if (citizenBirthdays[currMonth].ContainsKey(currDay)) {
                HashSet<Citizen> citizensToAge = citizenBirthdays[currMonth][currDay];
                for (int i = 0; i < citizensToAge.Count; i++) {
                    citizensToAge.ElementAt(i).AdjustAge(1);
                }
            }
        }
    }



}
