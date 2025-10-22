package com.pdo.product_category.service;

import com.pdo.product_category.DTO.ProductRequestDTO;
import com.pdo.product_category.entity.Category;
import com.pdo.product_category.entity.Product;
import com.pdo.product_category.exception.CategoryNotFoundException;
import com.pdo.product_category.exception.GenericException;
import com.pdo.product_category.exception.NotFoundException;
import com.pdo.product_category.repository.CategoryRepository;
import com.pdo.product_category.repository.ProductRepository;
import org.springframework.stereotype.Service;

@Service
public class ProductService {

    private final ProductRepository productRepository;
    private final CategoryRepository categoryRepository;

    public ProductService(ProductRepository productRepository, CategoryRepository categoryRepository) {
        this.productRepository = productRepository;
        this.categoryRepository = categoryRepository;
    }

    public Product saveProduct(ProductRequestDTO requestDTO) {
        Category category = categoryRepository.findById(requestDTO.getCategoryId())
                .orElseThrow(() -> new CategoryNotFoundException("Category Not found with ID. ----Service"));

        Product product = Product.builder()
                .name(requestDTO.getName())
                .price(requestDTO.getPrice())
                .category(category)
                .build();
        return productRepository.saveAndFlush(product);
    }

    public Product findProductById(Integer id) {
        return productRepository.findById(id)
                .orElseThrow(() -> new GenericException("Product ID not found. service--- " + id));
    }

    public Product updateById(Integer id, Product product) {
        Product productEntity = productRepository.findById(id)
                .orElseThrow(() -> new NotFoundException("Product ID not found. ---service"));

        Product productUpdate = Product.builder()
                .name(product.getName() != null ?
                        product.getName() : productEntity.getName())
                .price(product.getPrice() != null ?
                        product.getPrice() : productEntity.getPrice())
                .category(product.getCategory() != null ?
                        product.getCategory() : productEntity.getCategory())
                .id(productEntity.getId())
                .build();

        return productRepository.saveAndFlush(productUpdate);
    }

    public Product deleteId(Integer id) {
        Product product = productRepository.findById(id)
                .orElseThrow(() -> new NotFoundException("Product ID not found with id: " + id));

        productRepository.deleteById(id);
        return product;
    }

}