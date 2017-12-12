using UnityEngine;
using System.Collections;

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
	}
}

