package ro.ubb.model;

import lombok.Data;

import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

@Data
public class Product {

    private String name;
    private int price;
    private int totalQuantity;
    private final Lock lock;

    public Product() {
        lock = new ReentrantLock();
    }

    public Product(String name, int price, int quantity) {
        this.name = name;
        this.price = price;
        this.totalQuantity = quantity;
        lock = new ReentrantLock();
    }
}