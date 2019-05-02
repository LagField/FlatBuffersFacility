using System.Collections.Generic;
using FlatBuffers;

namespace Game_WebProtocol.FB_WebProtocol
{
    public class Enemy
    {
        public int id;
        public string name = "";
        public bool isLock;
        public float hp;
        public List<int> inventoryIds = new List<int>();
        public Car drivenCar;
        public List<Car> ownCars = new List<Car>();
        public List<string> all_names = new List<string>();
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
    }

    public class Car
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
    }

    public class Weapon
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
    }

}
