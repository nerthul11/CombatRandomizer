using System;
using CombatRandomizer.Manager;
using CombatRandomizer.Settings;
using ItemChanger;
using ItemChanger.Modules;
using Modding;
using UnityEngine;

namespace CombatRandomizer.Modules
{
    public class CombatModule : Module
    {
        public SaveSettings Settings { get; set; } = new();
        public class SaveSettings 
        {
            public Difficulty Difficulty = CombatManager.Settings.Difficulty;
            public bool LimitNailDamage = CombatManager.Settings.LimitNailDamage;
        }
        // Module properties
        public int NailItems { get; set; } = 0;
        public int NotchFragments { get; set; } = 0;
        public int SoulGainItems { get; set; } = 0;
        public int SoulPlugItems { get; set; } = 0;
        public int Frames { get; set; } = 0;
        public static CombatModule Instance => ItemChangerMod.Modules.GetOrAdd<CombatModule>();
        public override void Unload() 
        {
            ModHooks.GetPlayerIntHook -= OverrideNailDamage;
            ModHooks.SoulGainHook -= OverrideSoulGain;
            ModHooks.HeroUpdateHook -= SoulDrain;
        }
        public override void Initialize() 
        {
            ModHooks.GetPlayerIntHook += OverrideNailDamage;
            ModHooks.SoulGainHook += OverrideSoulGain;
            ModHooks.HeroUpdateHook += SoulDrain;
        }

        private void SoulDrain()
        {
            if (GameManager.instance.isPaused)
                return;
            
            Frames += 1;
            if (Frames == 120)
            {
                int amount = 1;
                if (Settings.Difficulty == Difficulty.Normal)
                    amount = 3;
                if (Settings.Difficulty == Difficulty.Hard)
                    amount = 5;
                if (Settings.Difficulty == Difficulty.Extreme)
                    amount = 7;
                amount -= SoulPlugItems;
                PlayerData.instance.TakeMP(amount);
                GameCameras.instance.soulOrbFSM.SendEvent("MP DRAIN");
                Frames = 0;
            }
        }

        private int OverrideSoulGain(int soul)
        {
            // By default, you get 11 soul + 3 with SC + 8 with SE.
            // If main mana pool is full, you get a reduced 6 + 2 + 6.
            // Using these as reference, the charms will now grant a relative multiplier
            // instead of an absolute amount of soul per hit.
            float multiplier;
            if (PlayerData.instance.MPCharge < 99)
            {
                multiplier = soul / 11;
            }
            else
            {
                multiplier = soul / 6;
            }

            // Base gain depends on settings, and further gain is obtained with the item checks.
            if (Settings.Difficulty == Difficulty.Easy)
                soul = 8;
            if (Settings.Difficulty == Difficulty.Normal)
                soul = 6; 
            if (Settings.Difficulty == Difficulty.Hard)
                soul = 3;
            if (Settings.Difficulty == Difficulty.Extreme)
                soul = 1;
            
            soul += SoulGainItems;
            soul = Mathf.FloorToInt(multiplier * soul);
            return soul;
        }

        private int OverrideNailDamage(string name, int orig)
        {
            if (name == "nailDamage")
            {
                int base_damage = Settings.Difficulty >= Difficulty.Hard ? (5 - (int)Settings.Difficulty) : 5;
                orig = base_damage + NailItems;
            }

            // If required, cap max damage based on obtained vert items.
            if (Settings.LimitNailDamage)
            {
                bool hasWings = PlayerData.instance.hasDoubleJump;
                bool anyClaw = false;
                bool hasClaw = PlayerData.instance.hasWalljump;
                SplitClaw splitClaw = ItemChangerMod.Modules.Get<SplitClaw>();
                if (splitClaw != null)
                {
                    anyClaw = splitClaw.hasWalljumpAny;
                    hasClaw = splitClaw.hasWalljumpBoth;
                }

                // If no Wings or Claw, then max one vanilla upgrade
                if (!hasWings || !anyClaw || !hasClaw)
                    orig = Math.Min(orig, 9);
                
                // If no Wings and Split Claw, allow two upgrades
                if (anyClaw & !hasWings & !hasClaw)
                    orig = Math.Min(orig, 13);

                // If Wings but no Claw, allow three upgrades
                if (hasWings & !hasClaw & !hasClaw)
                    orig = Math.Min(orig, 17);

                // If Claw or Wings + Split Claw, do nothing
            }
            return orig;
        }
    }
}