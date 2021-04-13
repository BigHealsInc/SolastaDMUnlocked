using System;
using System.Reflection;
using UnityModManagerNet;
using HarmonyLib;
using SolastaModApi;

namespace SolastaDMUnlocked
{
    public class Main
    {
        // [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string msg)
        {
            if (logger != null) logger.Log(msg);
        }

        public static void Error(Exception ex)
        {
            if (logger != null) logger.Error(ex.ToString());
        }

        public static void Error(string msg)
        {
            if (logger != null) logger.Error(msg);
        }

        public static UnityModManager.ModEntry.ModLogger logger;
        public static bool enabled;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;

                ModBeforeDBReady();

                var harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
            return true;
        }

        [HarmonyPatch(typeof(MainMenuScreen), "RuntimeLoaded")]
        static class MainMenuScreen_RuntimeLoaded_Patch
        {
            static void Postfix()
            {
                ModAfterDBReady();
            }
        }

        // ENTRY POINT IF YOU NEED SERVICE LOCATORS ACCESS
        static void ModBeforeDBReady()
        {
            
        }

        // ENTRY POINT IF YOU NEED SAFE DATABASE ACCESS
        static void ModAfterDBReady()
        {
            UnlockEnemies();
            UnlockTraps();
            UnlockItems();
            //UnlockLootPacks();
        }

        private static void UnlockEnemies()
        {
            // Should I address issue of certain monsters not having useful AI, like City Guards (defaultBattleDecisionPackage)
            // Some monsters are causing a crash too, I should protect against this a bit - really seems to just be the Gargoyle which is missing its asset?
            MonsterDefinition[] monster_definitions = DatabaseRepository.GetDatabase<MonsterDefinition>().GetAllElements();
            foreach (MonsterDefinition monster_definition in monster_definitions)
            {
                Traverse.Create((object)monster_definition).Field("inDungeonEditor").SetValue((object)true);
            }
        }

        private static void UnlockLootPacks()
        {
            // Maybe not necessary, the lootpacks are all missing their GUI representation and don't offer much, would rather have a mod dedicated to DM loot packs
            throw new NotImplementedException();
        }

        private static void UnlockItems()
        {
            ItemDefinition[] item_definitions = DatabaseRepository.GetDatabase<ItemDefinition>().GetAllElements();
            foreach (ItemDefinition item_definition in item_definitions)
            {
                Traverse.Create((object)item_definition).Field("inDungeonEditor").SetValue((object)true);
            }
        }

        private static void UnlockTraps()
        {
            String description;
            String title;
            EnvironmentEffectDefinition[] env_effect_definitions = DatabaseRepository.GetDatabase<EnvironmentEffectDefinition>().GetAllElements();
            foreach (EnvironmentEffectDefinition env_effect_definition in env_effect_definitions)
            {
                description = env_effect_definition.GuiPresentation.Description;
                title = env_effect_definition.GuiPresentation.Title;
                if (title == "")
                {
                    title = env_effect_definition.name;
                    title = title.Replace("_", " ");
                }
                if (description == "")
                {
                    description = env_effect_definition.name;
                    description = description.Replace("_", " ");
                }
                GuiPresentationBuilder presentationBuilder = 
                new GuiPresentationBuilder(
                    LocalizationHelper.AddString("EnvironmentEffect/&" + env_effect_definition.name + "Description", description), 
                    LocalizationHelper.AddString("EnvironmentEffect/&" + env_effect_definition.name + "Title", title)
                );
                GuiPresentation guiPresentation = presentationBuilder.Build();
                Traverse.Create((object)env_effect_definition).Field(nameof(guiPresentation)).SetValue((object)guiPresentation);
                Traverse.Create((object)env_effect_definition).Field("inDungeonEditor").SetValue((object)true);
            }
        }
    }
}