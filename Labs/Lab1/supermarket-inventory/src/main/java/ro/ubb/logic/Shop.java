package ro.ubb.logic;

import lombok.Data;
import ro.ubb.exception.NoSuchProductException;
import ro.ubb.model.Bill;
import ro.ubb.model.Product;

import java.util.*;

import static ro.ubb.model.Constants.*;

@Data
public class Shop {

    private List<Product> products;
    private List<Bill> bills;
    private int money;
    private boolean check;

    public Shop() {
        bills = new ArrayList<>();
        money = 0;
        check = false;
    }

    public void makeRandomSales() {
        int billsCount = new Random().nextInt(BILLS_COUNT_UPPER_BOUND) + BILLS_COUNT_LOWER_BOUND;
        for (int i = 0; i < billsCount; i++) {
            makeSales();
//            runRandomCorrectnessCheck();
        }
    }

    private void makeSales() {
        int productsCount = new Random().nextInt(PRODUCTS_COUNT_UPPER_BOUND) + PRODUCTS_COUNT_LOWER_BOUND;
        Bill bill = new Bill();
        for (int i = 0; i < productsCount; i++) {
            sellRandomProduct(bill);
        }
        this.bills.add(bill);
    }

    private void sellRandomProduct(Bill bill) {
        Product productToSell = getRandomProduct();
        int quantityToRemove = getRandomQuantity();
        if(productToSell.getTotalQuantity() >= quantityToRemove) {
            productToSell.getLock().lock();

            productToSell.setTotalQuantity(productToSell.getTotalQuantity() - quantityToRemove);
            this.money += productToSell.getPrice() * quantityToRemove;
            bill.addLine(productToSell, quantityToRemove);

            productToSell.getLock().unlock();
        }
    }

    private void runRandomCorrectnessCheck() {
        Random random = new Random();
        if (random.nextInt(10) == 0) {
            verifySum();
        }
    }

    public void verifySum() {
        System.err.println("Verifying the money...");
        int moneyFromBills = bills.stream()
                .mapToInt(Bill::getMoney)
                .sum();
        if (moneyFromBills != money) {
            System.err.println("Money verification failed!");
        } else {
            System.err.println("Money verification is good!");
        }
    }

    private Product getRandomProduct() {
        return getRandomItemFromList(products);
    }

    private Product getRandomItemFromList(List<Product> items) {
        Random random = new Random();
        int randomIndex = random.nextInt(items.size());
        return items.get(randomIndex);
    }

    private int getRandomQuantity() {
        return new Random().nextInt(QUANTITY_UPPER_BOUND) + QUANTITY_LOWER_BOUND;
    }

    public List<Bill> getBills() {
        return bills;
    }

    public int getMoney() {
        return money;
    }
}
