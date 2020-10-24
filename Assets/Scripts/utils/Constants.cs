using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Constants
{
    public static readonly Random RANDOM = new Random();

    public const float HEX_FILL_DELAY = 0.05f;
    public const float DEFAULT_HEX_MOVE_TIME = 0.3f;

    public const float DISTANCE_FROM_COLUMN_TOP = 5f;

    public const float X_OFFSET = 0.776F;
    public const float Y_OFFSET = 0.888f;

    public const int DEFAULT_SCORE = 5;

    public const int DEFAULT_BOMB_TIMER_COUNT = 6;
    public const int BOMB_APPEAR_SCORE = 1000;

    public const int DEFAULT_WIDTH = 8;
    public const int MINIMUM_WIDTH = 5;
    public const int MAX_WIDTH = 13;

    public const int DEFAULT_HEIGHT = 9;
    public const int MINIMUM_HEIGHT = 5;
    public const int MAX_HEIGHT = 13;

    public const int DEFAULT_COLOR_COUNT = 5;
    public const int MINIMUM_COLOR_COUNT = 4;
    public const int MAX_COLOR_COUNT = 9;

    private static readonly List<Color> colors = new List<Color> {
        // Yellow
        new Color(1f, 1f, 0),
        // Red
        new Color(1f, 0, 0),
        // Blue
        new Color(0, 0, 202/255f),
        // Green
        new Color(0, 128/255f, 0),
        // Purple
        new Color(128/255f, 0, 128/255f),
        // Brown
        new Color(78/255f, 54/255f, 41/255f),
        // Silver
        new Color(192/255f, 192/255f, 192/255f),
        // Teal
        new Color(0, 128/255f, 128/255f),
        // Pink
        new Color(172/255f, 20/255f, 90/255f)
    };

    public static List<Color> GetColors(int size)
    {
        if (size > colors.Count)
        {
            size = colors.Count;
        }
        List<Color> result = new List<Color>();
        for (int i = 0; i < size; i++)
        {
            Color a = colors[i];
            result.Add(a);
        }
        return result;
    }
}
