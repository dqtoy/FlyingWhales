using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
		"Atlantis", "Albane", "Alamid", "Anderland", "Aurelia", "Ashanti", "Bathria", "Benin", "Bismuth", "Boron", "Burgundy", "Calabon", "Canton", "Chyland", "Croatia", "Ceres", 
		"Darmid", "Dirland", "Dohati", "Duscany", "Elarus", "Esmeris", "Erebor", "Emusil", "Fernica", "Fuchia", "Friedland", "Farrah", "Garwinia", "Grazil", "Gunark", "Gehenna", 
		"Hacatid", "Hermani", "Hisoka", "Howland", "Ianland", "Iridia", "Israel", "Imisil", "Jamalaya", "Jinni", "Jorara", "Justinia", "Karamba", "Kaedwen", "Korolus", "Kumael",
		"Liria", "Lunesta", "Lusland", "Lyrica", "Makuria", "Meridian", "Mykland", "Morland", "Muskovich", "Nestori", "Norsica", "Nubland", "Naralan", "Obelin", "Ovid", "Opera", 
		"Oosmud", "Patani", "Petersen", "Polaris", "Prissia", "Quatar", "Qualia", "Quenden", "Rastafar", "Remedi", "Russo", "Roveri", "Scythra", "Shetland", "Sundira", "Silveria", 
		"Thenid", "Tilain", "Tesoro", "Tulisia", "Umbra", "Ulbany", "Uruk", "Virindor", "Visaya", "Vanad", "Zulu", "Zakrand", "Zerena"
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
		"Aglarond", "Alqualonde", "Avallone", "Astalen", "Aerwyn", "Arfinal", "Arundel", "Beleriand", "Brithombar", "Berefin", "Bataryn", "Brindelwa", "Curamar", "Celirith", "Cendralien",
		"Casimin", "Carmindel", "Damriel", "Delimar", "Doriath", "Dullahan", "Edhellond", "Eglarest", "Eroahar", "Etheria", "Esinia", "Enlibad", "Fernia", "Finlabad", "Frolien", "Faerwyn", 
		"Fyrwen", "Feanan", "Forlond", "Formenos", "Filarmar", "Gilinia", "Galawad", "Galavaliel", "Galadhon", "Gondolin", "Hamlindras", "Helmongard", "Harlond", "Imladris", "Iberia", "Irluwan", 
		"Kortirion", "Keswen", "Kirith", "Kilawen", "Kalrinien", "Lindon", "Lorien", "Lothlorien", "Lustria", "Legrandos", "Menegroth", "Mithlond", "Marowen", "Meniha", "Miraella", "Nargothrond", 
		"Nargahar", "Nurbad", "Oswyth", "Orifia", "Orfindel", "Pindolwyn", "Pruwyn", "Pithrilen", "Rivendell", "Rilmeroth", "Randilros", "Rilfindor", "Sardinia", "Sherbarad", "Solwyn", "Thauniel", 
		"Thalessa", "Tavrovel", "Thranduil", "Uralen", "Urdwyn", "Valinor", "Vinyamar", "Vasmundin", "Wylia"
	};

	private string[] baseElvenCityNames = new string[]{
		"Aglarond", "Alqualonde", "Avallone", "Beleriand", "Brithombar", "Doriath",
		"Galadhon", "Gondolin", "Harlond", "Imladris", "Lindon", "Lorien", "Lothlorien", "Menegroth", "Mithlond"
	};

	private string[] baseElvenFemaleNames = new string[]{
		"Arwen", "Amarie", "Alassea", "Arasinya", "Aredhel", "Authiel", "Amirala", "Bainwen", "Berissa", "Beriana", "Berondiel" , "Calathiel", "Caladwen", "Celebrian", "Castiel", "Ciristel", "Dayanara", 
		"Darxaniel", "Derenie", "Darla", "Ellenwe", "Ellethwen", "Elarinya", "Eleniel", "Erudessa", "Eldalothe", "Enelye", "Eruraina", "Firinea", "Fyrwen", "Froilwen", "Faralana", "Frindea", "Findemaxa", "Galadriel", 
		"Gwaerindis", "Galhadya", "Gerliana", "Harwel", "Hera", "Hanaxa", "Heloise", "Indis", "Irwindis", "Irsandwen", "Ishmila", "Livindel", "Larthindra", "Levana", "Luthien", "Medea", "Meliantha", "Maerwen", "Medlinya", 
		"Meltoriel", "Miriel", "Nathanya", "Nithiel", "Nimrodel", "Nerdanel", "Nessima", "Nostariel", "Ruanda", "Ruthlien", "Saerwen", "Sandara", "Sirissi", "Santiel", "Sidheil", "Silima", "Thoriel", "Thandiel", 
		"Vanya", "Vanafindiel", "Vanessea"
	};

	private string[] baseElvenMaleNames = new string[]{
		"Adan", "Alyameldir", "Amonost", "Aegnor", "Arandur", "Angrod", "Beren", "Baradhamon", "Beriadan", "Beleg", "Bercalion", "Cadgon", "Curufin", "Canardur", "Ceberlandon", "Calanon", "Caranion", "Castien", 
		"Celebrimbor", "Daeron", "Denethor", "Dramorion", "Durion", "Dirnost", "Eclesteron", "Ecthelion", "Eruadan", "Eglerion", "Eleyond", "Emerion", "Erunestian", "Elrond", "Faeron", "Faelon", "Feredir", "Fandur", "Fistilion", 
		"Fingolfin", "Galion", "Galathil", "Glorfindel", "Glandur", "Gimrinost", "Herion", "Hirgon", "Horthien", "Imrathon", "Ionwe", "Landion", "Legolas", "Limdur", "Lithaldoren", "Maeron", "Maeglad", "Mablung", 
		"Morcion", "Mithrellas", "Mornefindon", "Megildur", "Nendir", "Nedhudir", "Nibencarden", "Nimtolien", "Orodreth", "Orthorien", "Ovorion", "Rainion", "Rhovanion", "Sadron", "Saeldur", "Tangadion", "Taurion", 
		"Thalion", "Turin", "Thorontur", "Voronwe", "Vaessen", "Valanyonnen"
	};

    //private MarkovNameGenerator generatedHumanSurnames;
    //private MarkovNameGenerator generatedHumanKingdomNames;
    //private MarkovNameGenerator generatedElvenKingdomNames;
    ////private MarkovNameGenerator generatedElvenCityNames;
    //private MarkovNameGenerator generatedElvenFemaleNames;
    //private MarkovNameGenerator generatedElvenMaleNames;

    Sobriquet.Generator generatedHumanSurnames;
    Sobriquet.Generator generatedHumanKingdomNames;
    Sobriquet.Generator generatedElvenKingdomNames;
    Sobriquet.Generator generatedElvenFemaleNames;
    Sobriquet.Generator generatedElvenMaleNames;

    private List<string> humanKingdomNames;
    private List<string> humanSurnames;
    private List<string> elvenKingdomNames;
    private List<string> elvenFemaleNames;
    private List<string> elvenMaleNames;

    void Awake(){
		Instance = this;
        generatedHumanSurnames = new Sobriquet.Generator(2, baseHumanSurnames);
        generatedHumanKingdomNames = new Sobriquet.Generator(2, baseHumanKingdomNames);

        generatedElvenKingdomNames = new Sobriquet.Generator(2, baseElvenKingdomNames);
        generatedElvenFemaleNames = new Sobriquet.Generator(2, baseElvenFemaleNames);
        generatedElvenMaleNames = new Sobriquet.Generator(2, baseElvenMaleNames);

        humanKingdomNames = new List<string>();
        for (int i = 5; i <= 8; i++) {
            humanKingdomNames.AddRange(generatedHumanKingdomNames.AllRaw(i).Take(50000).ToList());
        }
        humanKingdomNames.AddRange(generatedHumanKingdomNames.AllRaw(9).Take(20000).ToList());
        humanKingdomNames.AddRange(generatedHumanKingdomNames.AllRaw(10).Take(20000).ToList());
        humanKingdomNames.AddRange(generatedHumanKingdomNames.AllRaw(11).Take(10000).ToList());
        humanKingdomNames.AddRange(generatedHumanKingdomNames.AllRaw(12).Take(10000).ToList());
        humanKingdomNames = Utilities.Shuffle(humanKingdomNames);


        humanSurnames = new List<string>();
        for (int i = 5; i <= 8; i++) {
            humanSurnames.AddRange(generatedHumanSurnames.AllRaw(i).Take(50000).ToList());
        }
        humanSurnames.AddRange(generatedHumanSurnames.AllRaw(9).Take(20000).ToList());
        humanSurnames.AddRange(generatedHumanSurnames.AllRaw(10).Take(20000).ToList());
        humanSurnames.AddRange(generatedHumanSurnames.AllRaw(11).Take(10000).ToList());
        humanSurnames.AddRange(generatedHumanSurnames.AllRaw(12).Take(10000).ToList());
        humanSurnames = Utilities.Shuffle(humanSurnames);

        elvenKingdomNames = new List<string>();
        for (int i = 5; i <= 8; i++) {
            elvenKingdomNames.AddRange(generatedElvenKingdomNames.AllRaw(i).Take(50000).ToList());
        }
        elvenKingdomNames.AddRange(generatedElvenKingdomNames.AllRaw(9).Take(20000).ToList());
        elvenKingdomNames.AddRange(generatedElvenKingdomNames.AllRaw(10).Take(20000).ToList());
        elvenKingdomNames.AddRange(generatedElvenKingdomNames.AllRaw(11).Take(10000).ToList());
        elvenKingdomNames.AddRange(generatedElvenKingdomNames.AllRaw(12).Take(10000).ToList());
        elvenKingdomNames = Utilities.Shuffle(elvenKingdomNames);

        elvenFemaleNames = new List<string>();
        for (int i = 5; i <= 7; i++) {
            elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(i).Take(50000).ToList());
        }
        elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(8).Take(20000).ToList());
        elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(9).Take(20000).ToList());
        elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(10).Take(20000).ToList());
        elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(11).Take(10000).ToList());
        elvenFemaleNames.AddRange(generatedElvenFemaleNames.AllRaw(12).Take(10000).ToList());
        elvenFemaleNames = Utilities.Shuffle(elvenFemaleNames);

        elvenMaleNames = new List<string>();
        for (int i = 5; i <= 7; i++) {
            elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(i).Take(50000).ToList());
        }
        elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(8).Take(20000).ToList());
        elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(9).Take(20000).ToList());
        elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(10).Take(20000).ToList());
        elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(11).Take(10000).ToList());
        elvenMaleNames.AddRange(generatedElvenMaleNames.AllRaw(12).Take(10000).ToList());
        elvenMaleNames = Utilities.Shuffle(elvenMaleNames);

        //generatedHumanSurnames = new MarkovNameGenerator(baseHumanSurnames, 3, 5);
        //      generatedHumanKingdomNames = new MarkovNameGenerator(baseHumanKingdomNames, 3, 5);
        //generatedElvenKingdomNames = new MarkovNameGenerator(baseElvenKingdomNames, 3, 6);
        //generatedElvenCityNames = new MarkovNameGenerator(baseElvenKingdomNames, 2, 5);
        //generatedElvenFemaleNames = new MarkovNameGenerator(baseElvenFemaleNames, 3, 4);
        //generatedElvenMaleNames = new MarkovNameGenerator(baseElvenMaleNames, 3, 4);
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
            if(humanKingdomNames.Count <= 0) {
                humanKingdomNames = generatedHumanKingdomNames.AllRaw(12).ToList();
            }
            int index = Random.Range(0, humanKingdomNames.Count);
            string humanKingdomName = humanKingdomNames[index];
            //humanKingdomNames.RemoveAt(index);
            return humanKingdomName.Trim();
		} else if(race == RACE.ELVES) {
            if (elvenKingdomNames.Count <= 0) {
                elvenKingdomNames = generatedElvenKingdomNames.AllRaw(12).ToList();
            }
            int index = Random.Range(0, elvenKingdomNames.Count);
            string elvenKingdomName = elvenKingdomNames[index];
            //elvenKingdomNames.RemoveAt(index);
            return elvenKingdomName.Trim();
        }
		return "";
	}

	public string GenerateCityName(RACE race){
        if (race == RACE.HUMANS) {
            if (humanKingdomNames.Count <= 0) {
                humanKingdomNames = generatedHumanKingdomNames.AllRaw(12).ToList();
            }
            int index = Random.Range(0, humanKingdomNames.Count);
            string humanKingdomName = humanKingdomNames[index];
            //humanKingdomNames.RemoveAt(index);
            return humanKingdomName;
        } else if (race == RACE.ELVES) {
            if (elvenKingdomNames.Count <= 0) {
                elvenKingdomNames = generatedElvenKingdomNames.AllRaw(12).ToList();
            }
            int index = Random.Range(0, elvenKingdomNames.Count);
            string elvenKingdomName = elvenKingdomNames[index];
            //elvenKingdomNames.RemoveAt(index);
            return elvenKingdomName;
        }
        return "";
	}

	public string GenerateElvenName(GENDER gender){
		if (gender == GENDER.MALE) {
            if (elvenMaleNames.Count <= 0) {
                elvenMaleNames = generatedElvenMaleNames.AllRaw(12).ToList();
            }
            int index = Random.Range(0, elvenMaleNames.Count);
            string elvenMaleName = elvenMaleNames[index];
            //elvenMaleNames.RemoveAt(index);
            return elvenMaleName.Trim();
		} else {
            if (elvenFemaleNames.Count <= 0) {
                elvenFemaleNames = generatedElvenFemaleNames.AllRaw(12).ToList();
            }
            int index = Random.Range(0, elvenFemaleNames.Count);
            string elvenFemaleName = elvenFemaleNames[index];
            //elvenFemaleNames.RemoveAt(index);
            return elvenFemaleName.Trim();
		}
//		return "";
	}

	public string GenerateWholeHumanName(GENDER gender){
		string firstName = GetHumanFirstName(gender);
		string surname = GetHumanSurname ();
		return firstName + " " + surname;
	}

	public string GetHumanSurname(){
        if (humanSurnames.Count <= 0) {
            humanSurnames = generatedHumanSurnames.AllRaw(12).ToList();
        }
        int index = Random.Range(0, humanSurnames.Count);
        string humanSurname = humanSurnames[index];
        //humanSurnames.RemoveAt(index);
        return humanSurname;
//		return "";
	}

	public string GetHumanFirstName(GENDER gender){
		if (gender == GENDER.MALE) {
			return humanMaleFirstNames [Random.Range (0, humanMaleFirstNames.Length)];
		} else {
			return humanFemaleFirstNames [Random.Range (0, humanFemaleFirstNames.Length)];
		}
//		return "";
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



