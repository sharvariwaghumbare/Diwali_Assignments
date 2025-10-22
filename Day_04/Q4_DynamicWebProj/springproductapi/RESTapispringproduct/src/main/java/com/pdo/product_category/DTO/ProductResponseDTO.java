package com.pdo.product_category.DTO;

import lombok.Builder;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
@Builder

public class ProductResponseDTO {
    private Integer id;
    private String name;
    private Double price;
    private String categoryName;
}
