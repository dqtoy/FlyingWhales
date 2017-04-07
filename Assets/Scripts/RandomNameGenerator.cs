using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomNameGenerator : MonoBehaviour {

	public static RandomNameGenerator Instance = null;

	private string[] baseHumanSurnames = new string[]{
		"Adams", "Atlee", "Anderson", "Baker", "Beauchamp", "Ballard", "Bainard", "Barnes", "Bardolf", "Bell", "Bennet", "Brooker", "Castillion", "Carpenter", "Clarke",
		"Corbon", "Colleville", "Cooper", "Dering", "Digby", "Duhamel", "Durandal", "Edgar", "Emory", "Esteney", "Fossard", "Fysher", "Fletcher", "Garin", "Gaveston",
		"Godfrey", "Gyfford", "Gregory", "Hamond", "Harcourt", "Hughes", "Kent", "Knighton", "Lovell", "Mannering", "Manston", "Mallory", "Madley", "Middleton", "Nelond",
		"Noyers", "Orlebar", "Osmont", "Payne", "Pennant", "Pratt", "Picard", "Raleigh", "Ratcliff", "Renold", "Rolfe", "Savill", "Senarponte", "Sutcliffe", "Talbot",
		"Thornton", "Thibault", "Walter", "Weston", "Williams", "Webb", "Whyte", "Yate", "Vernon", "Villon"
	};

	private string[] baseHumanKingdomNames = new string[]{
		"Atlantis", "Albane", "Alamid", "Andreland", "Aurelia", "Ashanti", "Bactria", "Benin", "Burgundy", "Calabon", "Chyland", "Croatia", "Ceres", "Darmid", "Dirkland",
		"Duscany", "Elarus", "Esmeris", "Erebor", "Fernica", "Fuschia", "Friedland", "Garwinia", "Grazil", "Gundrak", "Hacatid", "Hermani", "Howland", "Ianland", "Iridia",
		"Israel", "Jamalaya", "Jinni", "Justinia", "Karamba", "Kaedwen", "Korolus", "Lindria", "Lunestra", "Lustland", "Lyrica", "Makuria", "Meridian", "Mykland", "Morland",
		"Muskovich", "Nestori", "Norsica", "Numbland", "Obelin", "Ovid", "Opera", "Patani", "Petersen", "Prissia", "Qualia", "Quenden", "Rastafar", "Remedi", "Rustland", 
		"Scythra", "Shetland", "Sundria", "Silveria", "Thenid", "Tilbany", "Tulisia", "Umbra", "Ulbany", "Uruk", "Virindor", "Vanad", "Zulu", "Zakrand"
	};

	private string[] humanMaleFirstNames = new string[]{
		"Aldred", "Alistair", "Arthur", "Ashton", "Atkins", "Barric", "Bentley", "Blythe", "Braden", "Byram", "Caldwell", "Carlisle", "Clifton", "Colton", "Cuthbert",
		"Dalton", "Darren", "Dawson", "Denver", "Dudley", "Easton", "Edgar", "Elton", "Erwan", "Franklin", "Garett", "Gerard", "Gordon", "Hammond", "Holden", "Howard",
		"Hyde", "Irving", "Ian", "Jamie", "Jeremy", "Jesus", "John", "Keaton", "Kirby", "Kipling", "Lander", "Landon", "Leland", "Macon", "Moses", "Matthew", "Maxwell",
		"Maven", "Niles", "Nigel", "Nestor", "Oakley", "Oliver", "Oswald", "Payton", "Parker", "Preston", "Quentin", "Ramsey", "Randall", "Rhett", "River", "Robert",
		"Sawyer", "Shelton", "Silas", "Spencer", "Stewart", "Tanner", "Tim", "Terrence", "Ulmer", "Ulric", "Uther", "Wallace", "Wesley", "William", "Wolfe", "Yates"
	};

	private string[] humanFemaleFirstNames = new string[]{
		"Aelith", "Alvina", "Amity", "Audrey", "Bathilda", "Blossom", "Bliss", "Brianna", "Bridgit", "Cara", "Chauncey", "Clementine", "Casey", "Daisy", "Dawn", "Diana",
		"Devon", "Edith", "Edwina", "Elga", "Elvina", "Ermengard", "Evelyn", "Farah", "Faye", "Fiona", "Francesca", "Gidget", "Godiva", "Gunnhild", "Greta", "Hadley",
		"Hazel", "Hannah", "Holly", "Ingrid", "Ivy", "Kenley", "Kelby", "Karen", "Kate", "Laila", "Letha", "Lily", "Lindsey", "Madison", "Maggie", "Mildred", "Misty",
		"Norma", "Nyx", "Nana", "Nerissa", "Olga", "Oletha", "Posy", "Patsy", "Piper", "Precious", "Raissa", "Rhiannon", "Rose", "Roxanne", "Sable", "Shea", "Shelby",
		"Susan", "Suzette", "Tanya", "Tatiana", "Taylor", "Velma", "Vanessa", "Violet", "Whitney", "Willow", "Windy"
	};

	private string[] baseElvenKingdomNames = new string[]{
		"Astalen", "Aerwyn", "Brussia", "Delimar", "Dullahan", "Eroahar", "Etheria", "Fernia", "Finlabad", "Frolien", "Faerwyn",
		"Fyrwen", "Gilinia", "Galawad", "Iberia", "Keswen", "Kirith", "Lustria", "Marowen", "Meniha", "Miraella", "Nargahar",
		"Nurbad", "Oswyth", "Orifia", "Pruwyn", "Pithrilen", "Sardinia", "Solwyn", "Thauniel", "Thalessa", "Uralen", "Urdwyn",
		"Wylia"
	};

	private string[] baseElvenCityNames = new string[]{
		"Aglarond", "Alqualonde", "Avallone", "Beleriand", "Brithombar", "Doriath", "Edhellond", "Eglarest", "Feanan", "Forlond", "Formenos", "Galavaliel",
		"Galadhon", "Gondolin", "Harlond", "Imladris", "Kortirion", "Lindon", "Lorien", "Lothlorien", "Menegroth", "Mithlond", "Nargothrond", "Rivendell",
		"Sherbarad", "Tavrovel", "Thranduil", "Valinor", "Vinyamar"
	};

	private string[] baseElvenFemaleNames = new string[]{
		"Arwen", "Alassea", "Arasinya", "Authiel", "Bainwen", "Beriana", "Calathiel", "Caladwen", "Castiel", "Galadriel", "Gwaerindis", "Ellethwen", "Elarinya",
		"Eleniel", "Erudessa", "Eruraina", "Findemaxa", "Harwel", "Hera", "Luthien", "Medea", "Maerwen", "Medlinya", "Meltoriel", "Nithiel", "Nessima", "Nostariel",
		"Saerwen", "Santiel", "Sidheil", "Silima", "Thoriel", "Thandiel", "Vanya", "Vanafindiel", "Vanessea"
	};

	private string[] baseElvenMaleNames = new string[]{
		"Adan", "Alyameldir", "Amonost", "Arandur", "Baradhamon", "Beriadan", "Beleg", "Bercalion", "Ceberlandon", "Calanon", "Caranion", "Castien", "Celebrimbor",
		"Daeron", "Dramorion", "Durion", "Eruadan", "Eglerion", "Eleyond", "Emerion", "Erunestian", "Faeron", "Faelon", "Feredir", "Glandur", "Herion", "Hirgon", "Horthien", 
		"Imrathon", "Ionwe", "Landion", "Limdur", "Lithaldoren", "Maeron", "Maeglad", "Morcion", "Mornefindon", "Megildur", "Nendir", "Nedhudir", "Nibencarden",
		"Nimtolien", "Orthorien", "Ovorion", "Rainion", "Rhovanion", "Sadron", "Saeldur", "Tangadion", "Taurion", "Thalion", "Turin", "Thorontur", "Voronwe",
		"Vaessen", "Valanyonnen"
	};

	private MarkovNameGenerator generatedHumanSurnames;
	private MarkovNameGenerator generatedHumanKingdomNames;
	private MarkovNameGenerator generatedElvenKingdomNames;
	private MarkovNameGenerator generatedElvenCityNames;
	private MarkovNameGenerator generatedElvenFemaleNames;
	private MarkovNameGenerator generatedElvenMaleNames;

	void Awake(){
		Instance = this;
	}

	void Start(){
		generatedHumanSurnames = new MarkovNameGenerator(baseHumanSurnames, 3, 5);
		generatedHumanKingdomNames = new MarkovNameGenerator(baseHumanKingdomNames, 3, 5);
		generatedElvenKingdomNames = new MarkovNameGenerator(baseElvenKingdomNames, 2, 5);
		generatedElvenCityNames = new MarkovNameGenerator(baseElvenCityNames, 2, 5);
		generatedElvenFemaleNames = new MarkovNameGenerator(baseElvenFemaleNames, 3, 4);
		generatedElvenMaleNames = new MarkovNameGenerator(baseElvenMaleNames, 3, 4);
	}

	public string GenerateRandomName(RACE race, GENDER gender){
		if (race == RACE.HUMANS) {
			return GenerateWholeHumanName(gender);
		} else if(race == RACE.ELVES) {
			return GenerateElvenName(gender);
		}
		return "";
	}

	public string GenerateKingdomName(RACE race){
		if (race == RACE.HUMANS) {
			return generatedHumanKingdomNames.NextName;
		} else if(race == RACE.ELVES) {
			return generatedElvenKingdomNames.NextName;
		}
		return "";
	}

	public string GenerateCityName(RACE race){
		if (race == RACE.HUMANS) {
			return generatedHumanKingdomNames.NextName;
		} else if(race == RACE.ELVES) {
			return generatedElvenCityNames.NextName;
		}
		return "";
	}

	public string GenerateElvenName(GENDER gender){
		if (gender == GENDER.MALE) {
			return generatedElvenMaleNames.NextName;
		} else {
			return generatedElvenFemaleNames.NextName;
		}
	}

	public string GenerateWholeHumanName(GENDER gender){
		string firstName = GetHumanFirstName(gender);
		string surname = GetHumanSurname ();
		return firstName + " " + surname;
	}

	public string GetHumanSurname(){
		return generatedHumanSurnames.NextName;
	}

	public string GetHumanFirstName(GENDER gender){
		if (gender == GENDER.MALE) {
			return humanMaleFirstNames [Random.Range (0, humanMaleFirstNames.Length)];
		} else {
			return humanFemaleFirstNames [Random.Range (0, humanFemaleFirstNames.Length)];
		}
	}

//	public static string GenerateRandomName(){
//		string firstName = firstNames [Random.Range (0, firstNames.Length)];
//		string prefix = prefixes [Random.Range (0, prefixes.Length)];
//		string suffix = suffixes [Random.Range (0, suffixes.Length)];
//
//		int choice = Random.Range (0, 100);
//
//		if (choice >= 0 && choice < 25) {
//			return prefix + " " + firstName;
//		} else {
//			return firstName + " " + suffix;
//		}
//
//		return " ";
//	}
}



