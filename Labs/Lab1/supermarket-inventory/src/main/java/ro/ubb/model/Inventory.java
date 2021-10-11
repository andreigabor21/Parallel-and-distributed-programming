package ro.ubb.model;

import ro.ubb.exception.NoSuchProductException;

import java.util.*;

public class Inventory {

    private final Map<Product, Integer> quantities;

    public Inventory() {
        quantities = new HashMap<>();
    }

    public void add(Product product, int quantity) {
        if (!quantities.containsKey(product)) {
            quantities.put(product, quantity);
        } else {
            Integer currentQuantity = quantities.get(product);
            quantities.replace(product, currentQuantity + quantity);
        }
    }

    public void remove(Product product, int quantity) {
        if (!quantities.containsKey(product)) {
            throw new NoSuchProductException("Product not in inventory");
        } else {
            Integer currentQuantity = quantities.get(product);
            currentQuantity -= quantity;
            quantities.replace(product, currentQuantity <= 0 ? 0 : currentQuantity);
        }
    }

    @Override
    public String toString() {
        StringBuilder builder = new StringBuilder("Product Name | Price | Inventory Count \n");
        for (Product product : quantities.keySet()) {
            builder.append(product.getName())
                    .append("               ")
                    .append(product.getPrice())
                    .append("            ")
                    .append(quantities.get(product))
                    .append("\n");
        }
        return builder.toString();
    }

    public Set<Product> getProducts() {
        return quantities.keySet();
    }

    public int getQuantityOfProduct(Product product) {
        return quantities.get(product);
    }
}