# Fisher API Documentation

- [Fisher API Documentation](#fisher-api-documentation)
  - [Create Fisher](#create-fisher)
    - [Create Fisher Request](#create-fisher-request)
    - [Create Fisher Response](#create-fisher-response)


## Create Fisher

### Create Fisher Request
```yml
POST {{host}}/api/fishers
```

```json
{
    "FirstName": "Pepe",
    "SecondName": "Perez",
}
```

### Create Fisher Response
```yml
200 OK
Location: {{host}}/api/fishers/{{id}}
```

```json
{
    "id": "00000000-0000-0000-0000-000000000000",
    "FirstName": "Pepe",
    "SecondName": "Perez",
}
```
