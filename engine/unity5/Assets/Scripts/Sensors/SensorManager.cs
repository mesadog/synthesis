using System;
using System.Collections.Generic;
using UnityEngine;

class SensorManager : MonoBehaviour
{
    public GameObject Ultrasensor;

    void Start()
    {
        Ultrasensor = Resources.Load("Prefabs/UltrasonicSensor") as GameObject;
    }

}
