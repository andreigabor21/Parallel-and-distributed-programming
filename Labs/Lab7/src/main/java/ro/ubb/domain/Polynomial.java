package ro.ubb.domain;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class Polynomial {

    private final int order;
    private final List<Integer> coefficients;

    public Polynomial(int order) {
        this.order = order;
        coefficients = new ArrayList<>(order + 1);
        generateCoefficients();
    }

    public Polynomial(List<Integer> coefficients) {
        this.order = coefficients.size() - 1;
        this.coefficients = coefficients;
    }

    public int getSize() {
        return coefficients.size();
    }

    private void generateCoefficients() {
        Random random = new Random();
        for (int i = 0; i < order; i++) {
            coefficients.add(random.nextInt(10 + 1));
        }
        coefficients.add(random.nextInt(10) + 1);
    }

    @Override
    public String toString() {
        StringBuilder builder = new StringBuilder();
        int power = -1;
        for (int i = 0; i <= this.order; i++) {
            power++;
            if (coefficients.get(i) == 0) {
                continue;
            }
            builder.append(" ").append(coefficients.get(i)).append("x^").append(power).append(" +");
        }
        builder.deleteCharAt(builder.length() - 1);
        return builder.toString();
    }

    public List<Integer> getCoefficients() {
        return coefficients;
    }

    public int getOrder() {
        return order;
    }
}
