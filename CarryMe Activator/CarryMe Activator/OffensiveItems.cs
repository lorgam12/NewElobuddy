﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace CarryMe_Activator
{
	class OffensiveItems
	{
		public static Menu Menu;
		internal static void Load()
		{
			Menu = MainMenu.AddMenu("CM OffensiveItems", "cm.offitems", "CarryMe OffensiveItmes by Lekks.");
			Menu.Add("cm.offitems.deactivate", new CheckBox("Disable OffensiveItems Totally", false));

			new Items.Bilgewater_Cutlass();
			new Items.Blade_of_the_Ruined_King();
			new Items.Guardians_Horn();

		}

	
	}
}
