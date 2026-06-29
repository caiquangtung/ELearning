namespace ELearning.Infrastructure.Ai;

public static class EmbeddingVectorUtils
{
    public static void Normalize(float[] vector)
    {
        var sum = 0d;
        foreach (var value in vector)
            sum += value * value;

        var norm = Math.Sqrt(sum);
        if (norm <= 0)
            return;

        for (var i = 0; i < vector.Length; i++)
            vector[i] = (float)(vector[i] / norm);
    }

    public static double Norm(IReadOnlyList<float> vector)
    {
        var sum = 0d;
        foreach (var value in vector)
            sum += value * value;

        return Math.Sqrt(sum);
    }
}
