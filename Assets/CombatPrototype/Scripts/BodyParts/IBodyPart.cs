using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IBodyPart: EntityComponent {
	public enum IMPORTANCE {
		ESSENTIAL,
		NON_ESSENTIAL,
	}

	public enum ATTRIBUTE {
		CLAWED,
		CAN_PUNCH,
		CAN_GRIP,
        CAN_KICK,
        MAGICAL,
		CAN_SLASH,
		CAN_PIERCE,
		CAN_SHOOT,
        CAN_WHIP,
        CAN_EQUIP_TORSO_ARMOR,
        CAN_EQUIP_LEG_ARMOR,
        CAN_EQUIP_FOOT_ARMOR,
        CAN_EQUIP_ARM_ARMOR,
        CAN_EQUIP_HAND_ARMOR,
        CAN_EQUIP_HEAD_ARMOR,
        CAN_EQUIP_TAIL_ARMOR,
		NO_WEAPON,
		CAN_PUNCH_NO_WEAPON,
		CAN_KICK_NO_WEAPON,
		CAN_BITE_NO_WEAPON,
		CAN_WHIP_NO_WEAPON,
		CLAWED_NO_WEAPON,
		CAN_GRIP_NO_WEAPON,
		CAN_FLAME_BREATH_NO_WEAPON,
        NONE,
		CAN_EQUIP_HIP_ARMOR,
		NONDECAPITATABLE,
		CAN_CONFUSE,
		CAN_SLAM,
    }

	[SerializeField] internal string name;
//		[SerializeField] internal BODY_PART bodyPart;
	[SerializeField] internal IMPORTANCE importance;
	[SerializeField] internal List<BodyAttribute> attributes;
	internal List<STATUS_EFFECT> statusEffects = new List<STATUS_EFFECT>();
    internal List<Item> itemsAttached = new List<Item>();

    internal bool HasAttribute(ATTRIBUTE attribute) {
        for (int i = 0; i < attributes.Count; i++) {
            BodyAttribute currAttribute = attributes[i];
            if (currAttribute.attribute == attribute) {
				if(currAttribute.attribute.ToString().Contains("NO_WEAPON") && HasWeapon()){
					return false;
				}
				return true;
            }
        }
        return false;
    }

    internal bool HasUnusedAttribute(ATTRIBUTE attribute) {
        for (int i = 0; i < attributes.Count; i++) {
            BodyAttribute currAttribute = attributes[i];
            if (currAttribute.attribute == attribute && !currAttribute.isUsed) {
                return true;
            }
        }
        return false;
    }

    #region Items
    internal List<Item> GetAttachedItemsOfType(ITEM_TYPE itemType) {
        List<Item> items = new List<Item>();
        for (int i = 0; i < itemsAttached.Count; i++) {
            Item currItem = itemsAttached[i];
            if(currItem.itemType == itemType) {
                items.Add(currItem);
            }
        }
        return items;
    }

	internal bool HasWeapon(){
		for (int i = 0; i < itemsAttached.Count; i++) {
			Item currItem = itemsAttached[i];
			if(currItem.itemType == ITEM_TYPE.WEAPON) {
				return true;
			}
		}
		return false;
	}

	internal bool HasArmor(){
		for (int i = 0; i < itemsAttached.Count; i++) {
			Item currItem = itemsAttached[i];
			if(currItem.itemType == ITEM_TYPE.ARMOR) {
				return true;
			}
		}
		return false;
	}

	internal Armor GetArmor(){
		for (int i = 0; i < itemsAttached.Count; i++) {
			Item currItem = itemsAttached[i];
			if(currItem.itemType == ITEM_TYPE.ARMOR) {
				return (Armor)currItem;
			}
		}
		return null;
	}
    internal Weapon GetWeapon() {
        for (int i = 0; i < itemsAttached.Count; i++) {
            Item currItem = itemsAttached[i];
            if (currItem.itemType == ITEM_TYPE.WEAPON) {
                return (Weapon)currItem;
            }
        }
        return null;
    }
    #endregion

    #region Attributes
    internal BodyAttribute GetAttribute(ATTRIBUTE attribute, bool isUsed = false) {
        for (int i = 0; i < attributes.Count; i++) {
            BodyAttribute currAttribute = attributes[i];
            if(currAttribute.attribute == attribute && currAttribute.isUsed == isUsed) {
                return currAttribute;
            }
        }
        return null;
    }
    #endregion

	internal void AddStatusEffect(STATUS_EFFECT statusEffect){
		this.statusEffects.Add (statusEffect);
//			if(statusEffect == STATUS_EFFECT.DECAPITATED){
//				DropWeapons ();	
//			}
	}

	internal void RemoveStatusEffect(STATUS_EFFECT statusEffect){
		this.statusEffects.Remove (statusEffect);
	}
}