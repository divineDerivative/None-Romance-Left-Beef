using DivineFramework;
using DivineFramework.UI;
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
        internal SettingsHandler<Settings> handler = new();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hideButton, "hideRomanceButton", true);
            Scribe_Values.Look(ref hideMenu, "hideRomanceMenuOption", true);
            Scribe_Values.Look(ref WBRHideButton, "WBRHideButton", true);
            Scribe_Values.Look(ref WBRHideMenu, "WBRHideMenu", true);
        }

        internal void SetUpHandler()
        {
            if (BiotechActive)
            {
                handler.RegisterNewRow("RomanceHeader").AddLabel("NRLB.RomanceHeader".Translate);

                UIRow romanceButtonRow = handler.RegisterNewRow("RomanceButton");
                romanceButtonRow.AddSpace(relative: 0.05f);
                romanceButtonRow.Add(NewElement.Checkbox(relative: 0.5f)
                    .WithLabel("NRLB.RomanceButton".Translate)
                    .WithTooltip("NRLB.RomanceButtonTooltip".Translate)
                    .WithReference(this, nameof(hideButton), hideButton));

                UIRow romanceMenuRow = handler.RegisterNewRow("RomanceMenu");
                romanceMenuRow.AddSpace(relative: 0.05f);
                romanceMenuRow.Add(NewElement.Checkbox(relative: 0.5f)
                    .WithLabel("NRLB.RomanceMenu".Translate)
                    .WithTooltip("NRLB.RomanceMenuTooltip".Translate)
                    .WithReference(this, nameof(hideMenu), hideMenu));
            }
            handler.RegisterNewRow("Gap").AddLine(relative: 0.58f).HideWhen(() => !BiotechActive || !WBRActive);

            if (WBRActive)
            {
                handler.RegisterNewRow("HookupHeader").AddLabel("NRLB.HookupHeader".Translate);

                UIRow hookupButtonRow = handler.RegisterNewRow("HookupButton");
                hookupButtonRow.AddSpace(relative: 0.05f);
                hookupButtonRow.Add(NewElement.Checkbox(relative: 0.5f)
                    .WithLabel("NRLB.HookupButton".Translate)
                    .WithTooltip("NRLB.HookupButtonTooltip".Translate)
                    .WithReference(this, nameof(WBRHideButton), WBRHideButton));

                UIRow hookupMenuRow = handler.RegisterNewRow("HookupMenu");
                hookupMenuRow.AddSpace(relative: 0.05f);
                hookupMenuRow.Add(NewElement.Checkbox(relative: 0.5f)
                    .WithLabel("NRLB.HookupMenu".Translate)
                    .WithTooltip("NRLB.HookupMenuTooltip".Translate)
                    .WithReference(this, nameof(WBRHideMenu), WBRHideMenu));
            }

            handler.Initialize();
        }
    }

    public class NoneRomanceMod : Mod
    {
        public static Settings settings;

        public NoneRomanceMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
            ModManagement.RegisterMod("NRLB.ModTitle", typeof(NoneRomanceMod).Assembly.GetName().Name, new(FrameworkVersionInfo.Version), "[NoneRomance]", () => true);
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

            if (!settings.BiotechActive && !settings.WBRActive)
            {
                list.Label("NRLB.DontHaveRequirements".Translate());
                list.End();
                return;
            }

            if (!settings.handler.Initialized)
            {
                settings.handler.width = list.ColumnWidth;
                settings.SetUpHandler();
            }

            settings.handler.Draw(list);

            list.End();
        }
    }
}
