using UnityEngine;
using System.Collections;

public class CharacterPortrait : MonoBehaviour {

	public delegate void OnClickCharacterPortrait(Citizen citizenClicked, CharacterPortrait portraitClicked);
	public OnClickCharacterPortrait onClickCharacterPortrait;

	public delegate void OnHoverCharacterPortrait ();
	public OnHoverCharacterPortrait onHoverCharacterPortrait;

	public delegate void OnHoverExitCharacterPortrait ();
	public OnHoverExitCharacterPortrait onHoverExitCharacterPortrait;

	public UI2DSprite kingdomColorGO;
	public GameObject isDeadIcon;
	public GameObject characterInfoGO;
	public UILabel characterNameLbl;
	public UILabel characterKingdomNameLbl;
	public GameObject relationshipLine;
	public GameObject[] relationshipCircles;
	public GameObject lineGO;
	public GameObject flagGO;
	public UI2DSprite flagSprite;

    [SerializeField] private GameObject militaryAllianceIcon;
    [SerializeField] private GameObject mutualDefenseIcon;

    [SerializeField] private UILabel kingdomThreatLvlLbl;
    [SerializeField] private UILabel invasionValueLbl;

    [Space(10)]
    [Header("Deatiled Character Info")]
    [SerializeField] private GameObject detailedCharacterInfoGO;
    [SerializeField] private UILabel detailedCharacterInfoNameLbl;
    [SerializeField] private UILabel detailedCharacterInfoOtherLbl;

    public Citizen citizen;
	private bool isHoverEnabled = true;
	private bool isHovering = false;

	public void SetCitizen(Citizen citizen, bool showFlag = false, bool showInfo = false, bool showDetailedInfo = false){
		this.citizen = citizen;
		if (this.citizen.city != null) {
			this.kingdomColorGO.color = this.citizen.city.kingdom.kingdomColor;
		} else {
			this.kingdomColorGO.color = Color.white;
		}
		if (citizen.isDead) {
			isDeadIcon.SetActive(true);
		} else {
			isDeadIcon.SetActive(false);
		}

		if (showFlag) {
			flagSprite.color = this.citizen.city.kingdom.kingdomColor;
			flagGO.SetActive(true);
		}

		if (showInfo) {
            HideDetailedCharacterInfo();
			ShowCitizenInfo();
		}

        if (showDetailedInfo) {
            HideCitizenInfo();
            ShowDetailedCharacterInfo();
        }


	}

	public void ShowCitizenInfo(){
		characterNameLbl.text = this.citizen.name;
		characterKingdomNameLbl.text = this.citizen.city.kingdom.name;
		characterInfoGO.SetActive(true);
        this.SetHoverEnabled(false);
	}

    private void HideCitizenInfo() {
        characterInfoGO.SetActive(false);
        this.SetHoverEnabled(true);
    }

    public void ShowDetailedCharacterInfo() {
        detailedCharacterInfoNameLbl.text = string.Empty;
        if(this.citizen.role != ROLE.UNTRAINED) {
            detailedCharacterInfoNameLbl.text += Utilities.NormalizeString(this.citizen.role.ToString()) + " ";
        }
        detailedCharacterInfoNameLbl.text += this.citizen.name;
        detailedCharacterInfoOtherLbl.text = this.citizen.age.ToString() + "\n";
        //detailedCharacterInfoOtherLbl.text = this.citizen.age.ToString() + "\n"
        //                                    + Utilities.NormalizeString(this.citizen.preferredKingdomType.ToString()) + "\n";
        if (this.citizen.role == ROLE.GOVERNOR) {
            detailedCharacterInfoOtherLbl.text += this.citizen.city.name;
        } else {
            detailedCharacterInfoOtherLbl.text += this.citizen.city.kingdom.name;
        }
                                            
        //"[b]Age: [/b]" + this.citizen.age.ToString() + "\n"
        //+ "[b]Kingdom Type: [/b]" + Utilities.NormalizeString(this.citizen.preferredKingdomType.ToString()) + "\n"
        //+ "[b]Kingdom: [/b]" + this.citizen.city.kingdom.name;
        detailedCharacterInfoGO.SetActive(true);
        this.SetHoverEnabled(false);
    }

    public void HideDetailedCharacterInfo() {
        detailedCharacterInfoGO.SetActive(false);
        this.SetHoverEnabled(true);
    }

    public void SetHoverEnabled(bool state){
		isHoverEnabled = state;
	}

	public void ShowRelationshipLine(KingdomRelationship relationship1 = null, KingdomRelationship relationship2 = null){
		relationshipLine.SetActive(true);
		if (relationship1 != null && relationship2 != null) {
			relationshipCircles[0].SetActive(true);
			relationshipCircles[1].SetActive(true);
			relationshipCircles[0].GetComponent<RelationshipItem>().SetRelationship(relationship1, true);
			relationshipCircles[1].GetComponent<RelationshipItem>().SetRelationship(relationship2, true);

            kingdomThreatLvlLbl.text = "KT: " + relationship1.targetKingdomThreatLevel.ToString();
            invasionValueLbl.text = "IV: " + relationship1.targetKingdomInvasionValue.ToString();
        }
        
	}

	void OnHover(bool isOver){
		if (!isHoverEnabled) {
			return;
		}
		if (isOver) {
			this.isHovering = true;
			if (citizen.city != null) {
				UIManager.Instance.ShowSmallInfo ("[b]" + citizen.name + "[/b]" + "\n" + "[i]" + citizen.city.kingdom.name + "[/i]");
			} else {
				UIManager.Instance.ShowSmallInfo ("[b]" + citizen.name + "[/b]" + "\n" + "[i] No Kingdom [/i]");
			}
			if (onHoverCharacterPortrait != null) {
				this.onHoverCharacterPortrait ();
			}
		} else {
			this.isHovering = false;
			UIManager.Instance.HideSmallInfo ();
			if (onHoverExitCharacterPortrait != null) {
				if (!UIManager.Instance.kingdomInfoGO.activeSelf) {
					this.onHoverExitCharacterPortrait ();
				} else {
					if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id != this.citizen.city.kingdom.id) {
						this.onHoverExitCharacterPortrait ();
					}
				}
			}
		}
	}

	public void OnClick(){
		if (onClickCharacterPortrait == null) {
//			if (citizen.father == null || citizen.mother == null) {
//				Debug.Log (citizen.name + " doesn't have a father or a mother, not showing info");
//				return;
//			}
			UIManager.Instance.ShowCitizenInfo (citizen);
		} else {
			onClickCharacterPortrait(this.citizen, this);
		}
	}

	void Update(){
		if (this.isHovering) {
			if (citizen.city != null) {
				UIManager.Instance.ShowSmallInfo ("[b]" + citizen.name + "[/b]" + "\n" + "[i]" + citizen.city.kingdom.name + "[/i]");
			} else {
				UIManager.Instance.ShowSmallInfo ("[b]" + citizen.name + "[/b]" + "\n" + "[i] No Kingdom [/i]");
			}
		}
	}
}
