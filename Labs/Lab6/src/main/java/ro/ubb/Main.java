package ro.ubb;

import ro.ubb.domain.Graph;
import ro.ubb.domain.Task;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicBoolean;

public class Main {

    private static final int THREAD_COUNT = 5;

    public static void main(String[] args) throws InterruptedException {
        Graph graph = new Graph(10);
        System.out.println(graph);
        long startTime = System.currentTimeMillis();

        findHamiltonianCycle(graph);

        long endTime = System.currentTimeMillis();
        System.out.println(graph);
        System.out.println("Execution time: " + (endTime - startTime) + " ms");

    }

    private static void findHamiltonianCycle(Graph graph) throws InterruptedException {
        ExecutorService executor = Executors.newFixedThreadPool(THREAD_COUNT);

        List<Integer> result = new ArrayList<>(graph.size());

        AtomicBoolean atomicBoolean = new AtomicBoolean(false);

        for (int i = 0; i < graph.size(); i++){ //check from each node
            executor.submit(new Task(graph, i, result, atomicBoolean));
        }

        executor.shutdown();

        executor.awaitTermination(120, TimeUnit.SECONDS);
    }
}
