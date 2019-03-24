using System.Collections.Generic;
using FlatBuffers;

namespace MyProtocol
{
    public class Enemy : FlatBuffersFacility.PoolObject
    {
        public int id;
        public string name;
        public bool isLock;
        public float hp;
        public List<int> inventoryIds = new List<int>();
        public Car drivenCar;
        public List<Car> ownCars = new List<Car>();
        internal List<Offset<TestNameSpace.Car>> ownCarsOffsetList = new List<Offset<TestNameSpace.Car>>();
        public List<string> all_names = new List<string>();
        internal List<StringOffset> all_namesOffsetList = new List<StringOffset>();

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
        }
    }

    public class Car : FlatBuffersFacility.PoolObject
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

        public override void Release()
        {
            id = 0;
            speed = 0;
        }
    }

}
