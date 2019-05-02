using System.Collections.Generic;
using FlatBuffers;

namespace WebProtocol.FlatBuffersProtocol
{
    public class Enemy : FlatBuffersFacility.PoolObject
    {
        public int id;
        public Vec3 position;
        public List<int> inventoryIds = new List<int>();
        public Weapon weapon;
        public int teamId;

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<global::FlatBuffersProtocol.Enemy> offset = WebProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            global::FlatBuffersProtocol.Enemy source = global::FlatBuffersProtocol.Enemy.GetRootAsEnemy(bb);
            WebProtocolConvertMethods.Decode(this, source);
        }

        public override void Release()
        {
            id = 0;
            if(position != null)
            {
                FlatBuffersFacility.Pool.Put(position);
                position = null;
            }
            inventoryIds.Clear();
            if(weapon != null)
            {
                FlatBuffersFacility.Pool.Put(weapon);
                weapon = null;
            }
            teamId = 0;
        }
    }

    public class Vec3 : FlatBuffersFacility.PoolObject
    {
        public float x;
        public float y;
        public float z;

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<global::FlatBuffersProtocol.Vec3> offset = WebProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            global::FlatBuffersProtocol.Vec3 source = global::FlatBuffersProtocol.Vec3.GetRootAsVec3(bb);
            WebProtocolConvertMethods.Decode(this, source);
        }

        public override void Release()
        {
            x = 0;
            y = 0;
            z = 0;
        }
    }

    public class Weapon : FlatBuffersFacility.PoolObject
    {
        public int id;
        public int ammo_capacity;

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<global::FlatBuffersProtocol.Weapon> offset = WebProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            global::FlatBuffersProtocol.Weapon source = global::FlatBuffersProtocol.Weapon.GetRootAsWeapon(bb);
            WebProtocolConvertMethods.Decode(this, source);
        }

        public override void Release()
        {
            id = 0;
            ammo_capacity = 0;
        }
    }

}
