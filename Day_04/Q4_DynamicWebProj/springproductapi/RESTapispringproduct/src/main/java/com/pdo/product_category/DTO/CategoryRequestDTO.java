package com.pdo.product_category.DTO;

import com.pdo.product_category.entity.Product;
import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter
@Setter

public class CategoryRequestDTO {
    private String name;
    private List<Product> products;
}
