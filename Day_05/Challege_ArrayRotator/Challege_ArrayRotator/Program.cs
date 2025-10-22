using System;

public class ArrayRotator
{
    public static void Rotate(int[] nums, int k)
    {
        int n = nums.Length;
        k = k % n; // Handle cases where k > n

        Reverse(nums, 0, n - 1);       // Reverse the whole array
        Reverse(nums, 0, k - 1);       // Reverse first k elements
        Reverse(nums, k, n - 1);       // Reverse the rest
    }

    private static void Reverse(int[] nums, int start, int end)
    {
        while (start < end)
        {
            int temp = nums[start];
            nums[start] = nums[end];
            nums[end] = temp;
            start++;
            end--;
        }
    }

    // Example usage
    public static void Main()
    {
        int[] nums = { 1, 2, 3, 4, 5, 6, 7 };
        int k = 3;

        Rotate(nums, k);

        Console.WriteLine("Rotated array: " + string.Join(", ", nums));
    }
}