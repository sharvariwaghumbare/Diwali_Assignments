package com.pdo.product_category;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class ProductCategoryApplication {

	public static void main(String[] args) {
        SpringApplication.run(ProductCategoryApplication.class, args);
	}

}

/*

1. Product & Category (clássico CRUD duplo)

Descrição:
Gerenciar produtos e categorias. Cada produto pertence a uma categoria.

Entidades:

Category: id, name

Product: id, name, price, category

Relação:
ManyToOne (muitos produtos → uma categoria)

Diferenciais:

CRUD completo para ambos

Listar produtos por categoria

Mostrar nome da categoria dentro do produto (DTOs)


CATEGORIA
save
findByName
deleteByName
updateByName

*/
