using System.Collections.Generic;
using FlatBuffers;
using WebProtocol;
using WebProtocol.FlatBuffersProtocol;

namespace WebProtocol
{
    public static class WebProtocolConvertMethods
    {
        public static Offset<global::FlatBuffersProtocol.Enemy> Encode(WebProtocol.FlatBuffersProtocol.Enemy source, FlatBufferBuilder fbb)
        {
            Offset<global::FlatBuffersProtocol.Vec3> positionOffset  = new Offset<global::FlatBuffersProtocol.Vec3>();
            if(source.position != null)
            {
                positionOffset = Encode(source.position,fbb);
            }
            global::FlatBuffersProtocol.Enemy.StartInventoryIdsVector(fbb,source.inventoryIds.Count);
            for (int i = source.inventoryIds.Count - 1; i >= 0; i--)
            {
                fbb.AddInt(source.inventoryIds[i]);
            }
            VectorOffset inventoryIdsOffset = fbb.EndVector();
            Offset<global::FlatBuffersProtocol.Weapon> weaponOffset  = new Offset<global::FlatBuffersProtocol.Weapon>();
            if(source.weapon != null)
            {
                weaponOffset = Encode(source.weapon,fbb);
            }
            global::FlatBuffersProtocol.Enemy.StartEnemy(fbb);
            global::FlatBuffersProtocol.Enemy.AddId(fbb,source.id);
            global::FlatBuffersProtocol.Enemy.AddPosition(fbb,positionOffset);
            global::FlatBuffersProtocol.Enemy.AddInventoryIds(fbb,inventoryIdsOffset);
            global::FlatBuffersProtocol.Enemy.AddWeapon(fbb,weaponOffset);
            global::FlatBuffersProtocol.Enemy.AddTeamId(fbb,source.teamId);
            return global::FlatBuffersProtocol.Enemy.EndEnemy(fbb);
        }
         public static void Decode(WebProtocol.FlatBuffersProtocol.Enemy destination, global::FlatBuffersProtocol.Enemy source)
        {
            destination.id = source.Id;
            if (source.Position.HasValue)
            {
                destination.position = FlatBuffersFacility.Pool.Get<FlatBuffersProtocol.Vec3>();
                Decode(destination.position,source.Position.Value);
            }
            for (int i = 0; i < source.InventoryIdsLength; i++)
            {
                destination.inventoryIds.Add(source.InventoryIds(i));
            }
            if (source.Weapon.HasValue)
            {
                destination.weapon = FlatBuffersFacility.Pool.Get<FlatBuffersProtocol.Weapon>();
                Decode(destination.weapon,source.Weapon.Value);
            }
            destination.teamId = source.TeamId;
        }

        public static Offset<global::FlatBuffersProtocol.Vec3> Encode(WebProtocol.FlatBuffersProtocol.Vec3 source, FlatBufferBuilder fbb)
        {
            global::FlatBuffersProtocol.Vec3.StartVec3(fbb);
            global::FlatBuffersProtocol.Vec3.AddX(fbb,source.x);
            global::FlatBuffersProtocol.Vec3.AddY(fbb,source.y);
            global::FlatBuffersProtocol.Vec3.AddZ(fbb,source.z);
            return global::FlatBuffersProtocol.Vec3.EndVec3(fbb);
        }
         public static void Decode(WebProtocol.FlatBuffersProtocol.Vec3 destination, global::FlatBuffersProtocol.Vec3 source)
        {
            destination.x = source.X;
            destination.y = source.Y;
            destination.z = source.Z;
        }

        public static Offset<global::FlatBuffersProtocol.Weapon> Encode(WebProtocol.FlatBuffersProtocol.Weapon source, FlatBufferBuilder fbb)
        {
            global::FlatBuffersProtocol.Weapon.StartWeapon(fbb);
            global::FlatBuffersProtocol.Weapon.AddId(fbb,source.id);
            global::FlatBuffersProtocol.Weapon.AddAmmoCapacity(fbb,source.ammo_capacity);
            return global::FlatBuffersProtocol.Weapon.EndWeapon(fbb);
        }
         public static void Decode(WebProtocol.FlatBuffersProtocol.Weapon destination, global::FlatBuffersProtocol.Weapon source)
        {
            destination.id = source.Id;
            destination.ammo_capacity = source.AmmoCapacity;
        }

    }
}
