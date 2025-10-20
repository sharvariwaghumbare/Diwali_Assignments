package com.demo.challege;

public class Coding_Challege {

    public static void main(String[] args) {
        String[] arr = {"cat", "goat", "dog", "sparrow", "buffelo"};

        int maxLenght = 0;
        String longest = "";

        for (String s : arr) {
            if (s.length() > maxLenght) {
            	maxLenght= s.length();
                longest = s;
            }
        }
        System.out.println("Longest string: " + longest);
        System.out.println("Length of longest string: " + maxLenght);
    }
}
