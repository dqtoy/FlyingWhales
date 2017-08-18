using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CitizenManager : MonoBehaviour {

    public static CitizenManager Instance = null;

    private Dictionary<MONTH, Dictionary<int, HashSet<Citizen>>> citizenBirthdays;
    private Dictionary<int, HashSet<Citizen>> citizenAgeTable = new Dictionary<int, HashSet<Citizen>>() {
        {9, new HashSet<Citizen>()},
        {19, new HashSet<Citizen>()},
        {29, new HashSet<Citizen>()},
        {39, new HashSet<Citizen>()},
        {49, new HashSet<Citizen>()},
        {59, new HashSet<Citizen>()},
        {69, new HashSet<Citizen>()},
        {79, new HashSet<Citizen>()},
        {89, new HashSet<Citizen>()},
        {99, new HashSet<Citizen>()},
        {100, new HashSet<Citizen>()}
    };

    private Dictionary<int, int> ageRangeDeathChances = new Dictionary<int, int>() {
        {9, 5},
        {19, 1},
        {29, 15},
        {39, 20},
        {49, 51},
        {59, 100},
        {69, 320},
        {79, 734},
        {89, 1728},
        {99, 4000},
        {100, 8000},
    };

    private HashSet<Citizen> allCitizens;

    #region getters/setters
    public Dictionary<int, HashSet<Citizen>> elligibleCitizenAgeTable {
        get { return citizenAgeTable.Where(x => x.Value.Count() > 0).ToDictionary(x => x.Key, v => v.Value); }
    }
    #endregion

    private void Awake() {
        Instance = this;
        allCitizens = new HashSet<Citizen>();
        citizenBirthdays = new Dictionary<MONTH, Dictionary<int, HashSet<Citizen>>>();
        Messenger.AddListener("OnDayEnd", AgeCitizens);
        Messenger.AddListener("OnDayEnd", CheckForDeath);
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

        //Add Citizen to age table
        AddCitizenToAgeTable(citizen);
    }

    internal void UnregisterCitizen(Citizen citizen) {
        allCitizens.Remove(citizen);
        
        if (citizenBirthdays.ContainsKey(citizen.birthMonth)) {
            if (citizenBirthdays[citizen.birthMonth].ContainsKey(citizen.birthDay)) {
                HashSet<Citizen> citizensWithBday = citizenBirthdays[citizen.birthMonth][citizen.birthDay];
                citizensWithBday.Remove(citizen);
                if (citizensWithBday.Count <= 0) {
                    citizenBirthdays[citizen.birthMonth].Remove(citizen.birthDay);
                }
            }
        }
        //Remove Citizen from age table
        RemoveCitizenFromAgeTable(citizen);
    }

    protected void AgeCitizens() {
        MONTH currMonth = (MONTH)GameManager.Instance.month;
        int currDay = GameManager.Instance.days;
        if (citizenBirthdays.ContainsKey(currMonth)) {
            if (citizenBirthdays[currMonth].ContainsKey(currDay)) {
                HashSet<Citizen> citizensToAge = citizenBirthdays[currMonth][currDay];
                for (int i = 0; i < citizensToAge.Count; i++) {
                    Citizen currCitizen = citizensToAge.ElementAt(i);
                    currCitizen.AdjustAge(1);
                    RemoveCitizenFromAgeTable(currCitizen);
                    AddCitizenToAgeTable(currCitizen);
                }
            }
        }
    }

    private void AddCitizenToAgeTable(Citizen citizen) {
        if(citizen.age >= 100) {
            citizenAgeTable[100].Add(citizen);
            citizen.SetAgeTableKey(100);
        } else {
            for (int i = 0; i < citizenAgeTable.Keys.Count; i++) {
                int currKey = citizenAgeTable.Keys.ElementAt(i);
                if(citizen.age <= currKey) {
                    citizenAgeTable[currKey].Add(citizen);
                    citizen.SetAgeTableKey(currKey);
                    break;
                }
            }
        }
    }

    private void RemoveCitizenFromAgeTable(Citizen citizen) {
        if(citizen.ageTableKey != -1) {
            citizenAgeTable[citizen.ageTableKey].Remove(citizen);
            citizen.SetAgeTableKey(-1);
        }
    }

    #region Grim Reaper
    private void CheckForDeath() {
        int numOfRolls = allCitizens.Count / 10;
        for (int i = 0; i < numOfRolls; i++) {
            //a roll has a 15 out of 10000 chance to trigger a death
            if (Random.Range(0, 10000) < 15) {
                //Death Triggered
                HashSet<Citizen> citizensToChooseFrom = GetCitizensToDieToChooseFrom();
                if(citizensToChooseFrom.Count > 0) {
                    Citizen citizenToDie = citizensToChooseFrom.ElementAt(Random.Range(0, citizensToChooseFrom.Count));
                    citizenToDie.Death(DEATH_REASONS.ACCIDENT);
                }
            }
        }
    }

    private HashSet<Citizen> GetCitizensToDieToChooseFrom() {
        int[] elligibleKeys = elligibleCitizenAgeTable.Keys.ToArray();
        int totalChance = ageRangeDeathChances.Where(x => elligibleKeys.Contains(x.Key)).Sum(x => x.Value);
        int chance = Random.Range(0, totalChance);
        int upperBound = 0;
        int lowerBound = 0;
        for (int j = 0; j < elligibleKeys.Length; j++) {
            int currKey = elligibleKeys[j];
            upperBound += ageRangeDeathChances[currKey];
            if (chance >= lowerBound && chance < upperBound) {
                return elligibleCitizenAgeTable[currKey];
            }
        }
        return null;
    }
    #endregion

}
