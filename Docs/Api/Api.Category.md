# Category API Documentation

## Create Category
### Create Category Request
```yml
POST {{host}}/api/tournaments/{{tournamentID}}/categories
```

```json
{
    "name": "Primary"
}
```

### Create Category Response

```yml
200 OK
```

```json
{
    "id": 0,
    "name": "Primary",
    "createdDateTime": "2019-01-01T12:00:00Z"
}
```

## Update Category
### Update Category Request
```yml
PUT {{host}}/api/tournaments/{{tournamentID}}/categories/{{categoryID}}
```

```json
{
    "name": "Primary A"
}
```

## Update Category Response

```yml
200 OK
```

```json
{
    "id": 0,
    "name": "Primary A",
    "createdDateTime": "2019-01-01T12:00:00Z"
}
```