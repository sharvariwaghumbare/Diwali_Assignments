package com.pdo.product_category.controller;

import com.pdo.product_category.DTO.ProductRequestDTO;
import com.pdo.product_category.DTO.ProductResponseDTO;
import com.pdo.product_category.entity.Product;
import com.pdo.product_category.service.ProductService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/produto")
@RequiredArgsConstructor

public class ProductController {

    private final ProductService productService;

    @PostMapping
    public ResponseEntity<ProductResponseDTO> saveProduct(@RequestBody ProductRequestDTO requestDTO) {
        Product product = productService.saveProduct(requestDTO);

        ProductResponseDTO productResponseDTO = ProductResponseDTO.builder()
                .id(product.getId())
                .name(product.getName())
                .price(product.getPrice())
                .categoryName(product.getCategory().getName())
                .build();

        return ResponseEntity.ok(productResponseDTO);
    }

    @GetMapping
    public ResponseEntity<ProductResponseDTO> findProductById(@RequestParam Integer id) {
        Product product = productService.findProductById(id);

        ProductResponseDTO productResponseDTO = ProductResponseDTO.builder()
                .id(product.getId())
                .name(product.getName())
                .price(product.getPrice())
                .categoryName(product.getCategory().getName())
                .build();

        return ResponseEntity.ok(productResponseDTO);
    }

    @PutMapping
    public ResponseEntity<ProductResponseDTO> updateById(@RequestParam Integer id, @RequestBody Product product) {
        Product update = productService.updateById(id, product);

        ProductResponseDTO productResponseDTO = ProductResponseDTO.builder()
                .id(update.getId())
                .name(update.getName())
                .price(update.getPrice())
                .categoryName(update.getCategory().getName())
                .build();

        return ResponseEntity.ok(productResponseDTO);
    }

    @DeleteMapping
    public ResponseEntity<ProductResponseDTO> deleteById(@RequestParam Integer id) {
        Product product = productService.deleteId(id);

        ProductResponseDTO productResponseDTO = ProductResponseDTO.builder()
                .id(product.getId())
                .name(product.getName())
                .price(product.getPrice())
                .categoryName(product.getCategory().getName())
                .build();

        return ResponseEntity.ok(productResponseDTO);
    }

}