package com.pdo.product_category.exception;

public class ConflictException extends RuntimeException {
    public ConflictException(String message) {

        super(message);
    }
}
