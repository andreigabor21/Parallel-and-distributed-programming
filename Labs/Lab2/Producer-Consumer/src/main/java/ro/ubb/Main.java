package ro.ubb;

import java.util.Arrays;
import java.util.List;

/**
 * @author Andrei Gabor
 */
public class Main {

    private static final int NUMBER_OF_TASKS = 10;

    public static void main(String[] args) {
        ConcurrentStack stack = new ConcurrentStack(1);

        List<Integer> firstVector = Arrays.asList(1,1,1,1,1,1,1,1,1,1);
        List<Integer> secondVector = Arrays.asList(1,1,1,1,1,1,1,1,1,1);
        Producer producer = new Producer(firstVector, secondVector, stack, NUMBER_OF_TASKS);
        Consumer consumer = new Consumer(stack, NUMBER_OF_TASKS);

        Thread producerThread = new Thread(producer);
        Thread consumerThread = new Thread(consumer);

        producerThread.start();
        consumerThread.start();
        try {
            producerThread.join();
            consumerThread.join();
        } catch (InterruptedException e) {
            System.err.println(e.getMessage());
        }

        System.out.println("Main finished");

        System.out.println("Sum from consumer:" + consumer.getSum());
        int sum = 0;
        for(int i = 0; i < NUMBER_OF_TASKS; i++) {
            sum += firstVector.get(i) * secondVector.get(i);
        }
        System.out.println("Sum computer outside: " + sum);
    }
}
