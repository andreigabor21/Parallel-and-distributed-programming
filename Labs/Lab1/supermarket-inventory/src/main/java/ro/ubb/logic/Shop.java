package ro.ubb.logic;

import ro.ubb.exception.NoSuchProductException;
import ro.ubb.model.Bill;
import ro.ubb.model.Inventory;
import ro.ubb.model.Product;

import java.util.*;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

public class Shop implements Runnable {

    private final Inventory inventory;
    private final List<Bill> bills;
    private int money;
    private final Lock lock;

    public Shop(Inventory inventory) {
        this.inventory = inventory;
        bills = new ArrayList<>();
        money = 0;
        lock = new ReentrantLock();
    }

    @Override
    public void run() { //make sales
        int billsCount = new Random().nextInt(40) + 10;
        for (int i = 0; i < billsCount; i++) {
            makeSales();
        }
    }

    private void makeSales() {
        int productsCount = new Random().nextInt(30) + 10;
        Bill bill = new Bill();
        for (int i = 0; i < productsCount; i++) {
            sellRandomProduct(bill);
        }
        lock.lock();
        this.bills.add(bill);
        lock.unlock();
    }

    private void sellRandomProduct(Bill bill) {
        Product productToSell = getRandomProduct();
        int quantityToRemove = getRandomQuantity();
        try {
            lock.lock();
            inventory.remove(productToSell, quantityToRemove);
            this.money += productToSell.getPrice() * quantityToRemove;
            bill.add(productToSell, quantityToRemove);
            lock.unlock();
        } catch (NoSuchProductException ignored) {
        }
    }

    public void verifySum() {
        System.err.println("Verfying the money...");
        int moneyFromBills = bills.stream()
                .mapToInt(Bill::getMoneyFromBill)
                .sum();
        if (moneyFromBills != money) {
            System.err.println("Money verification failed!");
        }
        System.out.println(moneyFromBills);
        System.out.println(money);
    }

    private Product getRandomProduct() {
        Set<Product> products = inventory.getProducts();
        return getRandomItemFromSet(products);
    }

    private Product getRandomItemFromSet(Set<Product> items) {
        Random random = new Random();
        // Generate a random number using nextInt
        // method of the Random class.
        int randomNumber = random.nextInt(items.size() + 1);
        Iterator<Product> iterator = items.iterator();
        int currentIndex = 0;
        Product randomElement = null;
        // iterate the HashSet
        while (iterator.hasNext()) {
            randomElement = iterator.next();
            // if current index is equal to random number
            if (currentIndex == randomNumber)
                return randomElement;
            // increase the current index
            currentIndex++;
        }
        return randomElement;
    }

    private int getRandomQuantity() {
        return new Random().nextInt(5) + 1;
    }

    public List<Bill> getBills() {
        return bills;
    }

    public int getMoney() {
        return money;
    }
}
