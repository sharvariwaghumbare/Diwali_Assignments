package com.pdo.product_category.entity;

import com.fasterxml.jackson.annotation.JsonIgnore;
import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.*;

import java.util.List;

@Getter
@Setter
@AllArgsConstructor
@NoArgsConstructor
@Builder
@Table(name = "categoria")//alterar nome
@Entity

public class Category {

    @Id
    @GeneratedValue(strategy = GenerationType.AUTO)
    private Integer id;

    @NotNull
    @Column(name = "name", unique = true)
    private String name;

    @OneToMany(mappedBy = "category", fetch = FetchType.LAZY)//ACHO QUE E PARA CARREGAR AUTOMATICO, TIRAR PARA TESTE
    @JsonIgnore// quebra galho para nao listar infinito
    private List<Product> products;

}
