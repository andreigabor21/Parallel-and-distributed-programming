package ro.ubb.pdp;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Future;

import static ro.ubb.pdp.Config.NR_TASKS;

public class IDAStar {

    private final ExecutorService executorService;

    public IDAStar(ExecutorService executorService) {
        this.executorService = executorService;
    }

    public Matrix solve(Matrix root) throws ExecutionException, InterruptedException {
        long time = System.currentTimeMillis();
        int minBound = root.getManhattan();
        int distance;
        while (true) {
            Pair<Integer, Matrix> solution = searchParallel(root, 0, minBound, NR_TASKS);
            distance = solution.getFirst();
            if (distance == -1) {
                System.out.println("Solution found in " + solution.getSecond().getNumOfSteps() + " steps");
                System.out.println("Execution time: " + (System.currentTimeMillis() - time) + "ms");
                return solution.getSecond();
            } else {
                System.out.println("Depth " + distance + " reached in " + (System.currentTimeMillis() - time) + "ms");
            }
            minBound = distance;
        }
    }

    public Pair<Integer, Matrix> searchParallel(Matrix current, int numSteps, int bound, int nrThreads) throws ExecutionException, InterruptedException {
        if (nrThreads <= 1) {
            return search(current, numSteps, bound);
        }

        int estimation = numSteps + current.getManhattan();
        if (estimation > bound) {
            return new Pair<>(estimation, current);
        }
        if (estimation > 80) {
            return new Pair<>(estimation, current);
        }
        if (current.getManhattan() == 0) {
            return new Pair<>(-1, current);
        }
        int min = Integer.MAX_VALUE;
        List<Matrix> moves = current.generateMoves();
        List<Future<Pair<Integer, Matrix>>> futures = new ArrayList<>();
        for (Matrix next : moves) {
            Future<Pair<Integer, Matrix>> f = executorService.submit(() -> searchParallel(next, numSteps + 1, bound, nrThreads / moves.size()));
            futures.add(f);
        }
        for (Future<Pair<Integer, Matrix>> f : futures) {
            Pair<Integer, Matrix> result = f.get();
            int t = result.getFirst();
            if (t == -1) {
                return new Pair<>(-1, result.getSecond());
            }
            if (t < min) {
                min = t;
            }
        }
        return new Pair<>(min, current);
    }

    public Pair<Integer, Matrix> search(Matrix current, int numSteps, int bound) {
        int estimation = numSteps + current.getManhattan();
        if (estimation > bound) {
            return new Pair<>(estimation, current);
        }
        if (estimation > 80) {
            return new Pair<>(estimation, current);
        }
        if (current.getManhattan() == 0) {
            return new Pair<>(-1, current);
        }
        int min = Integer.MAX_VALUE;
        Matrix solution = null;
        for (Matrix next : current.generateMoves()) {
            Pair<Integer, Matrix> result = search(next, numSteps + 1, bound);
            int t = result.getFirst();
            if (t == -1) {
                return new Pair<>(-1, result.getSecond());
            }
            if (t < min) {
                min = t;
                solution = result.getSecond();
            }
        }
        return new Pair<>(min, solution);

    }
}
