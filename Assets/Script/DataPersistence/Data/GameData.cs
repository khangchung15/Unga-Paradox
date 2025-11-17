using UnityEngine;

[System.Serializable]
public class GameData
{
    public int currentCurrency;
    public float playerHealth;
    public Vector3 playerPosition;

    // the values defined in this constructor will be the default values
    // the game starts with when there is no save data to load
    public GameData(Vector3 playerPos)
    {
        this.currentCurrency = 0;
        this.playerHealth = 100;
        playerPosition = playerPos;
    }
}
