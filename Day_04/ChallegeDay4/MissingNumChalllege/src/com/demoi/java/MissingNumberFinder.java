package com.demoi.java;
public class MissingNumberFinder {
    public static int findMissing(int[] arr) {
        int n = arr.length;
        int expectedSum = (n * (n + 1)) / 2;
        int actualSum = 0;

        for (int num : arr) {
            actualSum += num;
        }

        return expectedSum - actualSum;
    }

    public static void main(String[] args) {
        int[] arr = {3, 0, 1};
        System.out.println("Missing number is: " + findMissing(arr)); // Output: 2
    }
}