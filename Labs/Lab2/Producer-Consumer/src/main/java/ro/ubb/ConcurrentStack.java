package ro.ubb;

import java.util.ArrayDeque;
import java.util.Deque;

/**
 * Implementation of a thread safe stack data structure.
 *
 * @author Andrei Gabor
 */
public class ConcurrentStack {

    private final Deque<Integer> stack;
    private static final Object lock = new Object();
    private final int size;

    /**
     * Construct a ConcurrentStack object, with the given size.
     *
     * @param size the maximum size of the stack.
     */
    public ConcurrentStack(int size) {
        this.size = size;
        this.stack = new ArrayDeque<>(size);
    }

    /**
     * Adds a new item to the stack.
     *
     * @param toAdd the element to add
     * @throws InterruptedException when the thread is interrupted
     */
    void addItem(Integer toAdd) throws InterruptedException {
        synchronized (lock) {
            if (stack.size() == size) {
                lock.wait();
            }
            stack.addFirst(toAdd);
            Logger.producerLog("Add to stack item: " + toAdd);
            lock.notifyAll();
        }
    }

    /**
     * Removes and returns the first item in the stack.
     *
     * @return the top of the stack
     * @throws InterruptedException when the thread is interrupted
     */
    Integer getNextItem() throws InterruptedException {
        synchronized (lock) {
            if (stack.size() == 0) {
                lock.wait();
            }
            Integer removed = stack.removeFirst();
            Logger.consumerLog("Removed from stack: " + removed);
            lock.notifyAll();
            return removed;
        }
    }
}
