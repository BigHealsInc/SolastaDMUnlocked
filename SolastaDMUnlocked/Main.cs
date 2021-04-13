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
            // Modifying existing enemy attack types - is this improper to not use Traverse? Sort of a pain with nested content
            var skeleton = DatabaseHelper.MonsterAttackDefinitions.Attack_Skeleton_Scimitar;
            skeleton.EffectDescription.EffectForms[0].DamageForm.DamageType = "DamageSlashing";
            ((Database<MonsterAttackDefinition>)DatabaseRepository.GetDatabase<MonsterAttackDefinition>()).Add((MonsterAttackDefinition)skeleton);

            UnlockEnemies();
            UnlockTraps();
            //UnlockItems();
            //UnlockLootPacks();
        }

        private static void UnlockEnemies()
        {
            MonsterDefinition[] monster_definitions = DatabaseRepository.GetDatabase<MonsterDefinition>().GetAllElements();
            foreach (MonsterDefinition monster_definition in monster_definitions)
            {
                Traverse.Create((object)monster_definition).Field("inDungeonEditor").SetValue((object)true);
            }
        }

        private static void UnlockLootPacks()
        {
            throw new NotImplementedException();
        }

        private static void UnlockItems()
        {
            throw new NotImplementedException();
        }

        private static void UnlockTraps()
        {
            String description;
            EnvironmentEffectDefinition[] env_effect_definitions = DatabaseRepository.GetDatabase<EnvironmentEffectDefinition>().GetAllElements();
            foreach (EnvironmentEffectDefinition env_effect_definition in env_effect_definitions)
            {
                if (env_effect_definition.GuiPresentation.Title == "") 
                {
                    if (env_effect_definition.GuiPresentation.Description == "")
                    {
                        description = env_effect_definition.name;
                    } else {
                        description = env_effect_definition.GuiPresentation.Description;
                    }
                    GuiPresentationBuilder presentationBuilder = 
                    new GuiPresentationBuilder(
                        LocalizationHelper.AddString("EnvironmentEffect/&" + env_effect_definition.name + "Description", description), 
                        LocalizationHelper.AddString("EnvironmentEffect/&" + env_effect_definition.name + "Title", env_effect_definition.name)
                    );
                    GuiPresentation guiPresentation = presentationBuilder.Build();
                    Traverse.Create((object)env_effect_definition).Field(nameof(guiPresentation)).SetValue((object)guiPresentation);
                }
                Traverse.Create((object)env_effect_definition).Field("inDungeonEditor").SetValue((object)true);
            }
        }
    }
}