package ro.ubb.logic;

public class CheckEverythingThread implements Runnable {

    Shop supermarket;
    int threadNumber;

    public CheckEverythingThread(Shop supermarket, int threadNumber) {
        this.supermarket = supermarket;
        this.threadNumber = threadNumber;
    }

    @Override
    public void run() {
        System.out.println("Thread " + threadNumber + " is checking if the supermarket is ok");
        boolean ok = supermarket.checkConsistency();
        if (ok)
            System.out.println("ok!");
        else System.out.println("not ok :/");
    }
}
