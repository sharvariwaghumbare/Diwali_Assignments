package com.pdo.product_category.DTO;

import com.pdo.product_category.entity.Product;
import lombok.Builder;
import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter
@Setter
@Builder

public class CategoryResponseDTO {
    private Integer id;
    private String name;
    private List<Product> products;
}
