package com.pdo.product_category.service;

import com.pdo.product_category.DTO.CategoryRequestDTO;
import com.pdo.product_category.entity.Category;
import com.pdo.product_category.exception.ConflictException;
import com.pdo.product_category.exception.GenericException;
import com.pdo.product_category.exception.NotFoundException;
import com.pdo.product_category.repository.CategoryRepository;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;

@Service
public class CategoryService {
    private final CategoryRepository repository;

    public CategoryService(CategoryRepository repository) {
        this.repository = repository;
    }

    public Category saveCategory(CategoryRequestDTO requestDTO) {
        if (repository.findByName(requestDTO.getName()).isPresent()) {
            throw new ConflictException("Category already exists: -----service ");
        }
        Category category = Category.builder()
                .name(requestDTO.getName())
                .products(requestDTO.getProducts())
                .build();

        return repository.saveAndFlush(category);
    }

    public Category findByname(String name) {
        return repository.findByName(name).orElseThrow(
                () -> new GenericException("Category not found ---service")
        );
    }

    public List<Category> findAllCategories() {
        return repository.findAll();
    }

    public Category updateById(Integer id, Category category) { //ID do OBJ, User o novo usuario digitado no postman
        Category categoryEntity = repository.findById(id)
                .orElseThrow(() -> new NotFoundException("category ID not found: ---service"));
//ENTENDER OQ TA FAZENDO.
        categoryEntity.setName(
                category.getName() != null ? category.getName() : categoryEntity.getName()
        );

        categoryEntity.setProducts(
                category.getProducts() != null && !category.getProducts().isEmpty()
                        ? new ArrayList<>(category.getProducts())
                        : categoryEntity.getProducts()
        );

        return repository.saveAndFlush(categoryEntity);
    }

}
