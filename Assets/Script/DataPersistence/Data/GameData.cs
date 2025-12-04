using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GameData
{
    public string sceneName;
    //public int currentCurrency;
    //public int totalCurrency;
    //public float playerHealth;
    //public float playerMaxHealth;
    //public float playerSpeed;
    //public Vector3 playerPosition;
    //public SerializableDictionary<string, bool> enemiesKilled;
    //public SerializableDictionary<int, string> weaponHotbar;
    //public SerializableDictionary<string, int> enemiesHealth;
    //public SerializableDictionary<string, Vector3> enemiesPosition;

    // the values defined in this constructor will be the default values
    // the game starts with when there is no save data to load
    public GameData()
    {
        sceneName = "";
        //this.currentCurrency = 0;
        //this.totalCurrency = 0;
        //this.playerHealth = 100;
        //this.playerMaxHealth = 100;
        //this.playerSpeed = 4;
        //playerPosition = Vector3.zero;
        //enemiesKilled = new SerializableDictionary<string, bool>();
        //weaponHotbar = new SerializableDictionary<int, string>();
        //enemiesHealth = new SerializableDictionary<string, int>();
        //enemiesPosition = new SerializableDictionary<string, Vector3>();
    }
}
