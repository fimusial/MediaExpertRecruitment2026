# EzCatalog UI
TODO!

## Tech stack
TODO!

# EzCatalog API
REST API serving a catalog of products: paged browsing with a cursor, plus creating and updating single products.
Responses are hypermedia-driven, clients enter at `/` and follow the `_links` of each resource.
Data lives in an in-memory database seeded sample products on startup, so no database setup is needed to run it.

## Tech stack
 - .NET 10
 - Layered (DDD): Domain, Application, Infrastructure, WebAPI
 - MediatR (commands, queries, pipeline behaviors), FluentValidation
 - Entity Framework Core (in-memory provider)
 - OpenAPI 3.1 + Swagger UI, HATEOAS
 - xUnit v3, FluentAssertions, NSubstitute
 - StyleCop and .NET analyzers, warnings treated as errors
 - Docker

## Requirements
 - .NET 10 SDK (required)
 - Docker (optional)

## Run
```bash
dotnet run --project .\WebAPI\WebAPI.csproj
```

## API URLs
 - WebAPI root: https://localhost:7171
 - Swagger UI: https://localhost:7171/swagger
 - OpenAPI: https://localhost:7171/openapi/v1.json


## API examples

### Get the API entry point
`GET /`
```
Response:
{
    "_links": {
        "self": {
            "href": "https://localhost:7171/",
            "method": "GET"
        },
        "products": {
            "href": "https://localhost:7171/products?limit=20",
            "method": "GET"
        }
    }
}
```

### Get a page of products
`GET /products?cursor=01a0a76a-0520-7000-85c0-0af1b9866e5e&limit=10`
```
Response:
{
    "totalCount": 1000,
    "nextCursor": "01a0a76a-0520-7000-85b6-dc988ded7260",
    "products": [
        {
            "id": "01a0a76a-0520-7000-85bf-da90fb7f50fc",
            "sku": "PHN-MOT-996",
            "name": "Motorola Smartphone Plus 5G 256GB Silver",
            "priceAmount": 5351.99,
            "priceCurrency": "USD",
            "_links": {
                "self": {
                    "href": "https://localhost:7171/products/01a0a76a-0520-7000-85bf-da90fb7f50fc",
                    "method": "GET"
                },
                "update": {
                    "href": "https://localhost:7171/products/01a0a76a-0520-7000-85bf-da90fb7f50fc",
                    "method": "PATCH"
                }
            }
        }
        // ...
    ],
    "_links": {
        "self": {
            "href": "https://localhost:7171/products?cursor=01a0a76a-0520-7000-85c0-0af1b9866e5e&limit=10",
            "method": "GET"
        },
        "first": {
            "href": "https://localhost:7171/products?limit=10",
            "method": "GET"
        },
        "next": {
            "href": "https://localhost:7171/products?cursor=01a0a76a-0520-7000-85b6-dc988ded7260&limit=10",
            "method": "GET"
        },
        "create": {
            "href": "https://localhost:7171/products",
            "method": "POST"
        }
    }
}
```

### Get a product
`GET /products/01a0a76a-0520-7000-85bf-da90fb7f50fc`
```
Response:
{
    "id": "01a0a76a-0520-7000-85bf-da90fb7f50fc",
    "sku": "PHN-MOT-996",
    "name": "Motorola Smartphone Plus 5G 256GB Silver",
    "priceAmount": 5351.99,
    "priceCurrency": "USD",
    "_links": {
        "self": {
            "href": "https://localhost:7171/products/01a0a76a-0520-7000-85bf-da90fb7f50fc",
            "method": "GET"
        },
        "update": {
            "href": "https://localhost:7171/products/01a0a76a-0520-7000-85bf-da90fb7f50fc",
            "method": "PATCH"
        }
    }
}
```

### Create a product
`POST /products`
```
Request:
{
    "sku": "ABC-XYZ-001",
    "name": "New Product",
    "priceAmount": 99.99,
    "priceCurrency": "EUR"
}
```

```
Response Location Header:
https://localhost:7171/products/01a0a76e-2d8a-7000-85cf-0969c64a317b
```

### Update a product
`PATCH /products/01a0a76e-2d8a-7000-85cf-0969c64a317b`
```
Request:
{
    "name": "New Product (discounted!)",
    "priceAmount": 59.99,
    "priceCurrency": "EUR"
}
```

### Error response
`PATCH /products/01a0a76e-2d8a-7000-85cf-0969c64a317b`
```
Request:
{
    "name": "",
    "priceAmount": -59.99
}
```

```
Response:
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "name": [
            "Product name cannot be empty."
        ],
        "priceCurrency": [
            "PriceAmount and PriceCurrency must be provided together."
        ],
        "priceAmount": [
            "Product price must be positive."
        ]
    },
    "traceId": "00-52e0d8ab9bdb5d27efa09ed29154b776-f6a23ff5e7b3c23f-00"
}
```