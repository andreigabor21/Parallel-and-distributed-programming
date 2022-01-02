package ro.ubb.domain;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Random;

public final class Graph {

    private final List<List<Integer>> edges;
    private final List<Integer> nodes;

    public Graph(int nodeCount) {
        this.edges = new ArrayList<>(nodeCount);
        this.nodes = new ArrayList<>();

        for (int i = 0; i < nodeCount; i++) {
            this.edges.add(new ArrayList<>());
            this.nodes.add(i);
        }

        this.generateRandomHamiltonian(nodeCount);
    }

    private void generateRandomHamiltonian(int size) {
        List<Integer> nodes = this.getNodes();

        Collections.shuffle(nodes);

        for (int i = 0; i < nodes.size() - 1; i++){
            this.addEdge(nodes.get(i),  nodes.get(i + 1));
        }

        this.addEdge(nodes.get(nodes.size() - 1), nodes.get(0));

        Random random = new Random();

        for (int i = 0; i < size / 2; i++){
            int a = random.nextInt(size - 1);
            int b = random.nextInt(size - 1);

            this.addEdge(a, b);
        }
    }

    public void addEdge(int a, int b) {
        this.edges.get(a).add(b);
    }

    public List<Integer> getNeighboursOf(int node) {
        return this.edges.get(node);
    }

    public List<Integer> getNodes(){
        return nodes;
    }

    public int size() {
        return this.edges.size();
    }

    @Override
    public String toString() {
        return "Graph{" +
                "nodes=" + nodes +
                ", edges=" + edges +
                '}';
    }
}
