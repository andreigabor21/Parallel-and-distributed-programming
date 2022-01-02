package ro.ubb.domain;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;
import java.util.stream.Collectors;
import java.util.stream.IntStream;

public class Polynomial {

    private final List<Integer> coefficients;
    private final int degree;

    public Polynomial(List<Integer> coefficients) {
        this.coefficients = coefficients;
        this.degree = coefficients.size() - 1;
    }

    public Polynomial(int degree) {
        this.degree = degree;
        coefficients = new ArrayList<>(degree + 1);
        generateRandomCoefficients();
    }

    public int getLength() {
        return this.coefficients.size();
    }

    public List<Integer> getCoefficients() {
        return coefficients;
    }

    public int getDegree() {
        return degree;
    }

    private void generateRandomCoefficients() {
        Random random = new Random();
        int maxValue = 10;
        for (int i = 0; i < degree; i++) {
            coefficients.add(random.nextInt(maxValue));
        }
        coefficients.add(random.nextInt(maxValue) + 1);
    }

    public static Polynomial addZeros(Polynomial polynomial, int offset) {
        List<Integer> coefficients = IntStream.range(0, offset)
                .mapToObj(i -> 0)
                .collect(Collectors.toList());
        coefficients.addAll(polynomial.getCoefficients());
        return new Polynomial(coefficients);
    }

    public static Polynomial add(Polynomial firstPolynomial, Polynomial secondPolynomial) {
        int minimumDegree = Math.min(firstPolynomial.getDegree(), secondPolynomial.getDegree());
        int maximumDegree = Math.max(firstPolynomial.getDegree(), secondPolynomial.getDegree());
        List<Integer> newCoefficients = new ArrayList<>(maximumDegree + 1);

        //Add the 2 polynomials
        for (int i = 0; i <= minimumDegree; i++) {
            newCoefficients.add(firstPolynomial.getCoefficients().get(i) + secondPolynomial.getCoefficients().get(i));
        }

        addRemainingCoefficients(firstPolynomial, secondPolynomial, minimumDegree, maximumDegree, newCoefficients);

        return new Polynomial(newCoefficients);
    }


    public static Polynomial subtract(Polynomial firstPolynomial, Polynomial secondPolynomial) {
        int minimumDegree = Math.min(firstPolynomial.getDegree(), secondPolynomial.getDegree());
        int maximumDegree = Math.max(firstPolynomial.getDegree(), secondPolynomial.getDegree());
        List<Integer> newCoefficients = new ArrayList<>(maximumDegree + 1);

        //Subtract the 2 polynomials
        for (int i = 0; i <= minimumDegree; i++) {
            newCoefficients.add(firstPolynomial.getCoefficients().get(i) - secondPolynomial.getCoefficients().get(i));
        }

        addRemainingCoefficients(firstPolynomial, secondPolynomial, minimumDegree, maximumDegree, newCoefficients);

        //remove coefficients starting from the biggest power if coefficient is 0

        int i = newCoefficients.size() - 1;
        while (newCoefficients.get(i) == 0 && i > 0) {
            newCoefficients.remove(i);
            i--;
        }

        return new Polynomial(newCoefficients);
    }

    private static void addRemainingCoefficients(Polynomial p1, Polynomial p2, int minDegree, int maxDegree, List<Integer> newCoefficients) {
        if (minDegree != maxDegree) {
            if (maxDegree == p1.getDegree()) {
                for (int i = minDegree + 1; i <= maxDegree; i++) {
                    newCoefficients.add(p1.getCoefficients().get(i));
                }
            } else {
                for (int i = minDegree + 1; i <= maxDegree; i++) {
                    newCoefficients.add(p2.getCoefficients().get(i));
                }
            }
        }
    }

    @Override
    public String toString() {
        StringBuilder bobTheBuilder = new StringBuilder();
        int power = 0;
        for (int i = 0; i <= this.degree; i++) {
            if (coefficients.get(i) == 0) {
                power++;
                continue;
            }

            bobTheBuilder.append(" ")
                    .append(coefficients.get(i))
                    .append("x^")
                    .append(power)
                    .append(" +");
            power++;
        }
        bobTheBuilder.deleteCharAt(bobTheBuilder.length() - 1); //delete last +

        return bobTheBuilder.toString();
    }
}
