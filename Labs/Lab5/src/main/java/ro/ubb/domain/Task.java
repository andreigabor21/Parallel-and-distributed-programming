package ro.ubb.domain;

public class Task implements Runnable {

    private final int start;
    private final int end;
    private final Polynomial firstPolynomial;
    private final Polynomial secondPolynomial;
    private final Polynomial result;

    public Task(int start, int end, Polynomial firstPolynomial, Polynomial secondPolynomial, Polynomial result) {
        this.start = start;
        this.end = end;
        this.firstPolynomial = firstPolynomial;
        this.secondPolynomial = secondPolynomial;
        this.result = result;
    }

    @Override
    public void run() {
        for (int index = start; index < end; index++) {
            //case - no more elements to calculate
            if (index > result.getLength()) {
                return;
            }
            //find all the pairs that we add to obtain the value of a result coefficient
            for (int j = 0; j <= index; j++) {
                if (j < firstPolynomial.getLength() && (index - j) < secondPolynomial.getLength()) {
                    int value = firstPolynomial.getCoefficients().get(j) * secondPolynomial.getCoefficients().get(index - j);
                    result.getCoefficients().set(index, result.getCoefficients().get(index) + value);
                }
            }
        }
    }
}
