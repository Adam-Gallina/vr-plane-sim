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

    public enum SceneType { MainMenu, FFA, Dogfight, Survival, Race }
    public static GameScene MainMenu = new GameScene(0, "Main Menu Desktop", SceneType.MainMenu);
    public static GameScene FfaTest = new GameScene(1, "FFA", SceneType.FFA);

    
    public enum CamType { Desktop, VR }

    public struct GameScene
    {
        public int buildIndex;
        public string name;
        public SceneType mode;

        public GameScene(int index, string name, SceneType mode)
        {
            buildIndex = index;
            this.name = name;
            this.mode = mode;
        }
    }
}