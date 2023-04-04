import http from "k6/http";
import { sleep, randomString } from "k6";

export const options = {
  scenarios: {
    stress: {
      executor: "ramping-arrival-rate",
      preAllocatedVUs: 500,
      timeUnit: "1s",
      stages: [
        { duration: "2m", target: 2 }, // below normal load
        // 10 x 1s = 600 r/min
        { duration: "2m", target: 10 }, // below normal load
        // { duration: "5m", target: 10 },
        // 20 x 1s = 1200 r/min
        { duration: "1m", target: 20 },
        // 30 x 1s = 1800 r/min
        { duration: "20s", target: 30 }, // around the breaking point
        { duration: "2m", target: 40 }, // beyond the breaking point
        { duration: "5m", target: 40 },
        { duration: "10m", target: 0 }, // scale down. Recovery stage.
      ],
    },
  },
};

export default function () {
  const BASE_URL = "http://localhost:5244"; // make sure this is not production
  const tournamentId = "B84D7572-D177-40AD-843D-1CF8EF662B51";

  const headers = {
    params: {
      headers: { 'Content-Type': 'application/json; charset=utf-8' },
    },
  }

  const responses = http.batch([
    // ["GET", `${BASE_URL}/tournaments/${tournamentId}/competitions/49BE8785-675A-4368-BC3C-5BF6E97C26BD/leaderboard`],
    // ["GET", `${BASE_URL}/tournaments/${tournamentId}/leaderboard`],
    // {
    //   method: "POST",
    //   url: `${BASE_URL}/tournaments/${tournamentId}/categories`,
    //   body: JSON.stringify({ "Name": `Ct ${Math.random()}${Math.random()}` }),
    //   params: {
    //     headers: { 'Content-Type': 'application/json; charset=utf-8' },
    //   }
    // },
    {
      method: "POST",
      url: `${BASE_URL}/fishers`,
      body: JSON.stringify({ "FirstName": "stretest", "LastName": "test" }),
      params: {
        headers: { 'Content-Type': 'application/json; charset=utf-8' },
      }
    },
  ]);

  // console.log(responses);
}
