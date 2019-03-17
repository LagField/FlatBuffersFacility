using System.Collections.Generic;
using FlatBuffers;

namespace SkillData
{
    public static class SkillDataConvertMethods
    {
        public static Offset<TestNameSpace.Enemy> Encode(Enemy source, FlatBufferBuilder fbb)
        {
            StringOffset nameOffset = fbb.CreateString(source.name);
            TestNameSpace.Enemy.StartInventoryIdsVector(fbb,source.inventoryIds.Count);
            for (int i = source.inventoryIds.Count - 1; i >= 0; i--)
            {
                fbb.AddInt(source.inventoryIds[i]);
            }
            VectorOffset inventoryIdsOffset = fbb.EndVector();
            Offset<TestNameSpace.Car> drivenCarOffset = Encode(source.drivenCar,fbb);
            TestNameSpace.Enemy.StartOwnCarsVector(fbb,source.ownCars.Count);
            for (int i = source.ownCars.Count - 1; i >= 0; i--)
            {
                fbb.AddOffset(Encode(source.ownCars[i],fbb).Value);
            }
            VectorOffset ownCarsOffset = fbb.EndVector();
            TestNameSpace.Enemy.StartAllNamesVector(fbb,source.all_names.Count);
            for (int i = source.all_names.Count - 1; i >= 0; i--)
            {
                fbb.AddOffset(fbb.CreateString(source.all_names[i]).Value);
            }
            VectorOffset all_namesOffset = fbb.EndVector();
            TestNameSpace.Enemy.StartEnemy(fbb);
            TestNameSpace.Enemy.AddId(fbb,source.id);
            TestNameSpace.Enemy.AddName(fbb,nameOffset);
            TestNameSpace.Enemy.AddIsLock(fbb,source.isLock);
            TestNameSpace.Enemy.AddHp(fbb,source.hp);
            TestNameSpace.Enemy.AddInventoryIds(fbb,inventoryIdsOffset);
            TestNameSpace.Enemy.AddDrivenCar(fbb,drivenCarOffset);
            TestNameSpace.Enemy.AddOwnCars(fbb,ownCarsOffset);
            TestNameSpace.Enemy.AddAllNames(fbb,all_namesOffset);
            return TestNameSpace.Enemy.EndEnemy(fbb);
        }

        public static Offset<TestNameSpace.Car> Encode(Car source, FlatBufferBuilder fbb)
        {
            TestNameSpace.Car.StartCar(fbb);
            TestNameSpace.Car.AddId(fbb,source.id);
            TestNameSpace.Car.AddSpeed(fbb,source.speed);
            return TestNameSpace.Car.EndCar(fbb);
        }

    }
}
