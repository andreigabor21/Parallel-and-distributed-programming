package ro.ubb;

/**
 * The consumer class
 *
 * @author Andrei Gabor
 */
public class Consumer implements Runnable {

    private final ConcurrentStack stack;
    private final int numberOfTasks;
    private int sum;

    /**
     * Constructs a Consumer object having the given stack.
     *
     * @param stack         the given stack
     * @param numberOfTasks number of items to consume from the stack
     */
    public Consumer(ConcurrentStack stack, int numberOfTasks) {
        this.stack = stack;
        this.numberOfTasks = numberOfTasks;
        sum = 0;
    }

    /**
     * Consumes numberOfTasks items from the stack.
     */
    @Override
    public void run() {
        for (int i = 0; i < numberOfTasks; i++) {
            try {
                Integer consumed = stack.getNextItem();
                sum += consumed;
            } catch (InterruptedException e) {
                System.err.println(e.getMessage());
            }
        }
        Logger.consumerLog("Consumer finished");
    }

    public int getSum() {
        return sum;
    }
}
