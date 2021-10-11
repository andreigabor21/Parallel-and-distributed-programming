package ro.ubb.model;

import lombok.Data;

import java.util.ArrayList;
import java.util.List;

@Data
public class Bill {

    private List<Product> products;
    private List<Integer> quantitiesSold;

    public Bill() {
        products = new ArrayList<>();
        quantitiesSold = new ArrayList<>();
    }

    public void addLine(Product product, int quantitySold) {
        products.add(product);
        quantitiesSold.add(quantitySold);
    }

    public int getMoney() {
        int sum = 0;
        for(int i = 0; i < products.size(); i++) {
            sum += products.get(i).getPrice() * quantitiesSold.get(i);
        }
        return sum;
    }
}