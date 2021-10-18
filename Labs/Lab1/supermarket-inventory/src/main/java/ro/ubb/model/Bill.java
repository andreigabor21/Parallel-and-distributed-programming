package ro.ubb.model;

import lombok.Getter;
import lombok.RequiredArgsConstructor;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

/**
 * Class representation of a bill from the shop.
 *
 * @author Andrei Gabor
 */
@RequiredArgsConstructor
@Getter
public class Bill {

    private final List<Product> products;

    /**
     * Generates a list of random bills.
     *
     * @param products list of products be randomly picked
     * @param numberOfBills number of bills to generate
     * @return the list of generated bills
     */
    public static List<Bill> getRandomBills(List<ConcurrentProduct> products, int numberOfBills) {
        Random random = new Random();
        List<Bill> bills = new ArrayList<>();

        for (int i = 0; i < numberOfBills; ++i) {
            List<Product> billsProducts = new ArrayList<>();
            int productsInBill = random.nextInt(5) + 1;
            for (int j = 0; j < productsInBill; ++j) {
                int productIndex = random.nextInt(products.size());
                Product randomProduct = products.get(productIndex);

                String name = randomProduct.getName();
                int price = randomProduct.getPrice();
                int quantity = random.nextInt(5) + 1;
                billsProducts.add(new Product(name, quantity, price));
            }
            bills.add(new Bill(billsProducts));
        }
        return bills;
    }
}
