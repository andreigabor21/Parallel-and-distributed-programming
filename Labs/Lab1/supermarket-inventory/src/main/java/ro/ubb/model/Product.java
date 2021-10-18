package ro.ubb.model;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * Class representation of a product from the shop.
 *
 * @author Andrei Gabor
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
public class Product {

    private String name;
    private int totalQuantity;
    private int price;
}
