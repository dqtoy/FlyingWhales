using UnityEngine;
using System.Collections;

public class Spouse : Citizen {

	internal int _marriageCompatibility;
	internal bool isAbducted;

    public Spouse(Citizen citizenToMarry, City city, int age, GENDER gender, int generation) : base(city, age, gender, generation) {
        this._marriageCompatibility = GenerateMarriageCompatibility();
		this.isAbducted = false;
    }

    internal int GenerateMarriageCompatibility() {
        return Random.Range(-100, 101);
    }
}
