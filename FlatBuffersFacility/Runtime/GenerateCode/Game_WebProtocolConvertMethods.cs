using System.Collections.Generic;
using FlatBuffers;
using Game_WebProtocol;
using Game_WebProtocol.FB_WebProtocol;
using Game_WebProtocol.FB_WebProtocol2;

namespace Game_WebProtocol
{
    public static class Game_WebProtocolConvertMethods
    {
        public static Offset<global::FB_WebProtocol.Enemy> Encode(Game_WebProtocol.FB_WebProtocol.Enemy source, FlatBufferBuilder fbb)
        {
            StringOffset nameOffset = fbb.CreateString(source.name);
            global::FB_WebProtocol.Enemy.StartInventoryIdsVector(fbb,source.inventoryIds.Count);
            for (int i = source.inventoryIds.Count - 1; i >= 0; i--)
            {
                fbb.AddInt(source.inventoryIds[i]);
            }
            VectorOffset inventoryIdsOffset = fbb.EndVector();
            Offset<global::FB_WebProtocol.Car> drivenCarOffset  = new Offset<global::FB_WebProtocol.Car>();
            if(source.drivenCar != null)
            {
                drivenCarOffset = Encode(source.drivenCar,fbb);
            }
            for (int i = 0; i < source.ownCars.Count; i++)
            {
                source.ownCarsOffsetList.Add(Encode(source.ownCars[i],fbb));
            }
            global::FB_WebProtocol.Enemy.StartOwnCarsVector(fbb,source.ownCars.Count);
            for (int i = source.ownCars.Count - 1; i >= 0; i--)
            {
                fbb.AddOffset(source.ownCarsOffsetList[i].Value);
            }
            VectorOffset ownCarsOffset = fbb.EndVector();
            for (int i = 0; i < source.all_names.Count; i++)
            {
                source.all_namesOffsetList.Add(fbb.CreateString(source.all_names[i]));
            }
            global::FB_WebProtocol.Enemy.StartAllNamesVector(fbb,source.all_names.Count);
            for (int i = source.all_names.Count - 1; i >= 0; i--)
            {
                fbb.AddOffset(source.all_namesOffsetList[i].Value);
            }
            VectorOffset all_namesOffset = fbb.EndVector();
            Offset<global::FB_WebProtocol2.Weapon> weaponOffset  = new Offset<global::FB_WebProtocol2.Weapon>();
            if(source.weapon != null)
            {
                weaponOffset = Encode(source.weapon,fbb);
            }
            Offset<global::FB_WebProtocol.Weapon> weapon2Offset  = new Offset<global::FB_WebProtocol.Weapon>();
            if(source.weapon2 != null)
            {
                weapon2Offset = Encode(source.weapon2,fbb);
            }
            global::FB_WebProtocol.Enemy.StartEnemy(fbb);
            global::FB_WebProtocol.Enemy.AddId(fbb,source.id);
            global::FB_WebProtocol.Enemy.AddName(fbb,nameOffset);
            global::FB_WebProtocol.Enemy.AddIsLock(fbb,source.isLock);
            global::FB_WebProtocol.Enemy.AddHp(fbb,source.hp);
            global::FB_WebProtocol.Enemy.AddInventoryIds(fbb,inventoryIdsOffset);
            global::FB_WebProtocol.Enemy.AddDrivenCar(fbb,drivenCarOffset);
            global::FB_WebProtocol.Enemy.AddOwnCars(fbb,ownCarsOffset);
            global::FB_WebProtocol.Enemy.AddAllNames(fbb,all_namesOffset);
            global::FB_WebProtocol.Enemy.AddWeapon(fbb,weaponOffset);
            global::FB_WebProtocol.Enemy.AddWeapon2(fbb,weapon2Offset);
            return global::FB_WebProtocol.Enemy.EndEnemy(fbb);
        }
         public static void Decode(Game_WebProtocol.FB_WebProtocol.Enemy destination, global::FB_WebProtocol.Enemy source)
        {
            destination.id = source.Id;
            destination.name = source.Name;
            destination.isLock = source.IsLock;
            destination.hp = source.Hp;
            for (int i = 0; i < source.InventoryIdsLength; i++)
            {
                destination.inventoryIds.Add(source.InventoryIds(i));
            }
            if (source.DrivenCar.HasValue)
            {
                destination.drivenCar = FlatBuffersFacility.Pool.Get<FB_WebProtocol.Car>();
                Decode(destination.drivenCar,source.DrivenCar.Value);
            }
            for (int i = 0; i < source.OwnCarsLength; i++)
            {
                Car newCar = FlatBuffersFacility.Pool.Get<FB_WebProtocol.Car>();
                Decode(newCar,source.OwnCars(i).Value);
                destination.ownCars.Add(newCar);
            }
            for (int i = 0; i < source.AllNamesLength; i++)
            {
                destination.all_names.Add(source.AllNames(i));
            }
            if (source.Weapon.HasValue)
            {
                destination.weapon = FlatBuffersFacility.Pool.Get<FB_WebProtocol2.Weapon>();
                Decode(destination.weapon,source.Weapon.Value);
            }
            if (source.Weapon2.HasValue)
            {
                destination.weapon2 = FlatBuffersFacility.Pool.Get<FB_WebProtocol.Weapon>();
                Decode(destination.weapon2,source.Weapon2.Value);
            }
        }

        public static Offset<global::FB_WebProtocol.Vec3> Encode(Game_WebProtocol.FB_WebProtocol.Vec3 source, FlatBufferBuilder fbb)
        {
            global::FB_WebProtocol.Vec3.StartVec3(fbb);
            global::FB_WebProtocol.Vec3.AddX(fbb,source.x);
            global::FB_WebProtocol.Vec3.AddY(fbb,source.y);
            global::FB_WebProtocol.Vec3.AddZ(fbb,source.z);
            return global::FB_WebProtocol.Vec3.EndVec3(fbb);
        }
         public static void Decode(Game_WebProtocol.FB_WebProtocol.Vec3 destination, global::FB_WebProtocol.Vec3 source)
        {
            destination.x = source.X;
            destination.y = source.Y;
            destination.z = source.Z;
        }

        public static Offset<global::FB_WebProtocol.Car> Encode(Game_WebProtocol.FB_WebProtocol.Car source, FlatBufferBuilder fbb)
        {
            global::FB_WebProtocol.Car.StartCar(fbb);
            global::FB_WebProtocol.Car.AddId(fbb,source.id);
            global::FB_WebProtocol.Car.AddSpeed(fbb,source.speed);
            return global::FB_WebProtocol.Car.EndCar(fbb);
        }
         public static void Decode(Game_WebProtocol.FB_WebProtocol.Car destination, global::FB_WebProtocol.Car source)
        {
            destination.id = source.Id;
            destination.speed = source.Speed;
        }

        public static Offset<global::FB_WebProtocol.Weapon> Encode(Game_WebProtocol.FB_WebProtocol.Weapon source, FlatBufferBuilder fbb)
        {
            StringOffset wtfOffset = fbb.CreateString(source.wtf);
            global::FB_WebProtocol.Weapon.StartWeapon(fbb);
            global::FB_WebProtocol.Weapon.AddId(fbb,source.id);
            global::FB_WebProtocol.Weapon.AddWtf(fbb,wtfOffset);
            return global::FB_WebProtocol.Weapon.EndWeapon(fbb);
        }
         public static void Decode(Game_WebProtocol.FB_WebProtocol.Weapon destination, global::FB_WebProtocol.Weapon source)
        {
            destination.id = source.Id;
            destination.wtf = source.Wtf;
        }

        public static Offset<global::FB_WebProtocol2.Weapon> Encode(Game_WebProtocol.FB_WebProtocol2.Weapon source, FlatBufferBuilder fbb)
        {
            StringOffset nameOffset = fbb.CreateString(source.name);
            global::FB_WebProtocol2.Weapon.StartWeapon(fbb);
            global::FB_WebProtocol2.Weapon.AddId(fbb,source.id);
            global::FB_WebProtocol2.Weapon.AddName(fbb,nameOffset);
            global::FB_WebProtocol2.Weapon.AddAmmoCapacity(fbb,source.ammo_capacity);
            return global::FB_WebProtocol2.Weapon.EndWeapon(fbb);
        }
         public static void Decode(Game_WebProtocol.FB_WebProtocol2.Weapon destination, global::FB_WebProtocol2.Weapon source)
        {
            destination.id = source.Id;
            destination.name = source.Name;
            destination.ammo_capacity = source.AmmoCapacity;
        }

    }
}
