
class Program
{
    static void Main()
    {
        int[] numbers = new int[5];

        Console.WriteLine("Enter 5 numbers:");
        for (int i = 0; i < numbers.Length; i++)
        {
            Console.Write($"Number {i + 1}: ");
            numbers[i] = Convert.ToInt32(Console.ReadLine());
        }

        Array.Sort(numbers);

        Console.WriteLine("\nSorted Numbers:");
        foreach (int num in numbers)
        {
            Console.WriteLine(num);
        }
    }
}