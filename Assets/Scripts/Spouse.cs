using UnityEngine;
using System.Collections;

public class Spouse : Citizen {

	internal int _marriageCompatibility;

    public Spouse(Citizen citizenToMarry, City city, int age, GENDER gender, int generation) : base(city, age, gender, generation) {
        this._marriageCompatibility = GenerateMarriageCompatibility();
    }

    private int GenerateMarriageCompatibility() {
        return Random.Range(-100, 101);
    }
}
