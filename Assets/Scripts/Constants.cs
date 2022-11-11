using UnityEngine;

public class Constants
{
    public enum Tag { Ally, Enemy }
    public const string AllyTag = "Ally";
    public const string EnemyTag = "Enemy";
    public static string TagString(Tag tag)
    {
        switch (tag)
        {
            case Tag.Ally:
                return AllyTag;
            case Tag.Enemy:
                return EnemyTag;
        }

        Debug.LogError("Don't have return value for tag " + tag + ", returning 'Error'");
        return "Error";
    }

    public const int DamageableLayer = 6;
    public const int EnvironmentLayer = 7;
    public const int BulletLayer = 8;

    public const int MenuScene = 0;
    public const string MenuSceneName = "Main Menu Desktop";
    public const int GameScene = 1;
    //public const string GameSceneName = "Test Game";
    public const string GameSceneName = "Dogfight";

    public enum CamType { Desktop, VR }
}
