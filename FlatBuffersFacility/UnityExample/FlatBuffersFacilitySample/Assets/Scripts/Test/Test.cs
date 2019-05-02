using System.Collections;
using System.Collections.Generic;
using FlatBuffers;
using FlatBuffersFacility;
using UnityEngine;
using UnityEngine.Profiling;
using WebProtocol.FlatBuffersProtocol;

public class Test : MonoBehaviour
{
    FlatBufferBuilder fbb = new FlatBufferBuilder(1024);

    void Start()
    {
        EncodeAndDecode();
        
        Profiler.BeginSample("FlatBuffers Profiler");
        for (int i = 0; i < 10000; i++)
        {
            EncodeAndDecode();
        }
        Profiler.EndSample();
    }

    void EncodeAndDecode()
    {
        Enemy enemy = Pool.Get<Enemy>();

        enemy.id = 100;
        enemy.position = Pool.Get<Vec3>();
        enemy.position.x = 10;
        enemy.position.y = 0;
        enemy.position.z = 200;
        enemy.weapon = Pool.Get<Weapon>();
        enemy.weapon.id = 10032;
        enemy.weapon.ammo_capacity = 30;
        enemy.teamId = 1;
        for (int i = 0; i < 10; i++)
        {
            enemy.inventoryIds.Add(i * 3);
        }
        
        fbb.Clear();
        enemy.Encode(fbb);
        Pool.Put(enemy);
        enemy = null;
        
        Enemy anotherEnemy = Pool.Get<Enemy>();
        anotherEnemy.Decode(fbb.DataBuffer);
        Pool.Put(anotherEnemy);
//        Debug.Log(anotherEnemy.id);
//        Debug.Log(anotherEnemy.weapon.ammo_capacity);
    }
}
