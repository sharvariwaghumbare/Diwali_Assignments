package com.demo.datastructure;

//File: CircularLinkedListDemo.java
import java.util.Scanner;

class Node {
 int data;
 Node next;
 Node(int d) { data = d; }
}

class CircularLinkedList {
 Node head = null;

 void insertAt(int data, int pos) {
     Node newNode = new Node(data);
     if (head == null) {
         head = newNode;
         newNode.next = head;
         return;
     }
     if (pos == 1) {
         Node temp = head;
         while (temp.next != head) temp = temp.next;
         temp.next = newNode;
         newNode.next = head;
         head = newNode;
         return;
     }
     Node temp = head;
     for (int i = 1; i < pos - 1 && temp.next != head; i++)
         temp = temp.next;
     newNode.next = temp.next;
     temp.next = newNode;
 }

 void deleteNode(int key) {
     if (head == null) return;
     Node curr = head, prev = null;
     do {
         if (curr.data == key) {
             if (prev == null) { // deleting head
                 Node temp = head;
                 while (temp.next != head) temp = temp.next;
                 if (temp == head) head = null;
                 else {
                     temp.next = head.next;
                     head = head.next;
                 }
             } else {
                 prev.next = curr.next;
             }
             return;
         }
         prev = curr;
         curr = curr.next;
     } while (curr != head);
 }

 void modifyNode(int oldData, int newData) {
     if (head == null) return;
     Node temp = head;
     do {
         if (temp.data == oldData) {
             temp.data = newData;
             return;
         }
         temp = temp.next;
     } while (temp != head);
     System.out.println("Node not found.");
 }

 
 void display() {
     if (head == null) {
         System.out.println("List is empty.");
         return;
     }
     Node temp = head;
     do {
         System.out.print(temp.data + " ");
         temp = temp.next;
     } while (temp != head);
     System.out.println();
 }
}

public class CircularLinkedListDemo {
 public static void main(String[] args) {
     Scanner sc = new Scanner(System.in);
     CircularLinkedList list = new CircularLinkedList();
     int ch;
     do {
         System.out.println("\n1.Insert 2.Delete 3.Modify 4.Display 5.Exit");
         ch = sc.nextInt();
         switch (ch) {
             case 1:
                 System.out.print("Enter data and position: ");
                 list.insertAt(sc.nextInt(), sc.nextInt());
                 break;
             case 2:
                 System.out.print("Enter data to delete: ");
                 list.deleteNode(sc.nextInt());
                 break;
             case 3:
                 System.out.print("Enter old and new data: ");
                 list.modifyNode(sc.nextInt(), sc.nextInt());
                 break;
             case 4:
                 list.display();
                 break;
         }
     } while (ch != 5);
 }
}
