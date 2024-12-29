public class Tree
{
    public Tree? left;
    public Tree? right;
    public List<float>? leftCairn;
    public List<float>? rightCairn;
    public bool homogeneous;
    public Dictionary<int, int> histogram;
    public int estimate;
    public float error;

    public static Tree Create(List<(List<float> input, int output)> samples, int minLeaf)
    {
        float[,] distances = new float[samples.Count, samples.Count];
        for (int i = 0; i < samples.Count; i++)
        {
            Console.Write($"\rDistances {i + 1}/{samples.Count}");
            for (int j = i; j < samples.Count; j++)
            {
                float distance = CalculateDistance(samples[i].input, samples[j].input);
                distances[i, j] = distance;
                distances[j, i] = distance;
            }
        }
        Console.WriteLine();
        List<int> indices = Enumerable.Range(0, samples.Count).ToList();
        return new Tree(samples, indices, distances, minLeaf);
    }

    private Tree(List<(List<float> input, int output)> samples, List<int> indices, float[,] distances, int minLeaf)
    {
        // initialize null
        this.left = null;
        this.right = null;
        this.leftCairn = null;
        this.rightCairn = null;

        histogram = CalculateHistogram(samples, indices);
        estimate = CalculateEstimate(histogram);
        error = Misclassifications(samples.Count, estimate, histogram);

        // if the GINI is 0, we are done
        if (error == 0)
        {
            homogeneous = true;
            return;
        }
        else
        {
            homogeneous = false;
        }

        // seach for best cairns
        float bestSplitError = error;
        int bestLeftCairnIndex = -1;
        int bestRightCairnIndex = -1;
        List<int> leftIndices = new List<int>(indices.Count);
        List<int> rightIndices = new List<int>(indices.Count);
        for (int leftI = 0; leftI < indices.Count; leftI++)
        {
            Console.Write($"\rSplit {leftI + 1}/{indices.Count}");
            int leftIndex = indices[leftI];
            for (int rightI = leftI + 1; rightI < indices.Count; rightI++)
            {
                int rightIndex = indices[rightI];
                leftIndices.Clear();
                rightIndices.Clear();
                foreach(int index in indices)
                {                     
                    if (distances[leftIndex, index] <= distances[rightIndex, index])
                    {
                        leftIndices.Add(index);
                    }
                    else
                    {
                        rightIndices.Add(index);
                    }
                }
                if (leftIndices.Count < minLeaf || rightIndices.Count < minLeaf)
                {
                    continue;
                }
                Dictionary<int, int> leftHistogram = CalculateHistogram(samples, leftIndices);
                Dictionary<int, int> rightHistogram = CalculateHistogram(samples, rightIndices);
                int leftEstimate = CalculateEstimate(leftHistogram);
                int rightEstimate = CalculateEstimate(rightHistogram);
                float leftError = Misclassifications(leftIndices.Count, leftEstimate, leftHistogram);
                float rightError = Misclassifications(rightIndices.Count, rightEstimate, rightHistogram);
                float leftWeight = (float)leftIndices.Count / (float)indices.Count;
                float rightWeight = (float)rightIndices.Count / (float)indices.Count;
                float splitError = (leftWeight * leftError) + (rightWeight * rightError);
                if (splitError < bestSplitError)
                {
                    bestSplitError = splitError;
                    bestLeftCairnIndex = leftIndex;
                    bestRightCairnIndex = rightIndex;
                }
            }
        }
        Console.WriteLine();

        // if we found an improvement, recurse
        if (bestSplitError < error)
        {
            leftIndices.Clear();
            rightIndices.Clear();
            foreach(int index in indices)
            {
                if (distances[bestLeftCairnIndex, index] <= distances[bestRightCairnIndex, index])
                {
                    leftIndices.Add(index);
                }
                else
                {
                    rightIndices.Add(index);
                }
            }
            leftCairn = new List<float>(samples[bestLeftCairnIndex].input);
            rightCairn = new List<float>(samples[bestRightCairnIndex].input);
            left = new Tree(samples, leftIndices, distances, minLeaf);
            right = new Tree(samples, rightIndices, distances, minLeaf);
        }
    }

    private static float CalculateDistance(List<float> a, List<float> b)
    {
        float distance = 0;
        for (int i = 0; i < a.Count; i++)
        {
            float delta = a[i] - b[i];
            distance += delta * delta;
        }
        return MathF.Sqrt(distance);
    }

    private Dictionary<int, int> CalculateHistogram(List<(List<float> input, int output)> samples, List<int> indices)
    {
        Dictionary<int, int> histogram = new Dictionary<int, int>();
        foreach (int index in indices)
        {
            int output = samples[index].output;
            if (!histogram.ContainsKey(output))
            {
                histogram[output] = 0;
            }
            histogram[output]++;
        }
        return histogram;
    }

    private int CalculateEstimate(Dictionary<int, int> histogram)
    {
        int bestOutput = -1;
        int bestOutputCount = -1;
        foreach (KeyValuePair<int, int> pair in histogram)
        {
            if (pair.Value > bestOutputCount)
            {
                bestOutput = pair.Key;
                bestOutputCount = pair.Value;
            }
        }
        return bestOutput;
    }

    private float GINI(int count, int estimate, Dictionary<int, int> histogram)
    {
        float sumOfSquaredProbabilities = 0;
        foreach (KeyValuePair<int, int> pair in histogram)
        {
            float probability = (float)pair.Value / (float)count;
            sumOfSquaredProbabilities += probability * probability;
        }
        float gini = 1 - sumOfSquaredProbabilities;
        return gini;
    }

    private float Misclassifications(int count, int estimate, Dictionary<int, int> histogram)
    {
        int misclassifications = count - histogram[estimate];
        return (float)misclassifications / (float)count;
    }

    public int Predict(List<float> input)
    {
        if (left == null || right == null)
        {
            return estimate;
        }
        else
        {
            if (leftCairn == null || rightCairn == null)
            {
                throw new Exception("Cairns are null");
            }
            float distanceToLeftCairn = CalculateDistance(input, leftCairn);
            float distanceToRightCairn = CalculateDistance(input, rightCairn);
            if (distanceToLeftCairn <= distanceToRightCairn)
            {
                return left.Predict(input);
            }
            else
            {
                return right.Predict(input);
            }
        }
    }
}