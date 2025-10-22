package com.pdo.product_category.controller;

import com.pdo.product_category.DTO.CategoryRequestDTO;
import com.pdo.product_category.DTO.CategoryResponseDTO;
import com.pdo.product_category.entity.Category;
import com.pdo.product_category.service.CategoryService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/categoria")
@RequiredArgsConstructor

public class CategoryController {

    private final CategoryService categoryService;

    @PostMapping
    public ResponseEntity<CategoryResponseDTO> saveCategory(@Valid @RequestBody CategoryRequestDTO requestDTO) {
        Category category = categoryService.saveCategory(requestDTO);
        CategoryResponseDTO responseDTO = CategoryResponseDTO.builder()
                .id(category.getId())
                .name(category.getName())
                .products(category.getProducts())
                .build();
        return ResponseEntity.ok(responseDTO);
    }

    @GetMapping("/search")
    public ResponseEntity<CategoryResponseDTO> findCategory(@RequestParam String name) {
        Category category = categoryService.findByname(name);

        CategoryResponseDTO responseDTO = CategoryResponseDTO.builder()
                .id(category.getId())
                .name(category.getName())
                .products(category.getProducts())
                .build();

        return ResponseEntity.ok(responseDTO);
    }

    @GetMapping
    public ResponseEntity<List<CategoryResponseDTO>> findAllCategory() {
        List<Category> categories = categoryService.findAllCategories();

        List<CategoryResponseDTO> responseDTOS = categories.stream()
                .map(category -> CategoryResponseDTO.builder()
                        .id(category.getId())
                        .name(category.getName())
                        .products(category.getProducts())
                        .build())
                .toList();

        return ResponseEntity.ok(responseDTOS);
    }

    @PutMapping
    public ResponseEntity<CategoryResponseDTO> updateById(@RequestParam Integer id, @RequestBody Category category) {
        Category update = categoryService.updateById(id, category);

        CategoryResponseDTO responseDTO = CategoryResponseDTO.builder()
                .id(update.getId())
                .name(update.getName())
                .build();

        return ResponseEntity.ok(responseDTO);
    }

}