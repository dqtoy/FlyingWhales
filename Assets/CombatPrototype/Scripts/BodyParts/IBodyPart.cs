using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class IBodyPart: EntityComponent {
		public enum IMPORTANCE {
			ESSENTIAL,
			NON_ESSENTIAL,
		}

		public enum ATTRIBUTE {
			CLAWED,
			CAN_PUNCH,
			CAN_GRIP,
		}

		public enum STATUS {
			NORMAL,
			INJURED,
			DECAPITATED,
			BURNING,
			BLEEDING,
		}

		[SerializeField] internal BODY_PART bodyPart;
		[SerializeField] internal IMPORTANCE importance;
		[SerializeField] internal List<ATTRIBUTE> attributes;
		internal List<STATUS> status = new List<STATUS>();
		internal List<Item> itemsAttached = new List<Item>();
	}
}

