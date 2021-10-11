package ro.ubb.exception;

public class NoSuchProductException extends RuntimeException {

    public NoSuchProductException(String message) {
        super(message);
    }
}
