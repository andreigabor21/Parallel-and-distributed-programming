package ro.ubb.logic;

import ro.ubb.model.Bill;

import java.util.List;

public class ProcessBillsThread implements Runnable {

    List<Bill> bills;
    Shop supermarket;
    int threadNumber;

    public ProcessBillsThread(List<Bill> bills, Shop supermarket, int threadNumber) {
        this.bills = bills;
        this.supermarket = supermarket;
        this.threadNumber = threadNumber;
    }

    @Override
    public void run() {
        for (Bill bill : bills) {
            supermarket.parseBill(bill);
        }
    }
}
