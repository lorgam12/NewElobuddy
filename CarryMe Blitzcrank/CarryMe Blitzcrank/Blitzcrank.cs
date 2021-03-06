﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CarryMe_Blitzcrank.Basics;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Notifications;
using SpellData = CarryMe_Blitzcrank.Basics.SpellData;

namespace CarryMe_Blitzcrank
{
	static class Blitzcrank
	{
		internal static MenuBuilder Config;
		public static int NotifieDelay;

		internal static double QHitCount;
		internal static double QTotalCastCount;

		internal static Spell.Skillshot Q = new Spell.Skillshot(SpellSlot.Q, 1025, SkillShotType.Linear, 250, 1800, 70);
		public static Spell.Active W = new Spell.Active(SpellSlot.W);
		public static Spell.Active E = new Spell.Active(SpellSlot.E);
		public static Spell.Active R = new Spell.Active(SpellSlot.R, 600);

		public static Obj_AI_Base QGlobalTarget;
		public static Prodiction.ProdictResult QGlobalPred;

		
		internal static void Load()
		{
			Config = Blitzcrank_Menu.Load();

			Game.OnUpdate += OnUpdate;
			Game.OnTick += OnTick;
			Drawing.OnDraw += OnDraw;
			Obj_AI_Base.OnBuffGain += Unit_OnGainBuff;
			Obj_AI_Base.OnBuffLose += Unit_OnLoseBuff;
			Obj_AI_Base.OnSpellCast += Unit_OnSpellCast;
			Orbwalker.OnPostAttack += OnAfterAttack;
		}

		private static void OnDraw(EventArgs args)
		{
			if (Config.IsChecked(MenuBuilder.MenuNames.Drawing, "draw.Q.range"))
			{
				var circle = new Geometry.Polygon.Circle(ObjectManager.Player.Position, Q.Range, 50);
				circle.Draw(Q.IsReady() ? Color.Green : Color.Brown);
			}

			if (Config.IsChecked(MenuBuilder.MenuNames.Drawing, "draw.Q.prediction"))
			{
				var  target = TargetSelector.GetTarget(ObjectManager.Get<AIHeroClient>().Where(u => u.IsEnemy && !u.IsDead && u.IsValidTarget(Q.Range) && !Config.IsChecked(MenuBuilder.MenuNames.BlackList, u.ChampionName)), DamageType.Magical);
				if (target != null)
				{
					var pred = Q.GetProdiction(target);
					var rect = new Geometry.Polygon.Rectangle(ObjectManager.Player.Position, pred.CastPosition, Q.Width);
					rect.Draw((pred.Hitchance >= HitChance.High && Q.IsReady()) ? Color.Blue : Color.Brown);
					Drawing.DrawText(pred.CastPosition.WorldToScreen(), Color.Wheat, pred.Hitchance.ToString(), 2);
				}			
			}
		}

		private static void OnUpdate(EventArgs args)
		{
			if (Config.IsChecked(MenuBuilder.MenuNames.Anti_FPS_Drop, "antiFPS.useOnTick"))
				return;
			OnLogicRun();
		}
		private static void OnTick(EventArgs args)
		{
			if (!Config.IsChecked(MenuBuilder.MenuNames.Anti_FPS_Drop, "antiFPS.useOnTick"))
				return;
			if (NotifieDelay + 15000 < Core.GameTickCount && Game.FPS > 100)
			{
				Chat.Print("CarryMe BlitzCrank: Please use framelock 60 fps");
				Chat.Print("CarryMe BlitzCrank: You find it in League Settings.");
				NotifieDelay = Core.GameTickCount;
			}
			OnLogicRun();
		}

		private static void OnLogicRun()
		{
			if (ObjectManager.Player.IsDead)
				return;
			KillstealCheck();

			if (R.IsReady() && Config.GetValue(MenuBuilder.MenuNames.Misc, "autoassist.alwaysUseRCount") <= EntityManager.Heroes.Enemies.Count(u => !u.IsDead && u.IsValidTarget(R.Range)))
			{
				var notifie = new SimpleNotification("R AlwaysCast -> Cast R",
								"Casting R couse i can hit " + EntityManager.Heroes.Enemies.Count(u => !u.IsDead && u.IsValidTarget(R.Range)) + " Enemys.");
				Notifications.Show(notifie);
				R.Cast();
			}

			if (E.IsReady() && (Config.IsChecked(Orbwalker.ActiveModes.Combo, "use.E.inrage") || Config.IsChecked(Orbwalker.ActiveModes.Harass, "use.E.inrage")))
			{
				if (EntityManager.Heroes.Enemies.Any(u => !u.IsDead && u.IsValidTarget(ObjectManager.Player.GetAutoAttackRange(u) - 50)))
				{
					E.Cast();
				}
			}

			if (R.IsReady() && Config.IsChecked(MenuBuilder.MenuNames.Misc, "use.R.autoAssist"))
			{
				foreach (var target in EntityManager.Heroes.Enemies.Where(u => !u.IsDead && u.IsValidTarget(R.Range)))
				{
					var target_safecast = target;
					foreach (
						var buddy in
							EntityManager.Heroes.Allies.Where(
								u =>
									!u.IsMe && !u.IsDead &&
									u.Distance(target_safecast) <=
									u.GetAutoAttackRange(target_safecast) + Config.GetValue(MenuBuilder.MenuNames.Misc, "additional.buddyrange")))
					{
						var Attackdamage = 0f;
						var attacks = Config.GetValue(MenuBuilder.MenuNames.Misc, "use.R.onAAleft");
						for (var i = 1; i <= attacks; ++i)
							Attackdamage += buddy.GetAutoAttackDamage(target, i == 1);
						if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) + Attackdamage >= target.Health &&
							ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) < target.Health)
						{
							if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Config.GetValue(MenuBuilder.MenuNames.Misc, "autoassist.HitCountInCombo") <= EntityManager.Heroes.Enemies.Count(u => !u.IsDead && u.IsValidTarget(R.Range))) ||
								(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Config.GetValue(MenuBuilder.MenuNames.Misc, "autoassist.HitCountInHarras") <= EntityManager.Heroes.Enemies.Count(u => !u.IsDead && u.IsValidTarget(R.Range))))
							{
								var notifie = new SimpleNotification("R Assister -> Cast R",
								"My Buddy need just to do " +
								((int)(target.Health - ObjectManager.Player.GetSpellDamage(target, SpellSlot.R)) + "Damage"));
								Notifications.Show(notifie);
								R.Cast();
								return;
							}
						}
					}
				}
			}
			if (Q.IsReady())
			{
				QGlobalTarget = TargetSelector.GetTarget(ObjectManager.Get<AIHeroClient>().Where(u => u.IsEnemy && !u.IsDead && u.IsValidTarget(Q.Range) && !Config.IsChecked(MenuBuilder.MenuNames.BlackList, u.ChampionName)), DamageType.Magical);
				if (QGlobalTarget != null)
				{
					QGlobalPred = Q.GetProdiction(QGlobalTarget);
					if (Config.IsChecked(Orbwalker.ActiveModes.Combo, "use.Q"))
					{
						if (QGlobalTarget.IsWithinDistance(Config.GetValue(Orbwalker.ActiveModes.Combo, "modiefied.Q.range.min"), Config.GetValue(Orbwalker.ActiveModes.Combo, "modiefied.Q.range.max")))
						{
							if (QGlobalPred.Hitchance >= HitChance.High)
								Q.Cast(QGlobalPred.CastPosition);
						}
					}

					if (Config.IsChecked(Orbwalker.ActiveModes.Harass, "use.Q"))
					{
						if (QGlobalTarget.IsWithinDistance(Config.GetValue(Orbwalker.ActiveModes.Harass, "modiefied.Q.range.min"), Config.GetValue(Orbwalker.ActiveModes.Harass, "modiefied.Q.range.max")))
						{
							if (QGlobalPred.Hitchance >= HitChance.High)
								Q.Cast(QGlobalPred.CastPosition);
						}
					}
				}
				else
				{
					QGlobalPred = null;
				}
			}

		}
		private static void KillstealCheck()
		{
			var combos = GetAllPossibleCombos(new[] { "Q", "E", "R" });
			var availableCombos = new List<ComboData>();
			foreach (var unit in EntityManager.Heroes.Enemies.Where(u => !u.IsDead && u.IsValidTarget(1500)))
			{
				foreach (var combo in combos)
				{
					var manacost = 0;
					var damage = 0f;
					var canuse = false;
					foreach (var spell in combo)
					{
						switch (spell)
						{
							case "Q":
								manacost += SpellData.Mana.Blitzcrank.Q;
								damage += ObjectManager.Player.GetSpellDamage(unit, SpellSlot.Q);
								canuse = Config.IsChecked(MenuBuilder.MenuNames.Killsteal, "use.Q") && Q.IsReady();
								break;
							case "E":
								manacost += SpellData.Mana.Blitzcrank.E;
								damage += ObjectManager.Player.GetSpellDamage(unit, SpellSlot.E);
								canuse = Config.IsChecked(MenuBuilder.MenuNames.Killsteal, "use.E") && E.IsReady();
								break;
							case "R":
								manacost += SpellData.Mana.Blitzcrank.R;
								damage += ObjectManager.Player.GetSpellDamage(unit, SpellSlot.R);
								canuse = Config.IsChecked(MenuBuilder.MenuNames.Killsteal, "use.R") && R.IsReady();
								break;
						}
						damage -= -unit.AllShield;
					}
					if (unit.Health <= damage && ObjectManager.Player.Mana >= manacost && canuse)
						availableCombos.Add(new ComboData(combo,damage));

				}
				foreach (var spell in availableCombos.OrderBy(u=> u.Damage).SelectMany(availablecombo => availablecombo.Combo))
				{
					switch (spell)
					{
						case "Q":
							if (unit.IsValidTarget(Q.Range))
							{
								var pred = Q.GetProdiction(unit);
								if (pred.Hitchance >= HitChance.High)
								{
									Q.Cast(pred.CastPosition);
									return;
								}
							}
							break;
						case "E":
							if (unit.IsValidTarget(ObjectManager.Player.GetAutoAttackRange(unit)))
							{
								E.Cast();
								Orbwalker.ResetAutoAttack();
								var unit_safecast = unit;
								Core.DelayAction(() => Player.IssueOrder(GameObjectOrder.AttackUnit, unit_safecast), 250);
								return;
							}
							break;
						case "R":
							if (unit.IsValidTarget(R.Range) )
							{
								R.Cast();
								return;
							}
							break;
					}
				}
			}
		}

		public class ComboData
		{
			public List<string> Combo;
			public float Damage;

			public ComboData(List<string> combo, float damage)
			{
				Combo = combo;
				Damage = damage;
			}
		
		}
		public static List<List<string>> GetAllPossibleCombos<T>(IEnumerable<T> elements)
		{
			var list = Combinations(elements, 3);
			var retlist = new List<List<string>>();
			foreach (var combo in list)
			{
				var spelllist = new List<string>();
				foreach (var spell in combo)
				{
					if (!spelllist.Contains(spell.ToString()))
						spelllist.Add(spell.ToString());
				}
				if (!retlist.Contains(spelllist))
					retlist.Add(spelllist);
			}
			return retlist;
		}
		public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)
		{
			return k == 0 ? new[] { new T[0] } :
			  elements.SelectMany((e, i) =>
				elements.Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
		}

		private static void OnAfterAttack(AttackableUnit target, EventArgs args)
		{
			if (E.IsReady() && (Config.IsChecked(Orbwalker.ActiveModes.Combo, "use.E.afterAA") || Config.IsChecked(Orbwalker.ActiveModes.Harass, "use.E.afterAA")))
			{
				var LastTarget = ObjectManager.Get<AIHeroClient>().FirstOrDefault(u => u.NetworkId == target.NetworkId && !u.IsDead);
				if (LastTarget != null)
				{
					E.Cast();
					Core.DelayAction(() => Player.IssueOrder(GameObjectOrder.AttackUnit, LastTarget), 100);
				}
			}
		}

		private static void Unit_OnGainBuff(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
		{
			if (!sender.IsEnemy || sender.IsMinion || sender.IsMonster || !args.Buff.Caster.IsMe ||
				args.Buff.Name != "rocketgrab2") return;
			++QHitCount;
			var notifie = new SimpleNotification("I hit allready " + Misc.ToWord((int)QHitCount) + " Q´s",
				"thats " + Math.Round(QHitCount /QTotalCastCount * 100) + "% of Total: " + QTotalCastCount);
			Notifications.Show(notifie);
			if (!E.IsReady() && (Config.IsChecked(Orbwalker.ActiveModes.Combo, "use.E") ||
				Config.IsChecked(Orbwalker.ActiveModes.Harass, "use.E") ||
				Config.IsChecked(MenuBuilder.MenuNames.Misc, "use.E")))
				return;
			E.Cast();
		}
		private static void Unit_OnLoseBuff(Obj_AI_Base sender, Obj_AI_BaseBuffLoseEventArgs args)
		{
			if (Config.IsChecked(Orbwalker.ActiveModes.Combo, "use.E") ||
				Config.IsChecked(Orbwalker.ActiveModes.Harass, "use.E") ||
				Config.IsChecked(MenuBuilder.MenuNames.Misc, "use.E"))
				return;
			if (!ObjectManager.Player.IsInAutoAttackRange(sender))
				return;
			if (!sender.IsDead && sender.IsEnemy && !sender.IsMinion && !sender.IsMonster && args.Buff.Caster.IsMe && args.Buff.Name == "rocketgrab2")
			{
				Core.DelayAction(() => Player.IssueOrder(GameObjectOrder.AttackUnit, sender), 100);
			}
		}

		private static void Unit_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
		{
			if (sender.IsMe && args.Slot == SpellSlot.Q)
			{
				++QTotalCastCount;
			}

		}
	}
}
