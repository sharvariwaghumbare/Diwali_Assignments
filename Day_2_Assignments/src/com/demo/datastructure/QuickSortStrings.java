package com.demo.datastructure;

//File: QuickSortStrings.java
import java.util.Arrays;

public class QuickSortStrings {
 public static void quickSort(String[] arr, int low, int high) {
     if (low < high) {
         int pi = partition(arr, low, high);
         quickSort(arr, low, pi - 1);
         quickSort(arr, pi + 1, high);
     }
 }

 private static int partition(String[] arr, int low, int high) {
     String pivot = arr[high];
     int i = (low - 1);
     for (int j = low; j < high; j++) {
         if (arr[j].compareToIgnoreCase(pivot) <= 0) {
             i++;
             String temp = arr[i];
             arr[i] = arr[j];
             arr[j] = temp;
         }
     }
     String temp = arr[i + 1];
     arr[i + 1] = arr[high];
     arr[high] = temp;
     return i + 1;
 }

 public static void main(String[] args) {
     String[] arr = {"cat", "dog", "buffelo", "sparrow", "cow"};
     quickSort(arr, 0, arr.length - 1);
     System.out.println("Sorted Strings: " + Arrays.toString(arr));
 }
}
