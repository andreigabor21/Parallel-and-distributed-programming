package ro.ubb.logic;

import ro.ubb.model.Bill;
import ro.ubb.model.ConcurrentProduct;
import ro.ubb.model.Product;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

public class Shop {

    private int money;
    private final List<ConcurrentProduct> products;
    private final Lock moneyMutex;
    private final ReadWriteLock readWriteLock;
    private final List<Bill> consumedBillList;

    /**
     * Constructs a shop object having the given products.
     *
     * @param products the given products
     */
    public Shop(List<ConcurrentProduct> products) {
        this.products = products;
        this.money = 0;
        this.moneyMutex = new ReentrantLock();
        this.readWriteLock = new ReentrantReadWriteLock();
        this.consumedBillList = new ArrayList<>();
    }

    /**
     * Parses a bill, modifying the sold quantity and the money. The read lock is called.
     *
     * @param bill the bill to parse
     */
    public void parseBill(Bill bill) {
        readWriteLock.readLock().lock();
        int totalPrice = 0;
        for (Product billProduct : bill.getProducts())
            for (ConcurrentProduct product : products)
                if (billProduct.getName().equals(product.getName())) {
                    product.getMutex().lock();
                    product.setSoldQuantity(product.getSoldQuantity() + billProduct.getTotalQuantity());
                    totalPrice += product.getPrice() * billProduct.getTotalQuantity();
                    product.getMutex().unlock();
                }
        moneyMutex.lock();
        consumedBillList.add(bill);
        money += totalPrice;
        moneyMutex.unlock();
        readWriteLock.readLock().unlock();
    }

    /**
     * Checks if the amount of money is consistent.
     *
     * @return true if consistent, otherwise false
     */
    public boolean checkConsistency() {
        readWriteLock.writeLock().lock();
        System.out.println("Checked so far " + consumedBillList.size() + " bills");
        int totalPrice = 0;
        for (Bill bill : consumedBillList)
            for (Product product : bill.getProducts()) {
                totalPrice += product.getTotalQuantity() * product.getPrice();
            }
        if (totalPrice != money) {
            readWriteLock.writeLock().unlock();
            return false;
        }
        readWriteLock.writeLock().unlock();
        return true;
    }
}
