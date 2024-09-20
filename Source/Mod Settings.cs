using UnityEngine;
using Verse;

namespace NoneRomance
{
    public class Settings : ModSettings
    {
        public bool hideButton = true;
        public bool hideMenu = true;
        public bool WBRHideButton = true;
        public bool WBRHideMenu = true;
        internal bool BiotechActive;
        internal bool WBRActive;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hideButton, "hideRomanceButton", true);
            Scribe_Values.Look(ref hideMenu, "hideRomanceMenuOption", true);
            Scribe_Values.Look(ref WBRHideButton, "WBRHideButton", true);
            Scribe_Values.Look(ref WBRHideMenu, "WBRHideMenu", true);
            base.ExposeData();
        }
    }

    public class NoneRomanceMod : Mod
    {
        public static Settings settings;

        public NoneRomanceMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "NRLB.ModTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Listing_Standard list = new()
            {
                ColumnWidth = (canvas.width / 2f) - 17f
            };
            list.Begin(canvas);
            if (settings.BiotechActive)
            {
                list.Label("NRLB.RomanceHeader".Translate());
                CheckboxLabledTabAndTooltip(list, "NRLB.RomanceButton".Translate(), ref settings.hideButton, 25f, "NRLB.RomanceButtonTooltip".Translate());
                CheckboxLabledTabAndTooltip(list, "NRLB.RomanceMenu".Translate(), ref settings.hideMenu, 25f, "NRLB.RomanceMenuTooltip".Translate());
                list.GapLine();
            }

            if (settings.WBRActive)
            {
                list.Label("NRLB.HookupHeader".Translate());
                CheckboxLabledTabAndTooltip(list, "NRLB.HookupButton".Translate(), ref settings.WBRHideButton, 25f, "NRLB.HookupButtonTooltip".Translate());
                CheckboxLabledTabAndTooltip(list, "NRLB.HookupMenu".Translate(), ref settings.WBRHideMenu, 25f, "NRLB.HookupMenuTooltip".Translate());
            }

            if (!settings.BiotechActive && !settings.WBRActive)
            {
                list.Label("NRLB.DontHaveRequirements".Translate());
            }
            list.End();
        }

        private void CheckboxLabledTabAndTooltip(Listing_Standard list, string label, ref bool checkOn, float tabIn, string tooltip)
        {
            float height = Text.CalcHeight(label, list.ColumnWidth);
            Rect rect = list.GetRect(height);
            rect.xMin += tabIn;
            if (!list.BoundingRectCached.HasValue || rect.Overlaps(list.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(rect))
                    {
                        Widgets.DrawHighlight(rect);
                    }
                    TooltipHandler.TipRegion(rect, tooltip);
                }
                Widgets.CheckboxLabeled(rect, label, ref checkOn);
            }
            list.Gap(list.verticalSpacing);
        }
    }
}
