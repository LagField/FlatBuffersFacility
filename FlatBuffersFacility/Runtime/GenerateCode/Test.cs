using System.Collections.Generic;
using FlatBuffers;

namespace MyProtocol
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
            Offset<TestNameSpace.Enemy> offset = MyProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            TestNameSpace.Enemy source = TestNameSpace.Enemy.GetRootAsEnemy(bb);
            MyProtocolConvertMethods.Decode(this, source);
        }
    }

    public class Car
    {
        public int id;
        public float speed;

        public void Encode(FlatBufferBuilder fbb)
        {
            Offset<TestNameSpace.Car> offset = MyProtocolConvertMethods.Encode(this, fbb);
            fbb.Finish(offset.Value);
        }

        public void Decode(ByteBuffer bb)
        {
            TestNameSpace.Car source = TestNameSpace.Car.GetRootAsCar(bb);
            MyProtocolConvertMethods.Decode(this, source);
        }
    }

}
