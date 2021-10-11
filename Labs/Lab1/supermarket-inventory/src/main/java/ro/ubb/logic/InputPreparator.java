package ro.ubb.logic;

import org.apache.commons.lang3.RandomStringUtils;
import ro.ubb.model.Product;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class InputPreparator {

    public static void writeProductsToFile() throws IOException {
        Path file = Paths.get("src/main/resources/data.txt");
        for (int i = 0; i < 1000; i++) {
            Files.write(file, generateProductString().getBytes(), StandardOpenOption.APPEND);
        }
    }

    public static void getFromFile(Shop shop) throws IOException {
        List<Product> products = new ArrayList<>();
        Files.readAllLines(Paths.get("src/main/resources/data.txt"))
                .forEach(line -> {
                    String[] tokens = line.split(" ");
                    Product product = new Product(tokens[0], Integer.parseInt(tokens[1]), Integer.parseInt(tokens[2]));
                    products.add(product);
                });
        shop.setProducts(products);
    }

    private static String generateProductString() {
        String productName = RandomStringUtils.randomAlphabetic(new Random().nextInt(8) + 4);
        int price = new Random().nextInt(200) + 1;
        int quantity = new Random().nextInt(50) + 1;

        StringBuilder builder = new StringBuilder();
        builder.append(productName)
                .append(" ")
                .append(price)
                .append(" ")
                .append(quantity)
                .append("\n");
        return builder.toString();
    }
}
