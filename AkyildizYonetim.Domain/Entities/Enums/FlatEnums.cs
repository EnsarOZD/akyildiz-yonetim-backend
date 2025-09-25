using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkyildizYonetim.Domain.Entities.Enums
{
	public class FlatEnums
	{
		public enum UnitType
		{
			Floor,   // Normal kat
			Entry,   // Giriş katı
			Parking  // OTOPARK (−3 & −4 tek unit)
		}

		public enum GroupStrategy
		{
			None,
			SplitIfMultiple // Grup içinde dolu sayısına böl (G ve 3. kat A/B)
		}
	}
}
