# Tournament API Documentation

- [Tournament API Documentation](#tournament-api-documentation)
  - [Create Tournament](#create-tournament)
    - [Create Tournament Request](#create-tournament-request)
    - [Create Tournament Response](#create-tournament-response)
  - [Add inscription](#add-inscription)
    - [Add inscription Request](#add-inscription-request)
    - [Add inscription Response](#add-inscription-response)
  - [Get tournament leader board](#get-tournament-leader-board)
    - [Get tournament leader board Request](#get-tournament-leader-board-request)
    - [Get tournament leader board Response](#get-tournament-leader-board-response)

## Create Tournament
### Create Tournament Request
```yml
POST {{host}}/api/tournaments
```

```json
{
    "name": "New Year Tournament",
    "startDate": "2019-01-01",
    "endDate": "2019-02-01"
}
```

### Create Tournament Response
```yml
200 OK
Location: {{host}}/api/tournaments/{{id}}
```

```json
{
    "id": "00000000-0000-0000-0000-000000000000",
    "name": "New Year Tournament",
    "startDate": "2019-01-01",
    "endDate": "2019-02-01",
    "competitionIds": [],
    "inscriptionIds": [],
    "createdDateTime": "2019-01-01T12:00:00"
}
```

## Add inscription

### Add inscription Request
```yml
POST {{host}}/api/tournaments/{{tournamentId}}/inscriptions
```

```json
{
    "fisherId": "00000000-0000-0000-0000-000000000000"
}
```

### Add inscription Response

```yml
200 OK
```

```json
{
    "id": "00000000-0000-0000-0000-000000000000",
    "tournamentId": "00000000-0000-0000-0000-000000000000",
    "fisherId": "00000000-0000-0000-0000-000000000000",
    "createdDateTime": "2019-01-01T12:00:00"
}
```

## Get tournament leader board
### Get tournament leader board Request
```yml
GET {{host}}/api/tournaments/{{tournamentId}}/leaderboard
```

### Get tournament leader board Response
```yml
200 OK
```

```json
{
    "A": [
        {
            "fisherId": "00000000-0000-0000-0000-000000000000",
            "FirstName": "John",
            "LastName": "Doe"
        },
        {
            "fisherId": "00000000-0000-0000-0000-000000000001",
            "FirstName": "Jane",
            "LastName": "Doe"
        }
    ],
    "B": [
        {
            "fisherId": "00000000-0000-0000-0000-000000000002",
            "FirstName": "John",
            "LastName": "Doe",
        },
        {
            "fisherId": "00000000-0000-0000-0000-000000000003",
            "FirstName": "Jane",
            "LastName": "Doe",
        }
    ]
}
```

You can derive the fisher position in a category based on their array index, index 0 would be position 1°.