﻿namespace HorseOverhaul
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewModdingAPI.Utilities;
    using System;

    public interface IGenericModConfigMenuAPI
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

        void RegisterLabel(IManifest mod, string labelName, string labelDesc);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);

        void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);

        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc, Func<Vector2, object, object> widgetUpdate, Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave);
    }

    public enum SaddleBagOption
    {
        Disabled = 0,
        Green = 1,
        Brown = 2,
        Horsemanship_Brown = 3,
        Horsemanship_Beige = 4
    }

    /// <summary>
    /// Config file for the mod
    /// </summary>
    public class HorseConfig
    {
        public bool ThinHorse { get; set; } = true;

        public bool MovementSpeed { get; set; } = true;

        public float MaxMovementSpeedBonus { get; set; } = 3f;

        public bool SaddleBag { get; set; } = true;

        public string VisibleSaddleBags { get; set; } = SaddleBagOption.Green.ToString();

        public bool Petting { get; set; } = true;

        public bool Water { get; set; } = true;

        public bool Feeding { get; set; } = true;

        public bool PetFeeding { get; set; } = true;

        public KeybindList HorseMenuKey { get; set; } = KeybindList.Parse("H");

        public KeybindList PetMenuKey { get; set; } = KeybindList.Parse("P");

        public bool DisableStableSpriteChanges { get; set; } = false;

        public static void VerifyConfigValues(HorseConfig config, HorseOverhaul mod)
        {
            bool invalidConfig = false;

            if (config.MaxMovementSpeedBonus < 0f)
            {
                config.MaxMovementSpeedBonus = 0f;
                invalidConfig = true;
            }

            SaddleBagOption res;
            if (Enum.TryParse(config.VisibleSaddleBags, true, out res))
            {
                // reassign to ensure casing is correct
                config.VisibleSaddleBags = res.ToString();
            }
            else
            {
                config.VisibleSaddleBags = SaddleBagOption.Disabled.ToString();
                invalidConfig = true;
            }

            if (invalidConfig)
            {
                mod.DebugLog("At least one config value was out of range and was reset.");
                mod.Helper.WriteConfig(config);
            }
        }

        public static void SetUpModConfigMenu(HorseConfig config, HorseOverhaul mod)
        {
            IGenericModConfigMenuAPI api = mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");

            if (api == null)
            {
                return;
            }

            var manifest = mod.ModManifest;

            api.RegisterModConfig(manifest, () => config = new HorseConfig(), delegate { mod.Helper.WriteConfig(config); VerifyConfigValues(config, mod); });

            api.RegisterLabel(manifest, "General", null);

            api.RegisterSimpleOption(manifest, "Thin Horse", null, () => config.ThinHorse, (bool val) => config.ThinHorse = val);
            api.RegisterSimpleOption(manifest, "Saddle Bags", null, () => config.SaddleBag, (bool val) => config.SaddleBag = val);
            api.RegisterChoiceOption(manifest, "Visible Saddle Bags", null, () => config.VisibleSaddleBags.ToString(), (string val) => config.VisibleSaddleBags = val, Enum.GetNames(typeof(SaddleBagOption)));

            api.RegisterLabel(manifest, "Friendship", null);

            api.RegisterSimpleOption(manifest, "Movement Speed (MS)", null, () => config.MovementSpeed, (bool val) => config.MovementSpeed = val);
            api.RegisterSimpleOption(manifest, "Maximum MS Bonus", null, () => config.MaxMovementSpeedBonus, (float val) => config.MaxMovementSpeedBonus = val);
            api.RegisterSimpleOption(manifest, "Petting", null, () => config.Petting, (bool val) => config.Petting = val);
            api.RegisterSimpleOption(manifest, "Water", null, () => config.Water, (bool val) => config.Water = val);
            api.RegisterSimpleOption(manifest, "Feeding", null, () => config.Feeding, (bool val) => config.Feeding = val);

            api.RegisterLabel(manifest, "Other", null);

            api.RegisterSimpleOption(manifest, "Pet Feeding", null, () => config.PetFeeding, (bool val) => config.PetFeeding = val);
            api.RegisterSimpleOption(manifest, "Disable Stable Sprite Changes", null, () => config.DisableStableSpriteChanges, (bool val) => config.DisableStableSpriteChanges = val);

            // this is a spacer
            api.RegisterLabel(manifest, string.Empty, null);
            api.RegisterLabel(manifest, "(Menu Key Rebinding Only Available In Config File)", null);
        }
    }
}