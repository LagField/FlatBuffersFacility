using System.Collections.Generic;
using FlatBuffers;

namespace Game_WebProtocol.FB_WebProtocol2
{
    public class Weapon : FlatBuffersFacility.PoolObject
    {
        public int id;
        public string name = "";
        public int ammo_capacity;

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<global::FB_WebProtocol2.Weapon> offset = Game_WebProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            global::FB_WebProtocol2.Weapon source = global::FB_WebProtocol2.Weapon.GetRootAsWeapon(bb);
            Game_WebProtocolConvertMethods.Decode(this, source);
        }

        public override void Release()
        {
            id = 0;
            name = "";
            ammo_capacity = 0;
        }
    }

}
