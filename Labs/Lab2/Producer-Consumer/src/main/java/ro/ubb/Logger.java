package ro.ubb;

/**
 * Helper class for printing on the standard output.
 *
 * @author Andrei Gabor
 */
public class Logger {

    /**
     * Prints the message on the standard output(as a Producer).
     *
     * @param message to print
     */
    static void producerLog(String message) {
        System.out.printf("Thread[%s] Producer: %s\n", Thread.currentThread().getName(), message);
    }

    /**
     * Prints the message on the standard output(as a Consumer).
     *
     * @param message to print
     */
    static void consumerLog(String message) {
        System.out.printf("Thread[%s] Consumer: %s\n", Thread.currentThread().getName(), message);
    }
}
