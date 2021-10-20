package ro.ubb;

import java.util.List;

/**
 * The producer class
 *
 * @author Andrei Gabor
 */
public class Producer implements Runnable {

    private final List<Integer> firstVector;
    private final List<Integer> secondVector;
    private final ConcurrentStack stack;
    private final int numberOfTasks;

    /**
     * Constructs a Producer object having the given stack.
     *
     * @param firstVector the first vector
     * @param secondVector the first vector
     * @param stack         the given stack
     * @param numberOfTasks number of items to insert into the stack
     */
    public Producer(List<Integer> firstVector, List<Integer> secondVector, ConcurrentStack stack, int numberOfTasks) {
        this.firstVector = firstVector;
        this.secondVector = secondVector;
        this.stack = stack;
        this.numberOfTasks = numberOfTasks;
    }

    /**
     * Produces numberOfTasks items from the stack.
     */
    @Override
    public void run() {
        for (int i = 0; i < numberOfTasks; i++) {
            try {
                int product = firstVector.get(i) * secondVector.get(i);
                stack.addItem(product);
            } catch (InterruptedException e) {
                System.err.println(e.getMessage());
            }
        }
        Logger.producerLog("Producer finished");
    }
}
