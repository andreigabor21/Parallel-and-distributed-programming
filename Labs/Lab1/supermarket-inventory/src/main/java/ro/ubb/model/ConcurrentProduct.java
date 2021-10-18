package ro.ubb.model;

import lombok.Data;
import lombok.ToString;

import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

/**
 * Extended product having a lock for concurrency issues.
 *
 * @author Andrei Gabor
 */
@Data
@ToString(callSuper = true)
public class ConcurrentProduct extends Product {

    private int soldQuantity;
    private final Lock mutex;

    public ConcurrentProduct(String name, int price, int quantity) {
        super(name, quantity, price);
        this.mutex = new ReentrantLock();
        this.soldQuantity = 0;
    }
}
