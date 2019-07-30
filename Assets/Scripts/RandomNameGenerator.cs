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
		"Darmid", "Delta", "Dirland", "Dohati", "Duscany", "Elarus", "Esmeris", "Erebor", "Emusil", "Erathia", "Folgeron", "Fernica", "Fuchia", "Friedland", "Farrah", "Garwinia", "Grazil", "Gunark", 
		"Gehenna", "Gladius", "Hacatid", "Hermani", "Hisoka", "Howland", "Hunan", "Ianland", "Iridia", "Israel", "Indosin", "Illumina", "Jamalaya", "Jinni", "Jorara", "Justinia", "Jelsebed",
		"Karamba", "Kaedwen", "Korolus", "Kumael", "Kestrella", "Losendro", "Liria", "Lunesta", "Lusland", "Lyrica", "Makuria", "Meridian", "Mykland", "Morland", "Muskovich", "Nestori", "Norsica", 
		"Nubland", "Naralan", "Nissin", "Obelin", "Ovid", "Opera", "Osmud", "Obelisk", "Patani", "Petersen", "Polaris", "Prissia", "Pusant", "Quatar", "Qualia", "Quenden", "Rastafar", "Remedi", "Russo", 
		"Roveri", "Scythra", "Shetland", "Sundira", "Silveria", "Sahari", "Takatak", "Thenid", "Tilain", "Tesoro", "Tulisia", "Umbra", "Ulbany", "Uruk", "Unibad", "Virindor", "Visaya", "Vanad", "Vesemin",
		"Vortex", "Zulu", "Zakrand", "Zerena", "Ziria"
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

	//private string[] baseElvenCityNames = new string[]{
	//	"Aglarond", "Alqualonde", "Avallone", "Beleriand", "Brithombar", "Doriath",
	//	"Galadhon", "Gondolin", "Harlond", "Imladris", "Lindon", "Lorien", "Lothlorien", "Menegroth", "Mithlond"
	//};

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

	private string[] baseAncientRuinPrefixes = new string[]{
		"Mystic", "Obsidian", "Shadow", "Glimmer", "Frey", "Arrow", "Deep", "Moon", "Ancient", "Forbidden", "Oblivion", "Still", "Dragon", "Amber", "Wendi", "Ingle", "Isle", "White", "Black", "Grimm", "New", "Small",
		"Hound", "Stone", "Cloud", "Frey", "Crystal", "Rage", "Mist", "Raven", "Troll", "Goblin", "Cold", "Angel", "Evil", "Mythic", "Silent", "Wailing", "Tilted", "Snobby", "Shifty", "Flush", "Greasy", "Anarchy", "Fatal",
		"Haunted", "Junk", "Pleasant", "Lonely", "Moisty", "Forgotten", "Salty", "Cruel", "Thunder", "Oaken", "Wind", "Never", "Basin"
	};

	private string[] baseAncientRuinSuffixes = new string[]{
		"lair", "town", "vault", "ruin", "lost", "hallow", "drift", "shade", "hold", "grove", "shell", "wick", "ville", "hill", "garde", "reach", "den", "dale", "moor", "more", "view", "wich", "borough", "chill", "berg", "burgh", "ster", "stall", 
		"cross", "storm", "fell", "bury", "ward", "hand", "cliff", "valley", "mire", "gulch", "well", "hall", "gall", "pass", "hollow", "mere", "land", "cairn", "wood", "lands", "horn", "keep", "cre", "scar", "shield", "rand", "borne", "port", "guard", "mount", 
		"bay", "high", "bourne", "helm", "frost", "mouth", "dusk", "wild", "ness", "dread", "warts", "nook", "spire", "steep", "frey", "fort"
	};

	private string[] baseTileNames = new string[]{
		"Aecianara", "Graggadalar", "Chussirah", "Strukimelan", "Slaeddithis", "Plobeonata", "Zeaconet", "Ceapiarial", "Inniariel", "Oseotara", "Kruziven", "Creokkiogarth", "Gloyiarial", "Pephadin", "Wubegus",
		"Uqupia", "Vreakkezan", "Kriaggagarth", "Craepenet", "Caeqarus", "Iobeocion", "Headragana", "Jiacrariel", "Yidrelan", "Sleatania", "Edramar", "Abbedran", "Stassuspea", "Gruhorene", "Paddiriel",
		"Eddiothis", "Olasia", "Chommagarth", "Luveaxath", "Pollonor", "Criassatuary", "Bresarea", "Heappeala", "Tiacrithaer", "Iodorune", "Annetika", "Waeqiomond", "Goglalan", "Caecasia", "Ashocia",
		"Chiafoxus", "Bruniothis", "Fliayetha", "Ugrimund", "Ialisia", "Ioppeamos", "Heahearia", "Iocearim", "Aereaphere", "Eachemar", "Ostreatara", "Acliovar", "Eollezan", "Siagrarath", "Flioddearea",
		"Staebbiaphere", "Teastrelar", "Ximiathra", "Wokkelar", "Umular", "Iossaque", "Kloddenet", "Ecether", "Greaxoque", "Ioweotha", "Tatatope", "Funiomond", "Riocrituary", "Dreohasos", "Eossigana",
		"Kreaqiodin", "Meammeaspea", "Bresalan", "Struledore", "Ihorynn", "Bleagirah", "Heanniodu", "Zemmolas", "Cistiapia", "Ottelon", "Heomiorim", "Rummemos", "Wreassiogana", "Accanara", "Wreoddiaryon"
	};

    private string[] baseRegionNames = new string[] {
        "Caloocan", "Las Piñas", "Makati", "Malabon", "Mandaluyong", "Manila", "Marikina", "Muntinlupa", "Navotas",
        "Parañaque", "Pasay", "Pasig", "Quezon", "San Juan", "Taguig", "Valenzuela"
    };

    private string[] minionNames = new string[] {
        "Abraxas", "Agares", "Aim", "Alloces", "Amdusias", "Amon", "Amy", "Andras", "Andrealphus", "Adromalius", "Asmodeus",
        "Astaroth", "Azazel", "Baal", "Baphomet", "Barbatos", "Barong", "Bathin", "Balam", "Beleth", "Belial", "Belphegor",
        "Berith", "Bifrons", "Botis", "Buer", "Bune", "Caim", "Choronzon", "Crocell", "Dantalion", "Decarabia", "Demogorgon", "Eligos",
        "Flauros", "Focalor", "Foras", "Forneus", "Furcas", "Furfur", "Gaap", "Gremory", "Glasya-labolas", "Gusion", "Haagenti",
        "Halphas", "Ipos", "Kimaris", "Leraje", "Lucifer", "Malphas", "Marax", "Marbas", "Marchosias", "Murmur", "Naberius", "Orias",
        "Orobas", "Ose", "Paimon", "Phenex", "Purson", "Raum", "Ronove", "Sabnock", "Samigina", "Sallos", "Seere", "Shax", "Sitri", "Stolas",
        "Valac", "Vapula", "Vassago", "Valefor", "Vepar", "Vine", "Vual", "Zepar", "Zagan"
    };

    private string[] spiderNames = new string[] {
        "Glork", "Rakkan", "Bumble", "Lartath", "Typchray", "Kahu", "Kaljou", "Tuvok", "Kyshf", "Palluhae", "Ezeroc",
        "Spidrid", "Slerdach", "Meleth", "Snendos", "Libnrak", "Umusaq", "Serpigo", "Ijushir", "Eshacer", "Alakyrr", "Iguker", "Skirax",
        "Ujarak", "Eruanna", "Annelida", "Naraku", "Xaggavea", "Ilphstra", "Ekicak", "Scissa", "Lakkucoa", "Tsuki", "Incey", "Ahmose", "Aurantia", "Alta", "Hesutu",
        "Anastera", "Krigon", "Seti", "Tal Tal", "Feriave", "Elifif", "Arkaitz", "Adiyis", "Shelob", "Xennowua", "Aine", "Gigit",
    };

    private string[] faeryFemaleNames = new string[] {
        "Lorelie", "Nixie", "Sereia", "Tiana", "Naida", "Melia", "Delphine", "Celeste", "Avery", "Asherah", "Ailsa",
        "Diana", "Cyrena", "Fiona", "Spectra", "Siofra", "Zanna", "Sebille", "Radella", "Oona", "Marigold", "Fayette", "Dariyah",
        "Asteria", "Kaia", "Aurora",
    };

    private string[] faeryMaleNames = new string[] {
        "Caspian", "Arion", "Jareth", "Oberon", "Triton", "Zephyr", "Cosmo", "Aelfdene", "Nyx", "Xantho", "Gullveig",
        "Flynn", "Helio", "Flix", "Cleon", "Lazuli", "Trevan", "Aphid", "Tarragon", "Caraway", "Carpus", "Skylark", "Cirro",
        "Alaneo", "Ginko", "Oleander",
    };

    private string[] goblinFemaleNames = new string[] {
        "Shanxee", "Fegrahx", "Stebdois", "Trahxi", "Shalx", "Cholme", "Gnokesh", "Deevons", "Blahossa", "Voplehx", "Dofil",
        "Iofz", "Mikild", "Trohee", "Onxe", "Wraalta", "Retzaga", "Fegsi", "Thriz", "Dyq", "Oinun", "Gyflult", "Klaasai",
        "Barleeth", "Dyteess",
    };

    private string[] goblinMaleNames = new string[] {
        "Kreelk", "Kohdibs", "Lasdoir", "Ukoc", "Sloikz", "Vreg", "Shapvith", "Voss", "Uvrefz", "Yzenk", "Brirx",
        "Sruiz", "Kegdiart", "Taarsots", "Srurmaar", "Jignierk", "Crezlezz", "Fraatukt", "Zolruirm", "Canralk", "Prevrax", "Hioq", "Zees",
        "Hobigs", "Fizigs",
    };

    #region Alliance
    private string[] allianceType = new string[]{
		"Alliance", "League", "Coalition", "Axis", "Union", "Entente", "Accord"
	};
	private string[] allianceNoun = new string[]{
		"Arms", "Baes", "Darkness", "Dogs", "Flame", "Future", "Fury", "Genius", "Guys", "Hands",
		"Light", "Might", "Peace", "People", "Pigs", "Promise", "Power", "Sweetness", "Sword", "Terror",
		"Unity", "World", "Zone"
	};
	private string[] allianceAdjective = new string[]{
		"Ancient", "Black", "Brave", "Calm", "Charming", "Crimson", "Cruel", "Cunning", "Eternal", "Evil",
		"Fantastic", "Fearless", "Green", "Holy", "Huge", "Intimidating", "Loyal", "Nasty", "Nice", "Passionate",
		"Pure", "Royal", "Ruthless", "Salty", "Sensible", "Sneaky", "Strong", "United", "White"
	};
	#endregion

	#region Warfare
	private string[] warfareAdjective = new string[]{
		"Acrid", "Bitter", "Bleeding", "Black", "Bloody", "Chilling", "Colossal", "Craven", "Daring", "Deadly",
		"Extreme", "Fierce", "Lazy", "Old", "Quarreling", "Random", "Red", "Sacred", "Starving", "Thundering", "Zealous"
	};
	private string[] warfareNoun = new string[]{
		"Aim", "Artists", "Bet", "Claim", "Crusade", "Domain", "Error", "Fork", "Graves", "Hearts", 
		"Hills", "Intent", "Insults", "Justice", "King", "Knights", "Letters", "Lions", "Lovers", "Madman",
		"Offense", "Passion", "Peasants", "Potatoes", "Rage", "Survival", "Truth", "Vipers"
	};
	#endregion

	#region International Incident
	private string[] incidentType = new string[]{
		"Incident", "Affair", "Crisis", "Issue", "Dilemma", "Clash"	
	};
	private string[] incidentNoun = new string[]{
		"Animal", "Argument", "Bastion", "Child", "Destiny", "Diplomat", "Elephant", "Face", "Farmer", "Grudge", "Lake",
		"Leather", "Man", "Mortar", "Muse", "Nobleman", "Promise", "Rally", "Seeds", "Spring", "Shelter", "Trash", "Trader", "Trespasser",
		"Tent", "Woman"
	};
	private string[] incidentAdjective = new string[] {
		"Angry", "Bizarre", "Broken", "Crushed", "Desolate", "Eroded", "Grumbling", "Failing", "Heretical", "Isolated", "Jaded", "Jilted",
		"Killer", "Lonely", "Missing", "Nasty", "Ornamental", "Poor", "Quiet", "Raging", "Rotten", "Sad", "Tragic", "Undulating" 
	};
	#endregion

    Sobriquet.Generator generatedHumanSurnames;
    Sobriquet.Generator generatedHumanKingdomNames;
    Sobriquet.Generator generatedElvenKingdomNames;
    Sobriquet.Generator generatedElvenFemaleNames;
    Sobriquet.Generator generatedElvenMaleNames;
	Sobriquet.Generator generatedAncientRuinNames;
	Sobriquet.Generator generatedTileNames;
    Sobriquet.Generator generatedRegionNames;

    private List<string> humanKingdomNames;
    private List<string> humanSurnames;
    private List<string> elvenKingdomNames;
    private List<string> elvenFemaleNames;
    private List<string> elvenMaleNames;
	private List<string> ancientRuinNames;
	private List<string> tileNames;
    private List<string> regionNames;
    private List<string> availableMinionNames;
    private List<string> availableSpiderNames;
    private List<string> availableHumanMaleNames;
    private List<string> availableHumanFemaleNames;
    private List<string> availableFaeryFemaleNames;
    private List<string> availableFaeryMaleNames;
    private List<string> availableGoblinFemaleNames;
    private List<string> availableGoblinMaleNames;

    void Awake(){
		Instance = this;
        generatedHumanSurnames = new Sobriquet.Generator(2, baseHumanSurnames);
        generatedHumanKingdomNames = new Sobriquet.Generator(2, baseHumanKingdomNames);

        generatedElvenKingdomNames = new Sobriquet.Generator(2, baseElvenKingdomNames);
        generatedElvenFemaleNames = new Sobriquet.Generator(2, baseElvenFemaleNames);
        generatedElvenMaleNames = new Sobriquet.Generator(2, baseElvenMaleNames);

		generatedAncientRuinNames = new Sobriquet.Generator (2, baseAncientRuinPrefixes);
		generatedTileNames = new Sobriquet.Generator (2, baseTileNames);

        generatedRegionNames = new Sobriquet.Generator(2, baseRegionNames);

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

		ancientRuinNames = new List<string>();
		for (int i = 4; i <= 6; i++) {
			ancientRuinNames.AddRange(generatedAncientRuinNames.AllRaw(i).Take(50000).ToList());
		}
		ancientRuinNames = Utilities.Shuffle(ancientRuinNames);

		tileNames = new List<string>();
		for (int i = 6; i <= 9; i++) {
			tileNames.AddRange(generatedTileNames.AllRaw(i).Take(20000).ToList());
		}
		tileNames = Utilities.Shuffle(tileNames);

        regionNames = new List<string>();
        for (int i = 6; i <= 9; i++) {
            regionNames.AddRange(generatedRegionNames.AllRaw(i).Take(20000).ToList());
        }
        regionNames = Utilities.Shuffle(regionNames);

        availableMinionNames = new List<string>(minionNames);
        availableSpiderNames = new List<string>(spiderNames);
        availableHumanFemaleNames = new List<string>(humanFemaleFirstNames);
        availableHumanMaleNames = new List<string>(humanMaleFirstNames);
        availableFaeryFemaleNames = new List<string>(faeryFemaleNames);
        availableFaeryMaleNames = new List<string>(faeryMaleNames);
        availableGoblinFemaleNames = new List<string>(goblinFemaleNames);
        availableGoblinMaleNames = new List<string>(goblinMaleNames);

        //generatedHumanSurnames = new MarkovNameGenerator(baseHumanSurnames, 3, 5);
        //      generatedHumanKingdomNames = new MarkovNameGenerator(baseHumanKingdomNames, 3, 5);
        //generatedElvenKingdomNames = new MarkovNameGenerator(baseElvenKingdomNames, 3, 6);
        //generatedElvenCityNames = new MarkovNameGenerator(baseElvenKingdomNames, 2, 5);
        //generatedElvenFemaleNames = new MarkovNameGenerator(baseElvenFemaleNames, 3, 4);
        //generatedElvenMaleNames = new MarkovNameGenerator(baseElvenMaleNames, 3, 4);
    }
    private void Start() {
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }

    public string GenerateMinionName() {
        string chosenName = availableMinionNames[Random.Range(0, availableMinionNames.Count)];
        availableMinionNames.Remove(chosenName);
        //if (availableMinionNames.Count == 0) {
        //    availableMinionNames.AddRange(minionNames);
        //}
        return chosenName;
    }

    public string GenerateSpiderName() {
        string chosenName = availableSpiderNames[Random.Range(0, availableSpiderNames.Count)];
        availableSpiderNames.Remove(chosenName);
        //if (availableSpiderNames.Count == 0) {
        //    availableSpiderNames.AddRange(minionNames);
        //}
        return chosenName;
    }
    public string GenerateFaeryName(GENDER gender) {
        string chosenName = string.Empty;
        if(gender == GENDER.MALE) {
            if (availableFaeryMaleNames.Count <= 0) {
                availableFaeryMaleNames.AddRange(faeryMaleNames);
            }
            int index = UnityEngine.Random.Range(0, availableFaeryMaleNames.Count);
            chosenName = availableFaeryMaleNames[index];
            availableFaeryMaleNames.RemoveAt(index);
        } else {
            if (availableFaeryFemaleNames.Count <= 0) {
                availableFaeryFemaleNames.AddRange(faeryFemaleNames);
            }
            int index = UnityEngine.Random.Range(0, availableFaeryFemaleNames.Count);
            chosenName = availableFaeryFemaleNames[index];
            availableFaeryFemaleNames.RemoveAt(index);
        }
        return chosenName;
    }

    public string GenerateGoblinName(GENDER gender) {
        string chosenName = string.Empty;
        if (gender == GENDER.MALE) {
            if (availableGoblinMaleNames.Count <= 0) {
                availableGoblinMaleNames.AddRange(goblinMaleNames);
            }
            int index = UnityEngine.Random.Range(0, availableGoblinMaleNames.Count);
            chosenName = availableGoblinMaleNames[index];
            availableGoblinMaleNames.RemoveAt(index);
        } else {
            if (availableGoblinFemaleNames.Count <= 0) {
                availableGoblinFemaleNames.AddRange(goblinFemaleNames);
            }
            int index = UnityEngine.Random.Range(0, availableGoblinFemaleNames.Count);
            chosenName = availableGoblinFemaleNames[index];
            availableGoblinFemaleNames.RemoveAt(index);
        }
        return chosenName;
    }

    private void OnCharacterDied(Character characterThatDied) {
        if (characterThatDied.minion != null) {
            //minion that died
            if (!availableMinionNames.Contains(characterThatDied.name)) {
                availableMinionNames.Add(characterThatDied.name); //return name to pool
            }
        }
    }

    public string GenerateRandomName(RACE race, GENDER gender){
		if (race == RACE.HUMANS) {
			return GenerateWholeHumanName(gender);
		} else if(race == RACE.ELVES) {
			return GenerateElvenName(gender);
        } else if (race == RACE.SPIDER) {
            return GenerateSpiderName();
        } else if (race == RACE.FAERY) {
            return GenerateFaeryName(gender);
        } else if (race == RACE.GOBLIN) {
            return GenerateGoblinName(gender);
        }
        return GenerateElvenName(gender);
	}

	public string GenerateKingdomName(){
        if(humanKingdomNames.Count <= 0) {
            humanKingdomNames = generatedHumanKingdomNames.AllRaw(12).ToList();
        }
        int index = Random.Range(0, humanKingdomNames.Count);
        string humanKingdomName = humanKingdomNames[index];
        //humanKingdomNames.RemoveAt(index);
        return humanKingdomName.Trim();
		//} else if(race == RACE.ELVES) {
  //          if (elvenKingdomNames.Count <= 0) {
  //              elvenKingdomNames = generatedElvenKingdomNames.AllRaw(12).ToList();
  //          }
  //          int index = Random.Range(0, elvenKingdomNames.Count);
  //          string elvenKingdomName = elvenKingdomNames[index];
  //          //elvenKingdomNames.RemoveAt(index);
  //          return elvenKingdomName.Trim();
  //      }
		//return "";
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
            string chosenName = availableHumanMaleNames[Random.Range(0, availableHumanMaleNames.Count)];
            availableHumanMaleNames.Remove(chosenName);
            if (availableHumanMaleNames.Count == 0) {
                availableHumanMaleNames = new List<string>(humanMaleFirstNames);
            }
            return chosenName;
		} else {
            string chosenName = availableHumanFemaleNames[Random.Range(0, availableHumanFemaleNames.Count)];
            availableHumanFemaleNames.Remove(chosenName);
            if (availableHumanFemaleNames.Count == 0) {
                availableHumanFemaleNames = new List<string>(humanFemaleFirstNames);
            }
            return chosenName;
        }
//		return "";
	}

	public string GetAllianceName(){
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 35){
			return allianceAdjective [UnityEngine.Random.Range (0, allianceAdjective.Length)] + " " + allianceNoun [UnityEngine.Random.Range (0, allianceNoun.Length)] + " " + allianceType [UnityEngine.Random.Range (0, allianceType.Length)];
		}else if(chance >= 35 && chance < 60){
			return allianceNoun [UnityEngine.Random.Range (0, allianceNoun.Length)] + " " + allianceType [UnityEngine.Random.Range (0, allianceType.Length)];
		}else if(chance >= 60 && chance < 75){
			return allianceType [UnityEngine.Random.Range (0, allianceType.Length)]+ " of " + allianceNoun [UnityEngine.Random.Range (0, allianceNoun.Length)];
		}else{
			return allianceType [UnityEngine.Random.Range (0, allianceType.Length)]+ " of " + allianceAdjective [UnityEngine.Random.Range (0, allianceAdjective.Length)] + " " + allianceNoun [UnityEngine.Random.Range (0, allianceNoun.Length)];
		}
	}
	public string GetWarfareName(){
		int chance = UnityEngine.Random.Range (0, 2);
		if(chance == 0){
			return "War of " + warfareAdjective[UnityEngine.Random.Range(0, warfareAdjective.Length)] + " " + warfareNoun[UnityEngine.Random.Range(0, warfareNoun.Length)];
		}else{
			return "War of the " + warfareAdjective[UnityEngine.Random.Range(0, warfareAdjective.Length)] + " " + warfareNoun[UnityEngine.Random.Range(0, warfareNoun.Length)];
		}
	}
	public string GetInternationalIncidentName(){
		return incidentAdjective[UnityEngine.Random.Range(0, incidentAdjective.Length)] + " " + incidentNoun[UnityEngine.Random.Range(0, incidentNoun.Length)] + " " + incidentType[UnityEngine.Random.Range(0, incidentType.Length)];

	}
    public string GetLandmarkName(LANDMARK_TYPE landmarkType) {
        return GetAncientRuinName();
    }

	public string GetAncientRuinName(){
		if(ancientRuinNames.Count <= 0) {
			ancientRuinNames = generatedAncientRuinNames.AllRaw(6).ToList();
		}
		int index = UnityEngine.Random.Range (0, ancientRuinNames.Count);
		string name = ancientRuinNames [index];
		ancientRuinNames.RemoveAt (index);
		return name + baseAncientRuinSuffixes [UnityEngine.Random.Range (0, baseAncientRuinSuffixes.Length)];
	}
	public string GetTileName(){
		if(tileNames.Count <= 0) {
			tileNames = generatedTileNames.AllRaw(6).ToList();
		}
		int index = UnityEngine.Random.Range (0, tileNames.Count);
		string name = tileNames [index];
		tileNames.RemoveAt (index);
		return name;
	}
    public string GetRegionName() {
        if (regionNames.Count <= 0) {
            regionNames = generatedRegionNames.AllRaw(6).ToList();
        }
        int index = UnityEngine.Random.Range(0, regionNames.Count);
        string name = regionNames[index];
        regionNames.RemoveAt(index);
        return name;
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

    public void RemoveNameAsAvailable(GENDER gender, RACE race, string name) {
        switch (race) {
            case RACE.HUMANS:
                if (gender == GENDER.MALE) {
                    availableHumanMaleNames.Remove(name);
                }else {
                    availableHumanFemaleNames.Remove(name);
                }
                break;
            
        }
    }
}



