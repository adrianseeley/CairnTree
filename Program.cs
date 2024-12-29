public class Program
{
    public static List<(List<float> input, int output)> ReadMNIST(string filename, int max = -1)
    {
        List<(List<float> input, int output)> data = new List<(List<float> input, int output)>();
        string[] lines = File.ReadAllLines(filename);
        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++) // skip headers
        {
            string line = lines[lineIndex].Trim();
            if (line.Length == 0)
            {
                continue; // skip empty lines
            }
            string[] parts = line.Split(',');
            int labelInt = int.Parse(parts[0]);
            List<float> input = new List<float>();
            for (int i = 1; i < parts.Length; i++)
            {
                input.Add(float.Parse(parts[i]));
            }
            data.Add((input, labelInt));
            if (max != -1 && data.Count >= max)
            {
                break;
            }
        }
        return data;
    }

    public static void Main(string[] args)
    {
        List<(List<float> input, int output)> mnistTrain = ReadMNIST("D:/data/mnist_train.csv", max: 1000);
        List<(List<float> input, int output)> mnistTest = ReadMNIST("D:/data/mnist_test.csv", max: 1000);
        Tree tree = Tree.Create(mnistTrain, minLeaf: 3);
        int correct = 0;
        int incorrect = 0;
        foreach ((List<float> input, int output) in mnistTest)
        {
            int estimate = tree.Predict(input);
            if (estimate == output)
            {
                correct++;
            }
            else
            {
                incorrect++;
            }
        }
        float fitness = (float)correct / (float)(correct + incorrect);
        Console.WriteLine($"Correct: {correct}, Incorrect: {incorrect}, Fitness: {fitness}");
        Console.ReadLine();
    }
}
