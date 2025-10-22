package com.demo.challege;

import java.util.*;

public class ArrayMergerDemo {
    public static int[] mergeSortedArrays(int[] nums1, int[] nums2) {
        int i = 0, j = 0;
        List<Integer> merged = new ArrayList<>();

        while (i < nums1.length && j < nums2.length) {
            if (nums1[i] <= nums2[j]) {
                merged.add(nums1[i++]);
            } else {
                merged.add(nums2[j++]);
            }
        }

        while (i < nums1.length) {
            merged.add(nums1[i++]);
        }

        while (j < nums2.length) {
            merged.add(nums2[j++]);
        }

        // Convert List<Integer> to int[]
        return merged.stream().mapToInt(Integer::intValue).toArray();
    }

    public static void main(String[] args) {
        int[] nums1 = {1, 3, 5, 7};
        int[] nums2 = {2, 4, 6, 8};

        int[] result = mergeSortedArrays(nums1, nums2);
        System.out.println("Merged array: " + Arrays.toString(result));
    }
}
