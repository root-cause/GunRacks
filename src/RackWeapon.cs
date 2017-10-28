using System;
using System.Linq;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using Newtonsoft.Json;

namespace GunRack
{
    public class GunRackWeapon
    {
        public Guid RackID { get; private set; }
        public int Index { get; private set; }

        public string WeaponName { get; private set; }
        public WeaponHash Hash { get; private set; }
        public int Ammo { get; private set; }

        public WeaponTint Tint { get; private set; }
        public WeaponComponent[] Components { get; private set; }

        [JsonIgnore]
        public GrandTheftMultiplayer.Server.Elements.Object GunObject;

        public GunRackWeapon(Guid rackID, int index, WeaponHash hash, int ammo, WeaponTint tint, WeaponComponent[] components, bool initCall = true)
        {
            RackID = rackID;
            Index = index;

            WeaponName = AllowedWeapons.RackWeaponData[hash].Name;
            Hash = hash;
            Ammo = ammo;

            Tint = tint;
            Components = components;

            if (!initCall)
            {
                GunRackEntity rack = Main.GunRacks.FirstOrDefault(gr => gr.ID == RackID);
                if (rack == null) return;

                GunObject = API.shared.createObject(API.shared.getHashKey(AllowedWeapons.RackWeaponData[hash].Model), new Vector3(), new Vector3());
                GunObject.attachTo(rack.RackObject, null, new Vector3(AllowedWeapons.RackOffsets[index] + AllowedWeapons.RackWeaponData[hash].Offset.X, -0.015 + AllowedWeapons.RackWeaponData[hash].Offset.Y, 0.05 + AllowedWeapons.RackWeaponData[hash].Offset.Z), new Vector3(0.0, 270.0, 270.0));
            }
        }

        public void Remove()
        {
            GunRackEntity rack = Main.GunRacks.FirstOrDefault(gr => gr.ID == RackID);
            if (rack == null) return;

            API.shared.deleteEntity(GunObject.handle);
            rack.Weapons[Index] = null;
        }
    }
}