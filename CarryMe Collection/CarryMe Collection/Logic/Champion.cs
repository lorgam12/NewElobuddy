﻿using System.Collections.Generic;
using CarryMe_Collection.Basics;
using EloBuddy;
using EloBuddy.SDK.Menu;
using Newtonsoft.Json.Serialization;

namespace CarryMe_Collection.Logic
{
	public class Champion
	{
		public static Menu Config;

		internal virtual void WriteMenuStart()
		{
			Config = MainMenu.AddMenu("CM-" + Champions.Me.ChampionName, "cm." + Champions.Me.ChampionName, "CarryMe - " + Champions.Me.ChampionName);
			Config.AddLabel("Wellcome to the CarryMe Series by Lekks/Lexxes");

			var ChangeLog = new List<string>
			{
				"Releasedate: 27.03.2016 16:21",
				"[0] Added Features to DrMundo",
				"[1] He Lasthit with Q also in Lasthit Mode",
				"[1] He Cast Q (cause slow) to nearst Enemy in FleeMode",
			};
			Config.AddSpoiler("1.0.0.3", "Show ChangeLog 1.0.0.3", ChangeLog, true);
			Config.AddSeparator(1);

			if (Config.ShouldReadChangeLog("1.0.0.3"))
				Chat.Print("Lexxes: There is some New Stuff, pls read the Changelog inside the Menu :P");
			
			ChangeLog = new List<string>
			{
				"Releasedate: 26.03.2016 01:53",
				"[0] Drawing Change",
				"[1] Changed the Drawing Methode from Poligon to Rendering",
			};
			Config.AddSpoiler("1.0.0.2", "Show ChangeLog 1.0.0.2", ChangeLog, true);
			Config.AddSeparator(1);
			 ChangeLog = new List<string>
			{
				"Releasedate: 25.03.2016 20:45",
				"[0] Test If Install/Upload/Aktual work correctly",
			};
			Config.AddSpoiler("1.0.0.1", "Show ChangeLog 1.0.0.1", ChangeLog, true);
			Config.AddSeparator(1);
			 ChangeLog = new List<string>
			{
				"Releasedate: 25.03.2016 20:05",
				"[0] Beta Released",
				"[1] New Core Specific Stuff",
				"[2] Unique LineSpell Prodiction (Not Elobuddy Prediction)",
				"[2] Basic Collision ( Hero, Minion, Monster )",
				"[2] Added Menubuilder + Drawer + Diffrent Logics",

				"[1] New Added Stuff",
				"[2] Mundo Q with a Beautiful Prediction",
				"[2] Mundo W with Perfect Turn on/off Logic",
				"[2] Mundo E with Amazing AA-reset logic and Anti-Waste Prediction",
				"[2] Mundo R on Min-Health-Percent"
			};
			Config.AddSpoiler("1.0.0.0", "Show ChangeLog 1.0.0.0", ChangeLog,true);
			
		}

		internal virtual void WriteMenuEnd()
		{
			var anti_fps = Config.AddSubMenu(MenuBuilder.MenuName.Anti_FPS);
			anti_fps.AddCheckBox("use.OnTick","Use OnTick",false);
		}
		internal virtual void OnPassive()
		{
			
		}

		internal virtual void OnCombo()
		{
			
		}

		internal virtual void OnFlee()
		{

		}

		internal virtual void OnHarass()
		{

		}

		internal virtual void OnJungleClear()
		{

		}

		internal virtual void OnLaneClear()
		{

		}

		internal virtual void OnLastHit()
		{

		}

		internal virtual void OnNone()
		{

		}
		internal virtual void OnAfterAttack(AttackableUnit target, System.EventArgs args)
		{
			
		}
	}
}
