package ro.ubb.logic;

import ro.ubb.model.ConcurrentProduct;

import java.io.BufferedInputStream;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.util.ArrayList;
import java.util.List;
import java.util.Scanner;

public class InputPreparator {

    public static List<ConcurrentProduct> getProducts() {
        Scanner reader;
        try {
            reader = new Scanner(new BufferedInputStream(new FileInputStream("src/main/resources/data.txt")));
        } catch (FileNotFoundException e) {
            return new ArrayList<>();
        }
        List<ConcurrentProduct> products = new ArrayList<>();
        int count = reader.nextInt();
        for (int i = 0; i < count; ++i) {
            String name = reader.next();
            int quantity = reader.nextInt();
            int price = reader.nextInt();
            products.add(new ConcurrentProduct(name, quantity, price));
        }
        return products;
    }
}
