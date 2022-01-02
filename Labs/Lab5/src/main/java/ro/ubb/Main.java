package ro.ubb;

import ro.ubb.domain.Operation;
import ro.ubb.domain.Polynomial;

import java.util.concurrent.ExecutionException;

public class Main {

    public static void main(String[] args) throws InterruptedException, ExecutionException {
        // write your code here
        Polynomial p = new Polynomial(10);
        Polynomial q = new Polynomial(10);

        System.out.println("P:" + p);
        System.out.println("Q:" + q);
        System.out.println("\n");

        //Simple
        simpleSequential(p, q);
        simpleThreaded(p, q);

        //Karatsuba
        karatsubaSequential(p, q);
        karatsubaThreaded(p, q);

    }

    private static void simpleSequential(Polynomial p, Polynomial q) {
        long startTime = System.currentTimeMillis();
        Polynomial result1 = Operation.simpleSequential(p, q);
        long endTime = System.currentTimeMillis();
        System.out.println("Simple sequential multiplication of polynomials: ");
        System.out.println("Execution time: " + (endTime - startTime) + " ms");
//        System.out.println(result1);
    }

    private static void simpleThreaded(Polynomial p, Polynomial q) throws InterruptedException {
        long startTime = System.currentTimeMillis();
        Polynomial result2 = Operation.simpleThreaded(p, q);
        long endTime = System.currentTimeMillis();
        System.out.println("Simple parallel multiplication of polynomials: ");
        System.out.println("Execution time: " + (endTime - startTime) + " ms");
//        System.out.println(result2);
    }

    private static void karatsubaSequential(Polynomial p, Polynomial q) {
        long startTime = System.currentTimeMillis();
        Polynomial result3 = Operation.karatsubaSequential(p, q);
        long endTime = System.currentTimeMillis();
        System.out.println("Karatsuba sequential multiplication of polynomials: ");
        System.out.println("Execution time: " + (endTime - startTime) + " ms");
//        System.out.println(result3);
    }

    private static void karatsubaThreaded(Polynomial p, Polynomial q) throws ExecutionException,
            InterruptedException {
        long startTime = System.currentTimeMillis();
        Polynomial result4 = Operation.karatsubaThreaded(p, q, 1);
        long endTime = System.currentTimeMillis();
        System.out.println("Karatsuba parallel multiplication of polynomials: ");
        System.out.println("Execution time: " + (endTime - startTime) + " ms");
        System.out.println(result4);
    }
}
