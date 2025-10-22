package com.pdo.product_category.exception;

public class NotFoundException extends RuntimeException {
    public NotFoundException(String message) {

        super(message);
    }
}
