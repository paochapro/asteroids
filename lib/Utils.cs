﻿using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Asteroids;

internal static class Utils
{
    public static void print(params object[] args)
    {
        foreach (object var in args)
            Console.Write(var + " ");
        Console.WriteLine();
    }
    public static int clamp(int value, int min, int max)
    {
        if (value > max) value = max;
        if (value < min) value = min;
        return value;
    }
    public static float clamp(float value, float min, float max)
    {
        if (value > max) value = max;
        if (value < min) value = min;
        return value;
    }

    public static string EndingAbcense(string str, string end) => str + (str.EndsWith(end) ? "" : end);
    
    public static string ReadUntil(this StreamReader reader, char value, bool skipLast = false)
    {
        string result = "";
        int peek = reader.Peek();

        while (Convert.ToChar(peek) != value && peek != -1)
        {
            result += Convert.ToChar(reader.Read());
            peek = reader.Peek();
        }

        if (skipLast) reader.Read();

        return result;
    }
    
    //Randomness
    public static int RandomBetween(int a, int b)
    {
        int seed = (int)DateTime.Now.Ticks;

        if (a > b)
        {
            (a, b) = (b, a);
        }

        return new Random(seed).Next(a, b);
    }
    public static int Random(int min, int max)
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(seed).Next(min, max);
    }
    public static float RandomFloat(float min, float max)
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(seed).NextSingle(min, max);
    }
    public static float RandomFloat()
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(seed).NextSingle(0.0f, 1.0f);
    }
    public static int RandomRange(Range<int> range)
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(seed).Next(range.Min, range.Max);
    }
    public static float RandomRange(Range<float> range)
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(seed).NextSingle(range.Min, range.Max);
    }
    public static double RandomRange(Range<long> range)
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(seed).NextInt64(range.Min, range.Min);
    }
    public static bool Chance(int percent)
    {
        int seed = (int)DateTime.Now.Ticks;
        return new Random(seed).Next(100) < percent;
    }
    public static int Chance(params int[] chances)
    {
        if (chances.Sum() != 100)
            return -1;

        int seed = (int)DateTime.Now.Ticks;
        int randomNumber = new Random(seed).Next(100) + 1;

        int previousSum = 0;
        int index = 0;
        foreach (int chance in chances)
        {
            if (randomNumber <= previousSum + chance &&
                randomNumber > previousSum)
            {
                return index;
            }

            index++;
            previousSum += chance;
        }

        //Error, impossible
        return -2;
    }

    //Math
    public static float center(float x, float x2, float size)   => (x + x2) / 2 - size / 2;
    public static float center(float x, float size)             => x / 2 - size / 2;
    public static int center(int x, int x2, int size)           => (x + x2) / 2 - size / 2;
    public static int center(int x, int size)                   => x / 2 - size / 2;
    public static Point2 center(Point2 p, RectangleF r)         => new Point2(p.X / 2 - r.Width / 2, p.Y / 2 - r.Height / 2); 
    public static Point2 center(Point2 p, Point2 s)             => new Point2(p.X / 2 - s.X / 2, p.Y / 2 - s.Y / 2); 

    public static int percent(double value, double percent) => (int)Math.Round(value / 100 * percent);
    public static double avg(params double[] values) => values.Average();
    public static int Round(double value) => (int)Math.Round(value);
    public static int Round(float value) => (int)Math.Round(value);

    public static Point2 Center(this Point2 p1, Point2 p2) => new Point2( (p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
}