package com.demo.datastructure;

//File: CircularQueueDemo.java
class CircularQueue {
 private int front = -1, rear = -1;
 private int[] queue;
 private int size;

 CircularQueue(int size) {
     this.size = size;
     queue = new int[size];
 }

 boolean isFull() {
     return (front == 0 && rear == size - 1) || (rear + 1 == front);
 }

 boolean isEmpty() {
     return front == -1;
 }

 void enqueue(int data) {
     if (isFull()) {
         System.out.println("Queue is full!");
         return;
     }
     if (front == -1) front = 0;
     rear = (rear + 1) % size;
     queue[rear] = data;
 }

 void dequeue() {
     if (isEmpty()) {
         System.out.println("Queue is empty!");
         return;
     }
     System.out.println("Removed: " + queue[front]);
     if (front == rear) front = rear = -1;
     else front = (front + 1) % size;
 }

 void display() {
     if (isEmpty()) {
         System.out.println("Queue is empty!");
         return;
     }
     System.out.print("Queue elements: ");
     int i = front;
     while (true) {
         System.out.print(queue[i] + " ");
         if (i == rear) break;
         i = (i + 1) % size;
     }
     System.out.println();
 }
}

public class CircularQueueDemo {
 public static void main(String[] args) {
     CircularQueue cq = new CircularQueue(5);
     cq.enqueue(10);
     cq.enqueue(20);
     cq.enqueue(30);
     cq.display();
     cq.dequeue();
     cq.display();
 }
}

