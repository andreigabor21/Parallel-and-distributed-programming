package ro.ubb.model;

public class Bill extends Inventory {

    public int getMoneyFromBill() {
        return this.getProducts().stream()
                .mapToInt(product -> product.getPrice() * this.getQuantityOfProduct(product))
                .sum();
    }
}