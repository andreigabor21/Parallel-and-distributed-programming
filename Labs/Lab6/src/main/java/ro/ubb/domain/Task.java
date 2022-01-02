package ro.ubb.domain;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

public class Task implements Runnable {

    private final Graph graph;
    private final int start;
    private final List<Integer> path;
    private final Lock lock;
    private final List<Integer> result;
    private final AtomicBoolean bool;

    public Task(Graph graph, int node, List<Integer> result, AtomicBoolean bool) {
        this.graph = graph;
        this.start = node;
        this.path = new ArrayList<>();
        this.lock = new ReentrantLock();
        this.bool = bool;
        this.result = result;
    }

    @Override
    public void run() {
        visit(start);
    }

    private void visit(int node) {
        path.add(node);
        if (!bool.get()) {
            if (path.size() == graph.size()) {
                if (graph.getNeighboursOf(node).contains(start)) {
                    bool.set(true);
                    this.lock.lock();
                    result.clear();
                    result.addAll(this.path);
                    System.out.println(result);
                    this.lock.unlock();
                }
                System.out.println(result);
                return;
            }

            graph.getNeighboursOf(node).forEach(neighbour -> {
                if (!this.path.contains(neighbour)) {
                    visit(neighbour);
                }
            });
        }
    }
}
