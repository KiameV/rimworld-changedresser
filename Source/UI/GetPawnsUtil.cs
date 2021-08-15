using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser.UI
{
	public static class PawnUtil
	{
		public static string GetMelee(Pawn p) => ((p.WorkTagIsDisabled(WorkTags.Violent)) ? "-" : p.skills.GetSkill(SkillDefOf.Melee).levelInt.ToString());
		public static string GetRanged(Pawn p) => ((p.WorkTagIsDisabled(WorkTags.Violent)) ? "-" : p.skills.GetSkill(SkillDefOf.Shooting).levelInt.ToString());
		public static string GetLabelAndStatsFor(Pawn pawn)
		{
			var melee = (pawn.WorkTagIsDisabled(WorkTags.Violent) ? "-" : pawn.skills.GetSkill(SkillDefOf.Melee).levelInt.ToString());
			var ranged = (pawn.WorkTagIsDisabled(WorkTags.Violent) ? "-" : pawn.skills.GetSkill(SkillDefOf.Shooting).levelInt.ToString());
			return $"{pawn.Name.ToStringShort}\n  {SkillDefOf.Melee.label}: {melee}\n  {SkillDefOf.Shooting.label}: {ranged}";
		}
		public static List<Pawn> GetColonyPawns(bool includeDead = false)
		{
			SortedDictionary<string, List<Pawn>> pawns = new SortedDictionary<string, List<Pawn>>();
			foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
			{
				if (p == null || !p.def.race.Humanlike || p.IsQuestHelper() || p.IsQuestLodger() || p.apparel?.LockedApparel?.Count != 0)
					continue;
				//if (p.guest != null)
				//	continue;
				if (!includeDead && p.Dead)
					continue;

				string name = p.Name.ToStringShort;
				if (!pawns.TryGetValue(name, out List<Pawn> ps))
				{
					ps = new List<Pawn>();
					pawns[name] = ps;
				}
				ps.Add(p);
			}

			List<Pawn> result = new List<Pawn>();
			foreach (var l in pawns.Values)
				foreach (var p in l)
					result.Add(p);
			return result;
        }

        public static List<Pawn> GetPrisonerPawns()
        {
			return new List<Pawn>(PawnsFinder.AllMaps_PrisonersOfColony);
		}
    }
}
