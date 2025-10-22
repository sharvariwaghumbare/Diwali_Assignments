using System;
using System.Collections;

class Program
{
    static void Main()
    {
        ArrayList names = new ArrayList();

        Console.WriteLine("Enter 10 names:");

        for (int i = 0; i < 10; i++)
        {
            Console.Write($"Name {i + 1}: ");
            string name = Console.ReadLine();
            names.Add(name);
        }

        names.Sort();

        Console.WriteLine("\nSorted Names:");
        foreach (string name in names)
        {
            Console.WriteLine(name);
        }
    }
}