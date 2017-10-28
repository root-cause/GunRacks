using System;
using System.IO;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using Newtonsoft.Json;

namespace GunRack
{
    public class GunRackEntity
    {
        [JsonProperty(Order = 1)]
        public Guid ID { get; private set; }

        [JsonProperty(Order = 2)]
        public string Owner { get; private set; }

        [JsonProperty(Order = 3)]
        public Vector3 Coords { get; private set; }

        [JsonProperty(Order = 4)]
        public float Rotation { get; private set; }

        [JsonProperty(Order = 5)]
        public bool IsPrivate { get; private set; }

        [JsonProperty(Order = 6)]
        public GunRackWeapon[] Weapons = new GunRackWeapon[10];

        [JsonIgnore]
        public GrandTheftMultiplayer.Server.Elements.Object RackObject { get; private set; }

        [JsonIgnore]
        public TextLabel RackLabel { get; private set; }

        [JsonIgnore]
        public ColShape RackColShape { get; private set; }

        [JsonIgnore]
        public DateTime RackLastSave { get; private set; }

        public GunRackEntity(Guid id, string owner, Vector3 coords, float rotation, bool isPrivate, bool firstTime = false)
        {
            ID = id;
            Owner = owner;
            Coords = coords;
            Rotation = rotation;
            IsPrivate = isPrivate;

            RackObject = API.shared.createObject(API.shared.getHashKey("prop_cs_gunrack"), coords, new Vector3(0f, 0f, rotation));

            RackLabel = API.shared.createTextLabel(string.Format("Gun Rack~n~~n~Owned By: ~b~{0}~n~~w~Locked: ~b~{1}", owner, (isPrivate) ? "Yes" : "No"), coords, 10f, 0.55f, true);
            RackLabel.attachTo(RackObject.handle, null, new Vector3(0f, 0f, 0.75f), new Vector3());

            RackColShape = API.shared.createCylinderColShape(coords, 1f, 1f);
            RackColShape.onEntityEnterColShape += (s, ent) =>
            {
                Client player;
                if ((player = API.shared.getPlayerFromHandle(ent)) != null)
                {
                    player.setData("RackID", ID);
                    player.triggerEvent("SetRackState", true);
                }
            };

            RackColShape.onEntityExitColShape += (s, ent) =>
            {
                Client player;
                if ((player = API.shared.getPlayerFromHandle(ent)) != null)
                {
                    player.resetData("RackID");
                    player.triggerEvent("SetRackState", false);
                }
            };

            if (firstTime) Save();
        }

        public void CreateGuns()
        {
            for (int i = 0; i < 10; i++)
            {
                if (Weapons[i] == null) continue;

                Weapons[i].GunObject = API.shared.createObject(API.shared.getHashKey(AllowedWeapons.RackWeaponData[Weapons[i].Hash].Model), new Vector3(), new Vector3());
                Weapons[i].GunObject.attachTo(RackObject, null, new Vector3(AllowedWeapons.RackOffsets[i] + AllowedWeapons.RackWeaponData[Weapons[i].Hash].Offset.X, -0.015 + AllowedWeapons.RackWeaponData[Weapons[i].Hash].Offset.Y, 0.05 + AllowedWeapons.RackWeaponData[Weapons[i].Hash].Offset.Z), new Vector3(0.0, 270.0, 270.0));
            }
        }

        public void SetPrivacy(bool lock_state)
        {
            IsPrivate = lock_state;

            RackLabel.text = string.Format("Gun Rack~n~~n~Owned By: ~b~{0}~n~~w~Locked: ~b~{1}", Owner, (IsPrivate) ? "Yes" : "No");
            Save();
        }

        public PutGunResult PutGun(Client player, WeaponHash weapon, int ammo, int index)
        {
            if (player.currentWeapon != weapon) return PutGunResult.NotCarrying;
            if (!AllowedWeapons.RackWeaponData.ContainsKey(weapon)) return PutGunResult.WeaponNotAllowed;
            if (index < 0 || index >= Weapons.Length) return PutGunResult.InvalidIndex;

            if (Weapons[index] != null) return PutGunResult.SlotOccupied;
            Weapons[index] = new GunRackWeapon(ID, index, weapon, ammo, player.getWeaponTint(weapon), API.shared.getPlayerWeaponComponents(player, weapon), false);

            player.removeWeapon(weapon);
            player.sendChatMessage(string.Format("~b~[GUN RACK] ~w~You put a ~y~{0} ~w~to the gun rack.", AllowedWeapons.RackWeaponData[weapon].Name));

            Save();
            return PutGunResult.Success;
        }

        public TakeGunResult TakeGun(Client player, int index)
        {
            if (index < 0 || index > 9) return TakeGunResult.InvalidIndex;
            if (Weapons[index] == null) return TakeGunResult.WeaponNotFound;

            player.giveWeapon(Weapons[index].Hash, Weapons[index].Ammo, true, false);
            player.setWeaponTint(Weapons[index].Hash, Weapons[index].Tint);
            foreach (WeaponComponent comp in Weapons[index].Components) player.setWeaponComponent(Weapons[index].Hash, comp);

            player.sendChatMessage(string.Format("~b~[GUN RACK] ~w~You took a ~y~{0} ~w~from the gun rack.", AllowedWeapons.RackWeaponData[Weapons[index].Hash].Name));

            Weapons[index].Remove();
            Save();
            return TakeGunResult.Success;
        }

        public void Save(bool force = false)
        {
            if (!force && DateTime.Now.Subtract(RackLastSave).TotalSeconds < Main.RackSaveInterval) return;
            if (!Directory.Exists(Main.RackDir)) Directory.CreateDirectory(Main.RackDir);

            File.WriteAllText(Main.RackDir + ID + ".json", JsonConvert.SerializeObject(this, Formatting.Indented));
            RackLastSave = DateTime.Now;
        }

        public void Remove(bool exit = false)
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                if (Weapons[i] == null) continue;

                if (exit)
                {
                    Weapons[i].GunObject.delete();
                }
                else
                {
                    Weapons[i].Remove();
                }
            }

            if (!exit)
            {
                foreach (NetHandle entity in RackColShape.getAllEntities())
                {
                    Client player;

                    if ((player = API.shared.getPlayerFromHandle(entity)) != null)
                    {
                        player.resetData("RackID");
                        player.triggerEvent("SetRackState", false);
                    }
                }

                Main.GunRacks.Remove(this);
                File.Delete(Main.RackDir + ID + ".json");
            }

            RackObject.delete();
            RackLabel.delete();
            API.shared.deleteColShape(RackColShape);
        }
    }
}