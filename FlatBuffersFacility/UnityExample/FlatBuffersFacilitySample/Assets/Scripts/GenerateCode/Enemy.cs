using System.Collections.Generic;
using FlatBuffers;

namespace WebProtocol.FlatBuffersProtocol
{
    public class Enemy
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
    }

    public class Vec3
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
    }

    public class Weapon
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
    }

}
