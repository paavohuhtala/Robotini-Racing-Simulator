﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using UnityEngine;

public class GameEvents
{
    
}

public enum GameEventType { lap, gameStatus }

[Serializable]
public class GameEvent
{
    [HideInInspector]
    public string type; // for deserialising as the correct type

    public GameEvent(float timestamp)
    {
        this.timestamp = timestamp;
        var type = this.GetType();
        this.type = type.FullName;
    }
    public float timestamp;

    public static GameEvent FromJson(string json)
    {        
        var typeName = JsonUtility.FromJson<GameEvent>(json).type;
        // deserialise first as plain GameEvent to get the instance Type
        var type = System.Type.GetType(typeName);
        // deserialise as the correct type
        return (GameEvent)JsonUtility.FromJson(json, type);
    }
}

[Serializable]
public class CarStatus
{
    public CarStatus(Vector3 position, Vector3 velocity, Quaternion rotation)
    {
        this.position = position;
        this.velocity = velocity;
        this.rotation = rotation;
    }
    public Vector3 position;
    public Vector3 velocity;
    public Quaternion rotation;
}

[Serializable]
public class GameStatus: GameEvent
{
    public GameStatus(CarStatus[] cars, float timestamp): base(timestamp)
    {
        this.cars = cars;
    }
    public CarStatus[] cars;
}

[Serializable]
public class CarInfo
{
    public string teamId;
    public string name;
    public CarInfo(string teamId, string name)
    {
        this.teamId = teamId;
        this.name = name;
    }
}

public class LapCompleted
{
    public CarInfo car;
    public int lapCount;
    public float lastLap;
    public float bestLap;

    public LapCompleted(CarInfo car, int lapCount, float lastLap, float bestLap)
    {
        this.car = car;
        this.lapCount = lapCount;
        this.lastLap = lastLap;
        this.bestLap = bestLap;
    }
}

public class CarRemoved
{
    public CarInfo car;
    public CarRemoved(CarInfo car)
    {
        this.car = car;
    }
}

public class CarDisconnected
{
    public CarInfo car;
    public CarDisconnected(CarInfo car)
    {
        this.car = car;
    }
}

public class CarConnected
{
    public CarInfo car;
    public CarSocket socket;
    public CarConnected(CarInfo car, CarSocket socket)
    {
        this.car = car;
        this.socket = socket;
    }
}

public class MotorsToggle
{
}

public class ResetTimers
{
}