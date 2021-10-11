import ro.ubb.logic.InputPreparator;
import ro.ubb.logic.Shop;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import static ro.ubb.model.Constants.THREAD_COUNT;

public class Main {

    public static void main(String[] args) throws IOException {
        Shop shop = new Shop();
        InputPreparator.getFromFile(shop);
//        System.out.println(shop.getProducts());

        List<Thread> threads = new ArrayList<>();
        for (int i = 0; i < THREAD_COUNT; i++) {
            threads.add(new Thread(() -> shop.makeRandomSales()));
        }

        long start = System.currentTimeMillis();
        threads.forEach(Thread::start);
        threads.forEach(thread -> {
            try {
                thread.join();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        });
        shop.verifySum();
        long end = System.currentTimeMillis();
        System.out.println("Time elapsed: " + (end-start));
        System.out.println("Total money: " + shop.getMoney());
    }
}
