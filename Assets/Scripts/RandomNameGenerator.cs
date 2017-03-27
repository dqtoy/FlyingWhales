using UnityEngine;
using System.Collections;

public class RandomNameGenerator {

	static string[] firstNames = new string[]{"Marvin", "Michael", "Kristoffer", "Janice", "Daenerys", "Jon", "Jim", "Scott", "Pam", "Lee Jae", "Royalty", "Park Hae", "Canada", "Aloy", "Vala", "Christine", "Lucas", "Zoe", "Drake", "Daddy", "Rap", "JRC", "Francis", "Rost", "Eddie", "Ki Tae", "Arrey"};
	static string[] prefixes = new string[]{"Wild", "Strong", "Wimpy", "Pretty", "Black", "White", "Crimson", "Bloody"};
	static string[] suffixes = new string[]{"of the Mountains", "of the Sun", ", the Rabid", ", the Drowned", ", the Beauty", "Bloodborn", ", the Chosen"};

	public static string GenerateRandomName(){
		string firstName = firstNames [Random.Range (0, firstNames.Length)];
		string prefix = prefixes [Random.Range (0, prefixes.Length)];
		string suffix = suffixes [Random.Range (0, suffixes.Length)];

		int choice = Random.Range (0, 100);

		if (choice >= 0 && choice < 25) {
			return prefix + " " + firstName;
		} else {
			return firstName + " " + suffix;
		}

		return " ";
	}
}
