using BetterRomance;
using BetterRomance.HarmonyPatches;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace NoneRomance
{
    [StaticConstructorOnStartup]
    public static class OnStartup
    {
        static OnStartup()
        {
            Harmony harmony = new(id: "rimworld.divineDerivative.NoneRomance");
            harmony.PatchAll();
            if (ModsConfig.IsActive("divineDerivative.Romance"))
            {
                NoneRomanceMod.settings.WBRActive = true;
                harmony.PatchWBR();
            }
            if (ModsConfig.BiotechActive)
            {
                NoneRomanceMod.settings.BiotechActive = true;
            }
        }
    }

    [HarmonyPatch]
    public static class CorePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SocialCardUtility), "CanDrawTryRomance")]
        public static void CanDrawTryRomancePostfix(ref bool __result)
        {
            if (__result && NoneRomanceMod.settings.hideButton)
            {
                __result = false;
            }
        }

#if v1_6
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FloatMenuOptionProvider_Romance), "AppliesInt")]
        public static void Postfix(ref bool __result)
        {
            if (__result && NoneRomanceMod.settings.hideMenu)
            {
                __result = false;
            }
        }
#else
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        public static IEnumerable<CodeInstruction> AddHumanlikeOrdersTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo Drafted = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Drafted));
            MethodInfo BiotechActive = AccessTools.PropertyGetter(typeof(ModsConfig), nameof(ModsConfig.BiotechActive));

            Label jumpLabel = new();
            bool labelFound = false;
            List<CodeInstruction> codes = new(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].Calls(Drafted) && codes[i + 1].opcode == OpCodes.Brtrue && codes[i + 2].Calls(BiotechActive))
                {
                    jumpLabel = (Label)codes[i + 1].operand;
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
#endif
    }

    public static class WBRPatches
    {
        public static void CanDrawTryHookupPostfix(ref bool __result)
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
            harmony.Patch(AccessTools.Method(typeof(HookupUtility), nameof(HookupUtility.CanDrawTryHookup)), postfix: new HarmonyMethod(typeof(WBRPatches), nameof(CanDrawTryHookupPostfix)));
#if !v1_6
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap_AddHumanlikeOrders), nameof(FloatMenuMakerMap_AddHumanlikeOrders.Postfix)), prefix: new HarmonyMethod(typeof(WBRPatches), nameof(AddHumanlikeOrdersPrefix)));
#else
            harmony.Patch(AccessTools.Method(typeof(FloatMenuOptionProvider_Hookup), nameof(FloatMenuOptionProvider_Hookup.Applies)), prefix: new HarmonyMethod(typeof(WBRPatches), nameof(AddHumanlikeOrdersPrefix)));
#endif
        }
    }
}
