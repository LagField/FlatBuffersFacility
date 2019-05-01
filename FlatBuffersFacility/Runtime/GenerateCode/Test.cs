using System.Collections.Generic;
using FlatBuffers;

namespace Game_WebProtocol
{
    public class Enemy
    {
        public int id;
        public string name;
        public bool isLock;
        public float hp;
        public List<int> inventoryIds = new List<int>();
        public Car drivenCar;
        public List<Car> ownCars = new List<Car>();
        public List<string> all_names = new List<string>();

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<FB_WebProtocol.Enemy> offset = Game_WebProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            FB_WebProtocol.Enemy source = FB_WebProtocol.Enemy.GetRootAsEnemy(bb);
            Game_WebProtocolConvertMethods.Decode(this, source);
        }
    }

    public class Car
    {
        public int id;
        public float speed;

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<FB_WebProtocol.Car> offset = Game_WebProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            FB_WebProtocol.Car source = FB_WebProtocol.Car.GetRootAsCar(bb);
            Game_WebProtocolConvertMethods.Decode(this, source);
        }
    }

}
