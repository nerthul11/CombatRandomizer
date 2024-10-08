using Modding;
using RandomizerCore.Logic;
using RandomizerCore.StringItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;


namespace CombatRandomizer.Manager
{
    public class LogicHandler
    {
        public static void Hook()
        {
            RCData.RuntimeLogicOverride.Subscribe(5f, ApplyLogic);
            if (ModHooks.GetMod("GodhomeRandomizer") is Mod)
                RCData.RuntimeLogicOverride.Subscribe(2000f, GodhomeRandoInterop);
        }

        private static void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!CombatManager.Settings.Enabled)
                return;

            lmb.GetOrAddTerm("NAILDAMAGE", TermType.Int);
            lmb.AddItem(new StringItemTemplate("Nail_Damage", "NAILDAMAGE++"));

            lmb.GetOrAddTerm("SOULGAIN", TermType.Int);
            lmb.AddItem(new StringItemTemplate("Soul_Gain", "SOULGAIN++"));

            lmb.GetOrAddTerm("SOULPLUG", TermType.Int);
            lmb.AddItem(new StringItemTemplate("Soul_Plug", "SOULPLUG++"));
            
            // To review best way to handle this. NOTCHES should equal total notches obtained, and not fragments.
            lmb.AddItem(new StringItemTemplate("Notch_Fragment", "NOTCHES++"));

            // Modify combat macros
            lmb.DoMacroEdit(new("ALLBALDURS", "((FIREBALL | QUAKE) + (SOULGAIN>7 | SOULPLUG>3)) | GLOWINGWOMB + (SOULGAIN>7 | SOULPLUG>3) | (WEAVERSONG | SPORESHROOM + FOCUS + (SOULGAIN>7 | SOULPLUG>3)) + OBSCURESKIPS | CYCLONE + DIFFICULTSKIPS"));
            lmb.DoMacroEdit(new("AERIALMINIBOSS", "ORIG + (NAILDAMAGE>4 + SOULGAIN>7 + SOULPLUG>3 | NAILDAMAGE>4 + SOULGAIN>5 + SOULPLUG>1 + MILDCOMBATSKIPS | SPICYCOMBATSKIPS)"));
            lmb.DoMacroEdit(new("BOSS", "ORIG + (NAILDAMAGE>4 + SOULGAIN>7 + SOULPLUG>3 | NAILDAMAGE>4 + SOULGAIN>5 + SOULPLUG>1 + MILDCOMBATSKIPS | SPICYCOMBATSKIPS)"));
            lmb.DoMacroEdit(new("BOSSFLUKE", "ORIG + (SOULGAIN>7 + SOULPLUG>3 | NAILDAMAGE>4 + SOULGAIN>5 + SOULPLUG>1 + MILDCOMBATSKIPS | SPICYCOMBATSKIPS) | NAILDAMAGE>12 + MILDCOMBATSKIPS | NAILDAMAGE>8 + SPICYCOMBATSKIPS"));
            lmb.DoMacroEdit(new("MINIBOSS", "ORIG + (NAILDAMAGE>4 + SOULGAIN>4 + SOULPLUG | NAILDAMAGE>2 + MILDCOMBATSKIPS | SPICYCOMBATSKIPS)"));
        }

        private static void GodhomeRandoInterop(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!CombatManager.Settings.Enabled)
                return;

            lmb.DoLogicEdit(new("Nail_Upgradable_1", "NAILDAMAGE>8"));
            lmb.DoLogicEdit(new("Nail_Upgradable_2", "NAILDAMAGE>12"));
            lmb.DoLogicEdit(new("Nail_Upgradable_3", "NAILDAMAGE>16"));
            lmb.DoLogicEdit(new("Nail_Upgradable_4", "NAILDAMAGE>20"));
        }
    }
}