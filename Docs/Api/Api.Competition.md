# Competition API Documentation

- [Competition API Documentation](#competition-api-documentation)
  - [Create Competition](#create-competition)
    - [Create Competition Request](#create-competition-request)
    - [Create Competition Response](#create-competition-response)
  - [Add Fisher Score](#add-fisher-score)
    - [Add Fisher Score Request](#add-fisher-score-request)
    - [Add Fisher Score Response](#add-fisher-score-response)
  - [Get Competition Leader Board](#get-competition-leader-board)
    - [Get Competition Leader Board Request](#get-competition-leader-board-request)
    - [Get Competition Leader Board Response](#get-competition-leader-board-response)

## Create Competition

### Create Competition Request
```yml
POST {{host}}/api/tournaments/{{tournamentId}}/competitions
```

```json
{
    "Competitions": [
        {
            "startDateTime": "2019-01-01T12:00:00Z",
            "location": {
                "Country": "Spain",
                "State": "Catalonia",
                "City": "Barcelona",
                "Place": "Port Vell"
            }
        }
    ]
}
```

### Create Competition Response
```yml
200 OK
Location: {{host}}/api/tournaments/{{tournamentId}}/competitions/{{id}}
```

```json
[
    {
        "id": "00000000-0000-0000-0000-000000000000",
        "tournamentId": "00000000-0000-0000-0000-000000000000",
        "startDateTime": "2019-01-01T12:00:00Z",
        "endDateTime": null,
        "location": {
            "Country": "Spain",
            "State": "Catalonia",
            "City": "Barcelona",
            "Place": "Port Vell"
        },
        "createdDateTime": "2019-01-01T12:00:00Z"
    }
]
```

## Add Fisher Score
### Add Fisher Score Request
```yml
POST {{host}}/api/tournaments/{{tournamentId}}/competitions/{{competitionId}}/scores
```

```json
{
    "tournamentId": "00000000-0000-0000-0000-000000000000",
    "competitionId": "00000000-0000-0000-0000-000000000000",
    "fisherId": "00000000-0000-0000-0000-000000000000",
    "score": 10
}
```

### Add Fisher Score Response
```yml
200 OK
```

```json
{
    "id": "00000000-0000-0000-0000-000000000000",
    "tournamentId": "00000000-0000-0000-0000-000000000000",
    "competitionId": "00000000-0000-0000-0000-000000000000",
    "fisherId": "00000000-0000-0000-0000-000000000000",
    "score": 10,
    "createdDateTime": "2019-01-01T12:00:00Z"
}
```

## Get Competition Leader Board
### Get Competition Leader Board Request
```yml
GET {{host}}/api/tournaments/{{tournamentId}}/competitions/{{competitionId}}/leaderboard
```

### Get Competition Leader Board Response
```yml
200 OK
```

```json
{
    "A": [
        {
            "fisherId": "00000000-0000-0000-0000-000000000000",
            "FirstName": "John",
            "LastName": "Doe",
            "TotalScore": 10
        },
        {
            "fisherId": "00000000-0000-0000-0000-000000000001",
            "FirstName": "Jane",
            "LastName": "Doe",
            "TotalScore": 5
        }
    ],
    "B": [
        {
            "fisherId": "00000000-0000-0000-0000-000000000002",
            "FirstName": "John",
            "LastName": "Doe",
            "TotalScore": 20
        },
        {
            "fisherId": "00000000-0000-0000-0000-000000000003",
            "FirstName": "Jane",
            "LastName": "Doe",
            "TotalScore": 15
        }
    ]
}
```