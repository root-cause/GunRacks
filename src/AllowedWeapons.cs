using System.Collections.Generic;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;

namespace GunRack
{
    public class WeaponData
    {
        public string Name;
        public string Model;
        public Vector3 Offset;

        public WeaponData(string name, string model_name, Vector3 offset)
        {
            Name = name;
            Model = model_name;
            Offset = offset;
        }
    }

    public class AllowedWeapons
    {
        public static float[] RackOffsets = { -0.575f, -0.445f, -0.315f, -0.185f, -0.055f, 0.065f, 0.195f, 0.325f, 0.455f, 0.585f };

        public static Dictionary<WeaponHash, WeaponData> RackWeaponData = new Dictionary<WeaponHash, WeaponData>
        {
            // light machine guns
            { WeaponHash.MG, new WeaponData("MG", "w_mg_mg", new Vector3(0.0, -0.02, -0.035)) },
            { WeaponHash.CombatMG, new WeaponData("Combat MG", "w_mg_combatmg", new Vector3(-0.005, 0.0, -0.035)) },

            // assault rifles
            { WeaponHash.AssaultRifle, new WeaponData("Assault Rifle", "w_ar_assaultrifle", new Vector3(0.0, 0.0, 0.0)) },
            { WeaponHash.CarbineRifle, new WeaponData("Carbine Rifle", "w_ar_carbinerifle", new Vector3(0.0, 0.015, 0.0)) },
            { WeaponHash.SpecialCarbine, new WeaponData("Special Carbine", "w_ar_specialcarbine", new Vector3(0.0, 0.025, 0.0)) },
            { WeaponHash.BullpupRifle, new WeaponData("Bullpup Rifle", "w_ar_bullpuprifle", new Vector3(0.0, 0.01, 0.02)) },

            // snipers
            { WeaponHash.SniperRifle, new WeaponData("Sniper Rifle", "w_sr_sniperrifle", new Vector3(0.0, 0.02, -0.065)) },
            { WeaponHash.HeavySniper, new WeaponData("Heavy Sniper", "w_sr_heavysniper", new Vector3(0.0, -0.01, -0.065)) },
            { WeaponHash.MarksmanRifle, new WeaponData("Marksman Rifle", "w_sr_marksmanrifle", new Vector3(0.0, 0.0, -0.025)) },

            // shotguns
            { WeaponHash.PumpShotgun, new WeaponData("Pump Shotgun", "w_sg_pumpshotgun", new Vector3(0.0, 0.0225, -0.03)) },
            { WeaponHash.BullpupShotgun, new WeaponData("Bullpup Shotgun", "w_sg_bullpupshotgun", new Vector3(0.0, 0.0, -0.075)) },
            { WeaponHash.AssaultShotgun, new WeaponData("Assault Shotgun", "w_sg_assaultshotgun", new Vector3(0.0, 0.021, -0.08)) },
            { WeaponHash.HeavyShotgun, new WeaponData("Heavy Shotgun", "w_sg_heavyshotgun", new Vector3(0.0, -0.02, -0.025)) },

            // misc
            { WeaponHash.Musket, new WeaponData("Musket", "w_ar_musket", new Vector3(0.0, 0.03, -0.075)) }
        };
    }
}