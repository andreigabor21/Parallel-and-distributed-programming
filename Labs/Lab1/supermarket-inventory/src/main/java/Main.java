import ro.ubb.logic.InputPreparator;
import ro.ubb.model.Inventory;
import ro.ubb.model.Product;
import ro.ubb.logic.Shop;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

public class Main {

    public static void main(String[] args) throws InterruptedException, IOException {
        Inventory inventory = InputPreparator.getFromFile();
//        System.out.println(inventory);

        Shop shop = new Shop(inventory);

        List<Thread> threads = new ArrayList<>();
        for (int i = 0; i < 5; i++) {
            threads.add(new Thread(shop));
        }
        for (int i = 0; i < 3; i++) {
            threads.get(i).start();
        }
        for (int i = 0; i < 3; i++) {
            threads.get(i).join();
        }
        shop.verifySum();
    }
}
