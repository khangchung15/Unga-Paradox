using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int currentCurrency;
    public float playerHealth;
    public Vector3 playerPosition;
    public SerializableDictionary<string, bool> enemiesKilled;
    //public SerializableDictionary<string, int> enemiesHealth;
    //public SerializableDictionary<string, Vector3> enemiesPosition;

    // the values defined in this constructor will be the default values
    // the game starts with when there is no save data to load
    public GameData(Vector3 playerPos)
    {
        this.currentCurrency = 0;
        this.playerHealth = 100;
        playerPosition = playerPos;
        enemiesKilled = new SerializableDictionary<string, bool>();
        //enemiesHealth = new SerializableDictionary<string, int>();
        //enemiesPosition = new SerializableDictionary<string, Vector3>();
    }
}
