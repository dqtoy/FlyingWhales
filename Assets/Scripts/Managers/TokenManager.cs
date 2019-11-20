
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;

public class TokenManager : MonoBehaviour {
    public static TokenManager Instance;

    public List<SpecialTokenSettings> specialTokenSettings;

    [SerializeField] private ItemSpriteDictionary itemSpritesDictionary;
    public List<SpecialObject> specialObjects { get; private set; }
    public List<SpecialToken> specialTokens { get; private set; }

    public Dictionary<SPECIAL_TOKEN, ItemData> itemData { get; private set; }

    void Awake() {
        Instance = this;

        //TODO: move this somewhere safer
        specialObjects = new List<SpecialObject>();
        specialTokens = new List<SpecialToken>();
    }

    public void Initialize() {
        ConstructItemData();
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
                if (UnityEngine.Random.Range(0, 100) < currSetting.appearanceWeight) {
                    Area chosenArea = areas[UnityEngine.Random.Range(0, areas.Count)];
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
        SPECIAL_TOKEN random = choices[UnityEngine.Random.Range(0, choices.Length)];
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
    public void AddSpecialToken(SpecialToken token) {
        specialTokens.Add(token);
    }
    public SpecialToken GetSpecialTokenByID(int id) {
        for (int i = 0; i < specialTokens.Count; i++) {
            if (specialTokens[i].id == id) {
                return specialTokens[i];
            }
        }
        return null;
    }
    private void ConstructItemData() {
        itemData = new Dictionary<SPECIAL_TOKEN, ItemData>() {
            {SPECIAL_TOKEN.TOOL, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.BLIGHTED_POTION, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.BOOK_OF_THE_DEAD, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.CHARM_SPELL, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.FEAR_SPELL, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.MARK_OF_THE_WITCH, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.BRAND_OF_THE_BEASTMASTER, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.BOOK_OF_WIZARDRY, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.SECRET_SCROLL, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.MUTAGENIC_GOO, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.DISPEL_SCROLL, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.PANACEA, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.JUNK, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.HEALING_POTION, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Healer), typeof(Herbalist) } } },
            {SPECIAL_TOKEN.ENCHANTED_AMULET, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.GOLDEN_NECTAR, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.SCROLL_OF_POWER, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.ACID_FLASK, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
            {SPECIAL_TOKEN.SCROLL_OF_FRENZY, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new Type[] { typeof(Builder) } } },
        };
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

public struct ItemData {
    public int supplyValue;
    public int craftCost;
    public int purchaseCost;
    public System.Type[] canBeCraftedBy;
}