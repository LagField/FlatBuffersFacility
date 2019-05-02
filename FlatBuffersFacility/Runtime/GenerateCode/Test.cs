using System.Collections.Generic;
using FlatBuffers;

namespace Game_WebProtocol.FB_WebProtocol
{
    public class Enemy : FlatBuffersFacility.PoolObject
    {
        public int id;
        public string name = "";
        public bool isLock;
        public float hp;
        public List<int> inventoryIds = new List<int>();
        public Car drivenCar;
        public List<Car> ownCars = new List<Car>();
        internal List<Offset<global::FB_WebProtocol.Car>> ownCarsOffsetList = new List<Offset<global::FB_WebProtocol.Car>>();
        public List<string> all_names = new List<string>();
        internal List<StringOffset> all_namesOffsetList = new List<StringOffset>();
        public FB_WebProtocol2.Weapon weapon;
        public Weapon weapon2;

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<global::FB_WebProtocol.Enemy> offset = Game_WebProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            global::FB_WebProtocol.Enemy source = global::FB_WebProtocol.Enemy.GetRootAsEnemy(bb);
            Game_WebProtocolConvertMethods.Decode(this, source);
        }

        public override void Release()
        {
            id = 0;
            name = "";
            isLock = false;
            hp = 0;
            inventoryIds.Clear();
            if(drivenCar != null)
            {
                FlatBuffersFacility.Pool.Put(drivenCar);
                drivenCar = null;
            }
            for (int i = 0; i < ownCars.Count; i++)
            {
                Car item = ownCars[i];
                FlatBuffersFacility.Pool.Put(item);
            }
            ownCars.Clear();
            ownCarsOffsetList.Clear();
            all_names.Clear();
            all_namesOffsetList.Clear();
            if(weapon != null)
            {
                FlatBuffersFacility.Pool.Put(weapon);
                weapon = null;
            }
            if(weapon2 != null)
            {
                FlatBuffersFacility.Pool.Put(weapon2);
                weapon2 = null;
            }
        }
    }

    public class Vec3 : FlatBuffersFacility.PoolObject
    {
        public float x;
        public float y;
        public float z;

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<global::FB_WebProtocol.Vec3> offset = Game_WebProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            global::FB_WebProtocol.Vec3 source = global::FB_WebProtocol.Vec3.GetRootAsVec3(bb);
            Game_WebProtocolConvertMethods.Decode(this, source);
        }

        public override void Release()
        {
            x = 0;
            y = 0;
            z = 0;
        }
    }

    public class Car : FlatBuffersFacility.PoolObject
    {
        public int id;
        public float speed;

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<global::FB_WebProtocol.Car> offset = Game_WebProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            global::FB_WebProtocol.Car source = global::FB_WebProtocol.Car.GetRootAsCar(bb);
            Game_WebProtocolConvertMethods.Decode(this, source);
        }

        public override void Release()
        {
            id = 0;
            speed = 0;
        }
    }

    public class Weapon : FlatBuffersFacility.PoolObject
    {
        public int id;
        public string wtf = "";

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<global::FB_WebProtocol.Weapon> offset = Game_WebProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            global::FB_WebProtocol.Weapon source = global::FB_WebProtocol.Weapon.GetRootAsWeapon(bb);
            Game_WebProtocolConvertMethods.Decode(this, source);
        }

        public override void Release()
        {
            id = 0;
            wtf = "";
        }
    }

}
