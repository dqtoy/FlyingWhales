using UnityEngine;
using System.Collections;

public class FactionSummaryEntry : MonoBehaviour {

    private Faction _faction;

    [SerializeField] private UI2DSprite _bgSprite;

    [SerializeField] private UI2DSprite _emblemBG;
    [SerializeField] private UI2DSprite _emblemSprite;
    [SerializeField] private UI2DSprite _emblemOutline;

    [SerializeField] private UILabel villagesLbl;
    [SerializeField] private UILabel populationLbl;
    [SerializeField] private UILabel charactersLbl;
    
    public void SetFaction(Faction faction) {
        _faction = faction;

        ConstructEmblem();

        _bgSprite.color = faction.factionColor;

        villagesLbl.text = faction.settlements.Count.ToString();
        populationLbl.text = faction.totalPopulation.ToString();
        charactersLbl.text = faction.totalCharacters.ToString();
    }

    private void ConstructEmblem() {
        Color emblemShieldColor = _faction.factionColor;
        emblemShieldColor.a = 255f / 255f;
        _emblemBG.sprite2D = _faction.emblemBG;
        _emblemBG.color = emblemShieldColor;
        _emblemSprite.sprite2D = _faction.emblem;
        _emblemOutline.sprite2D = _faction.emblemBG;
        _emblemOutline.width = _emblemBG.width;
        Color outlineColor;
        ColorUtility.TryParseHtmlString("#2d2e2e", out outlineColor);
        _emblemOutline.color = outlineColor;
        _emblemOutline.width += 8;

        _emblemSprite.width = 17;
        _emblemSprite.height = 19;
    }

    public void CenterOnFaction() {
        CameraMove.Instance.CenterCameraOn(_faction.settlements[0].location.gameObject);
    }
}
