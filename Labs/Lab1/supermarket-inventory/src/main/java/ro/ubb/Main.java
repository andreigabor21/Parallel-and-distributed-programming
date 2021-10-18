package ro.ubb;

import ro.ubb.logic.CheckEverythingThread;
import ro.ubb.logic.InputPreparator;
import ro.ubb.logic.ProcessBillsThread;
import ro.ubb.logic.Shop;
import ro.ubb.model.Bill;
import ro.ubb.model.ConcurrentProduct;

import java.util.ArrayList;
import java.util.List;

public class Main {
    public static void main(String[] args) throws InterruptedException {

        List<ConcurrentProduct> products = InputPreparator.getProducts();
        Shop supermarket = new Shop(products);
        List<Bill> bills = Bill.getRandomBills(products, products.size() / 5);

        int threadNumber = 50;
        int iterations = (int) Math.ceil((double) bills.size() / (double) threadNumber);

        List<Thread> threads = new ArrayList<>();
        int checkThreadNumber = threadNumber;
        for (int i = 0; i < threadNumber; ++i) {
            int start = i * iterations;
            int end = Math.min((i + 1) * iterations, bills.size());

            threads.add(new Thread(new ProcessBillsThread(bills.subList(start, end), supermarket, i)));
            if (i % 10 == 0) {
                threads.add(new Thread(new CheckEverythingThread(supermarket, checkThreadNumber)));
                checkThreadNumber++;
            }
        }
        for (Thread thread : threads)
            thread.start();
        for (Thread thread : threads)
            thread.join();
        new CheckEverythingThread(supermarket, checkThreadNumber).run();
    }
}
