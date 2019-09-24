
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokenManager : MonoBehaviour {
    public static TokenManager Instance;

    public List<SpecialTokenSettings> specialTokenSettings;

    [SerializeField] private ItemSpriteDictionary itemSpritesDictionary;
    public List<SpecialObject> specialObjects { get; private set; }

    void Awake() {
        Instance = this;
        specialObjects = new List<SpecialObject>();
    }

    public void Initialize() {
        LoadSpecialTokens();
    }

    private void LoadSpecialTokens() {
        //Reference: https://trello.com/c/Kuqt3ZSP/2610-put-2-healing-potions-in-the-warehouse-at-start-of-the-game
        LocationStructure warehouse = LandmarkManager.Instance.enemyOfPlayerArea.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        for (int i = 0; i < 4; i++) {
            LandmarkManager.Instance.enemyOfPlayerArea.AddSpecialTokenToLocation(CreateSpecialToken(SPECIAL_TOKEN.HEALING_POTION), warehouse);
        }
        for (int i = 0; i < 2; i++) {
            LandmarkManager.Instance.enemyOfPlayerArea.AddSpecialTokenToLocation(CreateSpecialToken(SPECIAL_TOKEN.TOOL), warehouse);
        }

        for (int i = 0; i < specialTokenSettings.Count; i++) {
            SpecialTokenSettings currSetting = specialTokenSettings[i];
            List<Area> areas = LandmarkManager.Instance.allAreas;
            //List<Area> areas = GetPossibleAreaSpawns(currSetting);
            //if (areas.Count <= 0) {
            //    continue; //skip
            //}
            for (int j = 0; j < currSetting.quantity; j++) {
                if (Random.Range(0, 100) < currSetting.appearanceWeight) {
                    Area chosenArea = areas[Random.Range(0, areas.Count)];
                    SpecialToken createdToken = CreateSpecialToken(currSetting.tokenType, currSetting.appearanceWeight);
                    if (createdToken != null) {
                        chosenArea.AddSpecialTokenToLocation(createdToken);
                        //createdToken.SetOwner(chosenArea.owner); //Removed this because of redundancy, SetOwner is already being called inside AddSpecialTokenToLocation
                        //Messenger.Broadcast<SpecialToken>(Signals.SPECIAL_TOKEN_CREATED, createdToken);
                    }
                }
            }
        }
    }
    public SpecialToken CreateRandomDroppableSpecialToken() {
        SPECIAL_TOKEN[] choices = Utilities.GetEnumValues<SPECIAL_TOKEN>().Where(x => x.CreatesObjectWhenDropped()).ToArray();
        SPECIAL_TOKEN random = choices[Random.Range(0, choices.Length)];
        return CreateSpecialToken(random);
    }
    public SpecialToken CreateSpecialToken(SPECIAL_TOKEN tokenType, int appearanceWeight = 0) {
        switch (tokenType) {
            case SPECIAL_TOKEN.BLIGHTED_POTION:
                return new BlightedPotion();
            case SPECIAL_TOKEN.BOOK_OF_THE_DEAD:
                return new BookOfTheDead();
            case SPECIAL_TOKEN.CHARM_SPELL:
                return new CharmSpell();
            case SPECIAL_TOKEN.FEAR_SPELL:
                return new FearSpell();
            case SPECIAL_TOKEN.MARK_OF_THE_WITCH:
                return new MarkOfTheWitch();
            case SPECIAL_TOKEN.BRAND_OF_THE_BEASTMASTER:
                return new BrandOfTheBeastmaster();
            case SPECIAL_TOKEN.BOOK_OF_WIZARDRY:
                return new BookOfWizardry();
            case SPECIAL_TOKEN.SECRET_SCROLL:
                return new SecretScroll();
            case SPECIAL_TOKEN.MUTAGENIC_GOO:
                return new MutagenicGoo();
            case SPECIAL_TOKEN.DISPEL_SCROLL:
                return new DispelScroll();
            case SPECIAL_TOKEN.PANACEA:
                return new Panacea();
            case SPECIAL_TOKEN.ENCHANTED_AMULET:
                return new EnchantedAmulet();
            case SPECIAL_TOKEN.GOLDEN_NECTAR:
                return new GoldenNectar();
            case SPECIAL_TOKEN.SCROLL_OF_POWER:
                return new ScrollOfPower();
            case SPECIAL_TOKEN.ACID_FLASK:
                return new AcidFlask();
            case SPECIAL_TOKEN.HEALING_POTION:
                return new HealingPotion();
            case SPECIAL_TOKEN.WATER_BUCKET:
                return new WaterBucket();
            default:
                return new SpecialToken(tokenType, appearanceWeight);
        }
    }
    public SpecialTokenSettings GetTokenSettings(SPECIAL_TOKEN tokenType) {
        for (int i = 0; i < specialTokenSettings.Count; i++) {
            if(specialTokenSettings[i].tokenType == tokenType) {
                return specialTokenSettings[i];
            }
        }
        return null;
    }
    public List<Area> GetPossibleAreaSpawns(SpecialTokenSettings setting) {
        List<Area> areas = new List<Area>();
        for (int i = 0; i < setting.areaLocations.Count; i++) {
            string areaName = setting.areaLocations[i];
            Area area = LandmarkManager.Instance.GetAreaByName(areaName);
            if (area == null) {
                //throw new System.Exception("There is no area named " + areaName);
            } else {
                areas.Add(area);
            }
        }
        return areas;
    }

    public Sprite GetItemSprite(SPECIAL_TOKEN tokenType) {
        if (itemSpritesDictionary.ContainsKey(tokenType)) {
            return itemSpritesDictionary[tokenType];
        }
        return null;
    }
    public void AddSpecialObject(SpecialObject obj) {
        specialObjects.Add(obj);
    }
    public SpecialObject GetSpecialObjectByID(int id) {
        for (int i = 0; i < specialObjects.Count; i++) {
            if(specialObjects[i].id == id) {
                return specialObjects[i];
            }
        }
        return null;
    }
}

[System.Serializable]
public class SpecialTokenSettings {
    public string tokenName;
    public SPECIAL_TOKEN tokenType;
    public int quantity;
    public int appearanceWeight;
    public List<string> areaLocations;
}
