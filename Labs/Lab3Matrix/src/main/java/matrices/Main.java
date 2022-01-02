package matrices;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Random;
import java.util.concurrent.Executors;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicInteger;

import static matrices.Constants.*;


public class Main {

    private static List<List<Integer>> result;

    public static void main(String[] args) throws InterruptedException {
        List<List<Integer>> firstMatrix, secondMatrix, coordinates;
        firstMatrix = initMatrix(MATRIX_ONE_ROWS, Constants.MATRIX_ONE_COLUMNS);
        secondMatrix = initMatrix(MATRIX_TWO_ROWS, MATRIX_TWO_COLUMNS);

        result = initMatrix(MATRIX_ONE_ROWS, MATRIX_TWO_COLUMNS);

        coordinates = initCoordinates(MATRIX_ONE_ROWS, MATRIX_TWO_COLUMNS);
//        System.out.println(coordinates);

        List<Thread> threads = new ArrayList<>();
        int batchSize = coordinates.size() / THREAD_COUNT;
        List<List<List<Integer>>> batches = new ArrayList<>();
        for (int i = 0; i <= coordinates.size() - batchSize; i += batchSize) {
            batches.add(new ArrayList<>(coordinates.subList(i, i + batchSize)));
        }

        int remaining = coordinates.size() % THREAD_COUNT;
        for (int i = 0; i < remaining; i++) {
            var remain = coordinates.get(coordinates.size() - i - 1);
            batches.get(i).add(remain);
        }

//        System.out.println("Batches: ");
//        batches.forEach(System.out::println);

//        1. Create an actual thread for each task (use the low-level thread mechanism from the programming language);

//        long startTime = System.currentTimeMillis();
//        batches.forEach(batch -> {
//                threads.add(new Thread(() -> {
//                    compute(firstMatrix, secondMatrix, batch);
//                }));
//            });
//        threads.forEach(Thread::start);
//        threads.forEach(thread -> {
//            try {
//                thread.join();
//            } catch (InterruptedException e) {
//                e.printStackTrace();
//            }
//        });
//        long endTime = System.currentTimeMillis();
//        System.out.println(endTime - startTime);

//        2. Use a thread pool

        long startTime = System.currentTimeMillis();
        ThreadPoolExecutor threadPoolExecutor = (ThreadPoolExecutor) Executors.newFixedThreadPool(THREAD_COUNT);
        coordinates.forEach(coordinate -> {
            threadPoolExecutor.submit(() -> {
                computeTask(firstMatrix, secondMatrix, coordinate);
            });
        });

        threadPoolExecutor.shutdown();
        threadPoolExecutor.awaitTermination(300, TimeUnit.SECONDS);

        result.forEach(System.out::println);
        long endTime = System.currentTimeMillis();
        System.out.println(endTime - startTime);
    }

    private static void computeTask(List<List<Integer>> matrix1, List<List<Integer>> matrix2, List<Integer> coordinate) {
        AtomicInteger product = new AtomicInteger();
        product.set(0);
        int row = coordinate.get(0);
        int column = coordinate.get(1);
        for (int i = 0; i < Constants.MATRIX_ONE_COLUMNS; i++) {
            product.addAndGet(matrix1.get(row).get(i) * matrix2.get(i).get(column));
        }
        result.get(row).set(column, product.get());
    }

    private static void compute(List<List<Integer>> matrix1, List<List<Integer>> matrix2, List<List<Integer>> batch) {
        AtomicInteger product = new AtomicInteger();
        batch.forEach(pair -> {
            product.set(0);
            int row = pair.get(0);
            int column = pair.get(1);
            for (int i = 0; i < Constants.MATRIX_ONE_COLUMNS; i++) {
                product.addAndGet(matrix1.get(row).get(i) * matrix2.get(i).get(column));
            }
            result.get(row).set(column, product.get());
        });
    }

    private static List<List<Integer>> initCoordinates(int resultMatrixRows, int resultMatrixColumns) {
        List<List<Integer>> coordinates = new ArrayList<>();
        for (int i = 0; i < resultMatrixRows; i++) {
            for (int j = 0; j < resultMatrixColumns; j++) {
                coordinates.add(new ArrayList<>(Arrays.asList(i, j)));
            }
        }
        return coordinates;
    }

    private static List<List<Integer>> initMatrix(int matrixOneRows, int matrixOneColumns) {
        List<List<Integer>> matrix = new ArrayList<>();
        Random random = new Random();
        for (int i = 0; i < matrixOneRows; i++) {
            matrix.add(new ArrayList<>());
            for (int j = 0; j < matrixOneColumns; j++) {
//                matrix.get(i).add(random.nextInt());
                matrix.get(i).add(1);
            }
        }
        return matrix;
    }
}