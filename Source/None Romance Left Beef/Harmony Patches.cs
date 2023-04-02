using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using BetterRomance;
using BetterRomance.HarmonyPatches;
using UnityEngine;

namespace NoneRomance
{
    [StaticConstructorOnStartup]
    public static class OnStartup
    {
        static OnStartup()
        {
            Harmony harmony = new Harmony(id: "rimworld.divineDerivative.NoneRomance");
            harmony.PatchAll();
            if (ModsConfig.IsActive("divineDerivative.Romance") || ModsConfig.IsActive("divineDerivative.RomanceDev"))
            {
                harmony.PatchWBR();
            }
        }
    }

    [HarmonyPatch]
    public static class CorePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SocialCardUtility), "CanDrawTryRomance")]
        public static void CanDrawTryRomancePostfix(Pawn pawn, ref bool __result)
        {
            if (__result && NoneRomanceMod.settings.hideButton)
            {
                __result = false;
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        public static IEnumerable<CodeInstruction> AddHumanlikeOrdersTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo Drafted = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Drafted));
            MethodInfo BiotechActive = AccessTools.PropertyGetter(typeof(ModsConfig), nameof(ModsConfig.BiotechActive));

            object jumpLabel = new object();
            bool labelFound = false;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].Calls(Drafted) && codes[i + 1].opcode == OpCodes.Brtrue && codes[i + 2].Calls(BiotechActive))
                {
                    jumpLabel = codes[i + 1].operand;
                    labelFound = true;
                }
                if (labelFound && codes[i].opcode == OpCodes.Brfalse && codes[i - 1].Calls(BiotechActive))
                {
                    yield return CodeInstruction.LoadField(typeof(NoneRomanceMod), nameof(NoneRomanceMod.settings));
                    yield return CodeInstruction.LoadField(typeof(Settings), nameof(Settings.hideMenu));
                    yield return new CodeInstruction(OpCodes.Brtrue, jumpLabel);
                }
            }
        }
    }

    public static class WBRPatches
    {
        public static void CanDrawTryHookupPostfix(Pawn pawn, ref bool __result)
        {
            if (__result && NoneRomanceMod.settings.WBRHideButton)
            {
                __result = false;
            }
        }

        public static bool AddHumanlikeOrdersPrefix()
        {
            return !NoneRomanceMod.settings.WBRHideMenu;
        }

        public static void PatchWBR(this Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(HookupUtility), nameof(HookupUtility.CanDrawTryHookup)), postfix: new HarmonyMethod(typeof(WBRPatches), nameof(WBRPatches.CanDrawTryHookupPostfix)));
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap_AddHumanlikeOrders), nameof(FloatMenuMakerMap_AddHumanlikeOrders.Postfix)), prefix: new HarmonyMethod(typeof(WBRPatches), nameof(WBRPatches.AddHumanlikeOrdersPrefix)));
        }
    }
}
