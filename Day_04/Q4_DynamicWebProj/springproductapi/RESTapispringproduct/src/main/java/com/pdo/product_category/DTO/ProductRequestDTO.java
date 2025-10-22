package com.pdo.product_category.DTO;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter

public class ProductRequestDTO {
    private String name;
    private Double price;
    private Integer categoryId;

}
