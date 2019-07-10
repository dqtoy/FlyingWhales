using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;

public class TokensUI : UIMenu {

    [Header("Characters")]
    [SerializeField] private GameObject characterEntryPrefab;
    [SerializeField] private ScrollRect tokensScrollView;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private Vector3 halfPosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;

    [Space(10)]
    [Header("Factions")]
    [SerializeField] private GameObject factionItemPrefab;

    [Space(10)]
    [Header("Locations")]
    [SerializeField] private GameObject locationItemPrefab;

    [Space(10)]
    [Header("Special Tokens")]
    [SerializeField] private GameObject specialTokenPrefab;

    private Dictionary<Character, CharacterTokenItem> characterEntries;
    private Dictionary<Faction, FactionTokenItem> factionItems;
    private Dictionary<Area, LocationTokenItem> areaItems;
    private Dictionary<SpecialToken, SpecialTokenItem> specialTokenItems;
    private List<CharacterTokenItem> _activeCharacterEntries;


    #region getters/setters
    public List<CharacterTokenItem> activeCharacterEntries {
        get { return _activeCharacterEntries; }
    }
    #endregion

    internal override void Initialize() {
        base.Initialize();
        _activeCharacterEntries = new List<CharacterTokenItem>();
        characterEntries = new Dictionary<Character, CharacterTokenItem>();
        Messenger.AddListener<Character>(Signals.CHARACTER_CREATED, AddCharacterEntry);
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, RemoveCharacterEntry);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, UpdateCharacterEntry);
        //Messenger.AddListener<Character>(Signals.ROLE_CHANGED, UpdateCharacterEntry);
        //Messenger.AddListener<Character>(Signals.FACTION_SET, UpdateCharacterEntry);
        
        //Faction
        Messenger.AddListener<Faction>(Signals.FACTION_CREATED, OnFactionCreated);
        Messenger.AddListener<Faction>(Signals.FACTION_DELETED, OnFactionDeleted);
        factionItems = new Dictionary<Faction, FactionTokenItem>();

        //Area
        Messenger.AddListener<Area>(Signals.AREA_CREATED, OnAreaCreated);
        Messenger.AddListener<Area>(Signals.AREA_DELETED, OnAreanDeleted);
        areaItems = new Dictionary<Area, LocationTokenItem>();

        //special tokens
        Messenger.AddListener<SpecialToken>(Signals.SPECIAL_TOKEN_CREATED, OnSpecialTokenCreated);
        specialTokenItems = new Dictionary<SpecialToken, SpecialTokenItem>();

        //Shared
        Messenger.AddListener<Token>(Signals.TOKEN_ADDED, OnTokenAdded);
        Messenger.AddListener<Token>(Signals.TOKEN_CONSUMED, OnTokenConsumed);
    }

    public override void OpenMenu() {
        //base.OpenMenu();
        isShowing = true;
    }
    public override void CloseMenu() {
        //base.CloseMenu();
        isShowing = false;
    }

    #region Character Tokens
    private void AddCharacterEntry(Character character) {
        if (character.role != null && character.role.roleType == CHARACTER_ROLE.PLAYER) {
            return;
        }
        GameObject newEntryGO = UIManager.Instance.InstantiateUIObject(characterEntryPrefab.name, tokensScrollView.content);
        newEntryGO.transform.localScale = Vector3.one;
        CharacterTokenItem newEntry = newEntryGO.GetComponent<CharacterTokenItem>();
        //newEntry.SetCharacter(character.characterToken);
        newEntry.Initialize();
        newEntry.gameObject.SetActive(false);
        characterEntries.Add(character, newEntry);
    }
    private void RemoveCharacterEntry(Character character) {
        CharacterTokenItem characterEntry = GetCharacterEntry(character);
        if (characterEntry != null) {
            characterEntries.Remove(character);
            ObjectPoolManager.Instance.DestroyObject(characterEntry.gameObject);
            //UpdateListColors();
        }
        //if (this.isShowing) {
        //    StartCoroutine(ExecuteLayoutGroup());
        //}
        //sortingAction();
    }
    private CharacterTokenItem GetCharacterEntry(Character character) {
        if (characterEntries.ContainsKey(character)) {
            return characterEntries[character];
        }
        return null;
    }
    private void UpdateCharacterEntry(Character character) {
        CharacterTokenItem charEntry = GetCharacterEntry(character);
        if (charEntry != null) {
            charEntry.UpdateCharacterInfo();
        }
        //sortingAction();
    }
    #endregion

    #region Faction Tokens
    private void OnFactionCreated(Faction createdFaction) {
        GameObject factionItemGO = UIManager.Instance.InstantiateUIObject(factionItemPrefab.name, tokensScrollView.content);
        FactionTokenItem factionItem = factionItemGO.GetComponent<FactionTokenItem>();
        factionItem.SetFactionToken(createdFaction.factionToken);
        factionItem.gameObject.SetActive(false);
        factionItems.Add(createdFaction, factionItem);
        //UpdateColors();
    }
    private void OnFactionDeleted(Faction deletedFaction) {
        if (factionItems.ContainsKey(deletedFaction)) {
            ObjectPoolManager.Instance.DestroyObject(factionItems[deletedFaction].gameObject);
            factionItems.Remove(deletedFaction);
            //UpdateColors();
        }
    }
    private FactionTokenItem GetItem(Faction faction) {
        if (factionItems.ContainsKey(faction)) {
            return factionItems[faction];
        }
        return null;
    }
    #endregion

    #region Area Tokens
    private void OnAreaCreated(Area createdArea) {
        GameObject locationItemGO = UIManager.Instance.InstantiateUIObject(locationItemPrefab.name, tokensScrollView.content);
        LocationTokenItem locationItem = locationItemGO.GetComponent<LocationTokenItem>();
        locationItem.SetLocation(createdArea.locationToken);
        locationItem.gameObject.SetActive(false);
        areaItems.Add(createdArea, locationItem);
    }
    private void OnAreanDeleted(Area deletedArea) {
        if (areaItems.ContainsKey(deletedArea)) {
            ObjectPoolManager.Instance.DestroyObject(areaItems[deletedArea].gameObject);
            areaItems.Remove(deletedArea);
        }
    }
    private LocationTokenItem GetItem(Area area) {
        if (areaItems.ContainsKey(area)) {
            return areaItems[area];
        }
        return null;
    }
    #endregion

    #region Special Tokens
    private void OnSpecialTokenCreated(SpecialToken token) {
        GameObject tokenGO = UIManager.Instance.InstantiateUIObject(specialTokenPrefab.name, tokensScrollView.content);
        SpecialTokenItem tokenItem = tokenGO.GetComponent<SpecialTokenItem>();
        tokenItem.SetToken(token);
        tokenItem.gameObject.SetActive(false);
        specialTokenItems.Add(token, tokenItem);
    }
    private SpecialTokenItem GetItem(SpecialToken token) {
        if (specialTokenItems.ContainsKey(token)) {
            return specialTokenItems[token];
        }
        return null;
    }
    #endregion


    //private void OnInteractionMenuOpened() {
    //    if (this.isShowing) {
    //        //if the menu is showing update it's open position
    //        //only open halfway
    //        tweener.SetAnimationPosition(openPosition, halfPosition, curve, curve);
    //        tweener.ChangeSetState(false);
    //        tweener.TriggerOpenClose();
    //        tweener.SetAnimationPosition(closePosition, halfPosition, curve, curve);
    //    } else {
    //        //only open halfway
    //        tweener.SetAnimationPosition(closePosition, halfPosition, curve, curve);
    //    }
    //}
    //private void OnInteractionMenuClosed() {
    //    if (this.isShowing) {
    //        tweener.SetAnimationPosition(halfPosition, openPosition, curve, curve);
    //        tweener.ChangeSetState(false);
    //        tweener.TriggerOpenClose();
    //        tweener.SetAnimationPosition(closePosition, openPosition, curve, curve);
    //    } else {
    //        //reset positions to normal
    //        tweener.SetAnimationPosition(closePosition, openPosition, curve, curve);
    //    }
    //}
    private void OnTokenAdded(Token token) {
        if (token is CharacterToken) {
            CharacterToken charToken = (token as CharacterToken);
            CharacterTokenItem item = GetCharacterEntry(charToken.character);
            if (item != null) {
                item.gameObject.SetActive(true);
                item.SetDraggable(true);
                _activeCharacterEntries.Add(item);
            }
        } else if (token is FactionToken) {
            FactionTokenItem item = GetItem((token as FactionToken).faction);
            if (item != null) {
                item.gameObject.SetActive(true);
            }
        } else if (token is LocationToken) {
            LocationTokenItem item = GetItem((token as LocationToken).location);
            if (item != null) {
                item.gameObject.SetActive(true);
            }
        } else if (token is SpecialToken) {
            SpecialTokenItem item = GetItem(token as SpecialToken);
            if (item != null) {
                item.gameObject.SetActive(true);
            }
        }
    }
    private void OnTokenConsumed(Token token) {
        if (token is CharacterToken) {
            CharacterToken charToken = (token as CharacterToken);
            CharacterTokenItem item = GetCharacterEntry(charToken.character);
            if (item != null) {
                item.gameObject.SetActive(false);
                item.SetDraggable(false);
                _activeCharacterEntries.Remove(item);
            }
        } else if (token is FactionToken) {
            FactionTokenItem item = GetItem((token as FactionToken).faction);
            if (item != null) {
                item.gameObject.SetActive(false);
            }
        } else if (token is LocationToken) {
            LocationTokenItem item = GetItem((token as LocationToken).location);
            if (item != null) {
                item.gameObject.SetActive(false);
            }
        } else if (token is SpecialToken) {
            SpecialTokenItem item = GetItem(token as SpecialToken);
            if (item != null) {
                item.gameObject.SetActive(false);
            }
        }
    }
}
