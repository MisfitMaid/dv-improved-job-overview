using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using DV;
using DV.Booklets;
using DV.Logic.Job;
using DV.RenderTextureSystem.BookletRender;
using HarmonyLib;
using UnityModManagerNet;

namespace ImprovedJobOverview;

[EnableReloading]
public static class ImprovedJobOverview
{
	// Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
	private static bool Load(UnityModManager.ModEntry modEntry)
	{
		Harmony? harmony = null;

		try
		{
			harmony = new Harmony(modEntry.Info.Id);
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			// Other plugin startup logic
		}
		catch (Exception ex)
		{
			modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
			harmony?.UnpatchAll(modEntry.Info.Id);
			return false;
		}

		return true;
	}

	static bool Unload(UnityModManager.ModEntry modEntry)
	{
		modEntry.Logger.Log("Map restart still required to regenerate overview book textures.");
		Harmony harmony = new Harmony(modEntry.Info.Id);
		harmony.UnpatchAll();

		return true;
	}
}


[HarmonyPatch(typeof(BookletCreator_JobOverview), "GetJobOverviewTemplatePaperData", new Type[] { typeof(TransportJobData) })]
static class Patch_GetJobOverviewTemplatePaperDataTrans
{
	static void Postfix(object[] __args, ref List<TemplatePaperData> __result)
	{
		TransportJobData job = (TransportJobData)__args[0];
		List<TemplatePaperData> r = new List<TemplatePaperData>();
		foreach (TemplatePaperData tpd in __result)
		{
			FrontPageTemplatePaperData x = (FrontPageTemplatePaperData)tpd;
			x.startStationName = x.startStationName + " / " + job.startingTrack.TrackPartOnly;
			x.endStationName = x.endStationName + " / " + job.destinationTrack.TrackPartOnly;
			r.Add(x);
		}

		__result = r;
	}
}

[HarmonyPatch(typeof(BookletCreator_JobOverview), "GetJobOverviewTemplatePaperData", new Type[] { typeof(EmptyHaulJobData) })]
static class Patch_GetJobOverviewTemplatePaperDataLogi
{
	static void Postfix(object[] __args, ref List<TemplatePaperData> __result)
	{
		EmptyHaulJobData job = (EmptyHaulJobData)__args[0];
		List<TemplatePaperData> r = new List<TemplatePaperData>();
		foreach (TemplatePaperData tpd in __result)
		{
			FrontPageTemplatePaperData x = (FrontPageTemplatePaperData)tpd;
			x.startStationName = x.startStationName + " / " + job.startingTrack.TrackPartOnly;
			x.endStationName = x.endStationName + " / " + job.destinationTrack.TrackPartOnly;
			r.Add(x);
		}

		__result = r;
	}
}

[HarmonyPatch(typeof(BookletCreator_JobOverview), "GetJobOverviewTemplatePaperData", new Type[] { typeof(ShuntingLoadJobData) })]
static class Patch_GetJobOverviewTemplatePaperDataShuntLoad
{
	static void Postfix(object[] __args, ref List<TemplatePaperData> __result)
	{
		ShuntingLoadJobData job = (ShuntingLoadJobData)__args[0];
		List<TemplatePaperData> r = new List<TemplatePaperData>();
		foreach (TemplatePaperData tpd in __result)
		{
			FrontPageTemplatePaperData x = (FrontPageTemplatePaperData)tpd;
			List<string> tracks = new List<string>();
			foreach (CarDataPerTrackID d in job.startingTracksData)
			{
				tracks.Add(d.track.TrackPartOnly);
			}
			string track = tracks.Join<string>();

			x.jobDescription = x.jobDescription + "\n  [" + track + "] -> " + job.loadMachineTrack.TrackPartOnly + " -> " + job.destinationTrack.TrackPartOnly;
			r.Add(x);
		}

		__result = r;
	}
}

[HarmonyPatch(typeof(BookletCreator_JobOverview), "GetJobOverviewTemplatePaperData", new Type[] { typeof(ShuntingUnloadJobData) })]
static class Patch_GetJobOverviewTemplatePaperDataShuntUnload
{
	static void Postfix(object[] __args, ref List<TemplatePaperData> __result)
	{
		ShuntingUnloadJobData job = (ShuntingUnloadJobData)__args[0];
		List<TemplatePaperData> r = new List<TemplatePaperData>();
		foreach (TemplatePaperData tpd in __result)
		{
			FrontPageTemplatePaperData x = (FrontPageTemplatePaperData)tpd;
			List<string> tracks = new List<string>();
			foreach (CarDataPerTrackID d in job.destinationTracksData)
			{
				tracks.Add(d.track.TrackPartOnly);
			}
			string track = tracks.Join<string>();

			x.jobDescription = x.jobDescription + "\n  " + job.startingTrack.TrackPartOnly + " -> " + job.unloadMachineTrack.TrackPartOnly + " -> [" + track + "]";
			r.Add(x);
		}

		__result = r;
	}
}


