namespace Skyla.Engine.MultiBuffers;

public static class BufferNeeds
{
    public static int BestRoot(int available, int size)
    {
        int a = available - 2;
        if (a <= 1)
        {
            return 1;
        }
        int k = int.MaxValue;
        double i = 1.0;
        while (a < k)
        {
            i++;
            k = (int)Math.Ceiling(Math.Pow(size, 1 / i));
        }
        return k;
    }

    public static int BestFactor(int available, int size)
    {
        int a = available - 2;
        if (a <= 1)
        {
            return 1;
        }
        int k = size;
        double i = 1.0;
        while (a < k)
        {
            i++;
            k = (int)Math.Ceiling(size / i);
        }
        return k;
    }
}
