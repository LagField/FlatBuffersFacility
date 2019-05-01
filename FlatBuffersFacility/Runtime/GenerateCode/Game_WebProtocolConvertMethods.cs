using System.Collections.Generic;
using FlatBuffers;

namespace Game_WebProtocol
{
    public static class Game_WebProtocolConvertMethods
    {
        public static Offset<FB_WebProtocol.Enemy> Encode(Enemy source, FlatBufferBuilder fbb)
        {
            StringOffset nameOffset = fbb.CreateString(source.name);
            FB_WebProtocol.Enemy.StartInventoryIdsVector(fbb,source.inventoryIds.Count);
            for (int i = source.inventoryIds.Count - 1; i >= 0; i--)
            {
                fbb.AddInt(source.inventoryIds[i]);
            }
            VectorOffset inventoryIdsOffset = fbb.EndVector();
            Offset<FB_WebProtocol.Car> drivenCarOffset  = new Offset<FB_WebProtocol.Car>();
            if(source.drivenCar != null)
            {
                drivenCarOffset = Encode(source.drivenCar,fbb);
            }
            Offset<FB_WebProtocol.Car>[] ownCarsOffsets = new Offset<FB_WebProtocol.Car>[source.ownCars.Count];
            for (int i = source.ownCars.Count - 1; i >= 0; i--)
            {
                ownCarsOffsets[i] = Encode(source.ownCars[i],fbb);
            }
            VectorOffset ownCarsOffset = FB_WebProtocol.Enemy.CreateOwnCarsVector(fbb, ownCarsOffsets);
            StringOffset[] all_namesOffsets = new StringOffset[source.all_names.Count];
            for (int i = source.all_names.Count - 1; i >= 0; i--)
            {
                all_namesOffsets[i] = fbb.CreateString(source.all_names[i]);
            }
            VectorOffset all_namesOffset = FB_WebProtocol.Enemy.CreateAllNamesVector(fbb, all_namesOffsets);
            FB_WebProtocol.Enemy.StartEnemy(fbb);
            FB_WebProtocol.Enemy.AddId(fbb,source.id);
            FB_WebProtocol.Enemy.AddName(fbb,nameOffset);
            FB_WebProtocol.Enemy.AddIsLock(fbb,source.isLock);
            FB_WebProtocol.Enemy.AddHp(fbb,source.hp);
            FB_WebProtocol.Enemy.AddInventoryIds(fbb,inventoryIdsOffset);
            FB_WebProtocol.Enemy.AddDrivenCar(fbb,drivenCarOffset);
            FB_WebProtocol.Enemy.AddOwnCars(fbb,ownCarsOffset);
            FB_WebProtocol.Enemy.AddAllNames(fbb,all_namesOffset);
            return FB_WebProtocol.Enemy.EndEnemy(fbb);
        }
         public static void Decode(Enemy destination, FB_WebProtocol.Enemy source)
        {
            destination.id = source.Id;
            destination.name = source.Name;
            destination.isLock = source.IsLock;
            destination.hp = source.Hp;
            for (int i = 0; i < source.InventoryIdsLength; i++)
            {
                destination.inventoryIds.Add(source.InventoryIds(i));
            }
            if (source.DrivenCar.HasValue)
            {
                destination.drivenCar = new Car();
                Decode(destination.drivenCar,source.DrivenCar.Value);
            }
            for (int i = 0; i < source.OwnCarsLength; i++)
            {
                Car newCar = new Car();
                Decode(newCar,source.OwnCars(i).Value);
                destination.ownCars.Add(newCar);
            }
            for (int i = 0; i < source.AllNamesLength; i++)
            {
                destination.all_names.Add(source.AllNames(i));
            }
        }

        public static Offset<FB_WebProtocol.Car> Encode(Car source, FlatBufferBuilder fbb)
        {
            FB_WebProtocol.Car.StartCar(fbb);
            FB_WebProtocol.Car.AddId(fbb,source.id);
            FB_WebProtocol.Car.AddSpeed(fbb,source.speed);
            return FB_WebProtocol.Car.EndCar(fbb);
        }
         public static void Decode(Car destination, FB_WebProtocol.Car source)
        {
            destination.id = source.Id;
            destination.speed = source.Speed;
        }

    }
}
