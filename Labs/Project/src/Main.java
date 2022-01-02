import ro.ubb.pdp.IDAStar;
import ro.ubb.pdp.Matrix;

import java.io.IOException;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import static ro.ubb.pdp.Config.NR_THREADS;


public class Main {

    public static void main(String[] args) throws IOException, InterruptedException, ExecutionException {
        Matrix initialState = Matrix.readFromFile();

        ExecutorService executorService = Executors.newFixedThreadPool(NR_THREADS);
        IDAStar idaStar = new IDAStar(executorService);

        Matrix solution = idaStar.solve(initialState);
        System.out.println(solution);
        executorService.shutdown();
        executorService.awaitTermination(1000000, TimeUnit.SECONDS);
    }
}
