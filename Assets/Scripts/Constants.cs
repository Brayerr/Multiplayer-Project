using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const string PLAYER_TAG = "Player";
    public const string ARROW_TAG = "Arrow";
    public const string END_GAME_RPC = "EndGameRPC";
    public const string PLAYER_CHARACTER_ID_PROPERTY_KEY = "PlayerCharacterID";
    public const string PLAYER_READY_PROPERTY_KEY = "PlayerReady";
    public const string ROOM_EVERYONE_READY_KEY = "RoomEveryoneReady";
    public const string PLAYER_INITIALIZED_KEY = "PlayerInitialized";

    public const string USER_UNIQUE_ID = "UserUniqueID";

    public const string PLAYER_PREFAB_NAME = "PlayerCapsule";
    public const string SCORE_KEY_NAME = "Score";
    public const string LOAD_GAME_NAME = "JoinOrStartGame";
    public const string PING_HASHTABLE_NAME = "ping";

    public static string[] ProprtiesToClearOnLeaveRoom = { PLAYER_CHARACTER_ID_PROPERTY_KEY,PLAYER_READY_PROPERTY_KEY,PING_HASHTABLE_NAME };
}
