import http from "k6/http";
import { check, fail, sleep, randomString } from "k6";

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
  const competitionIDs = [
    "49BE8785-675A-4368-BC3C-5BF6E97C26BD",
    "64B2EF5A-0B1B-4386-A722-C93C16B93F8B",
    "0CBDB8F2-38BE-4D6F-8E95-DAEA2D037528",
    "54A0C8B7-641B-4195-AB1C-8EC7659DAE85",
    "6E40B551-AF66-4777-9114-C9244C97CF48",
    "9475B34F-7C56-4B61-993A-68B685457B7C",
    "99445B43-D408-48A8-B943-9D5E12678439"
  ]

  const fishersIds = [
    "7448D5D7-BDFE-48F8-A550-F7BA7C8F6CEC",
    "314559DF-142F-415C-B70D-162F9B2C8E90",
    "D5E5BDDB-F5AB-446B-A25E-B2789352570F",
    "A12807D4-584B-4430-A958-D0857C7EE306",
    "9FD8C5A9-6900-4CE2-974F-90AEBFE68221"
  ]

  const headers = {
    params: {
      headers: { 'Content-Type': 'application/json; charset=utf-8' },
    },
  }

  const maxCompId = competitionIDs.length - 1;

  const requests = {
    'add score': {
      method: "POST",
      url: `${BASE_URL}/tournaments/${tournamentId}/competitions/${competitionIDs[randomIntFromInterval(0, maxCompId)]}/scores`,
      body: JSON.stringify({ "FisherId": fishersIds[randomIntFromInterval(0, fishersIds.length - 1)], "Score": 2 }),
      params: {
        headers: { 'Content-Type': 'application/json; charset=utf-8' },
      }
    },

    'get competition leaderboard': {
      method: "GET",
      url: `${BASE_URL}/tournaments/${tournamentId}/competitions/${competitionIDs[randomIntFromInterval(0, maxCompId)]}/leaderboard`,
    }

    // ["GET", `${BASE_URL}/tournaments/${tournamentId}/leaderboard`],

    // {
    //   method: "POST",
    //   url: `${BASE_URL}/tournaments/${tournamentId}/categories`,
    //   body: JSON.stringify({ "Name": `Ct ${Math.random()}${Math.random()}` }),
    //   params: {
    //     headers: { 'Content-Type': 'application/json; charset=utf-8' },
    //   }
    // },

    // 'add fisher': {
    //   method: "POST",
    //   url: `${BASE_URL}/fishers`,
    //   body: JSON.stringify({ "FirstName": "stress test", "LastName": "test" }),
    //   params: {
    //     headers: { 'Content-Type': 'application/json; charset=utf-8' },
    //   }
    // },
  }

  const responses = http.batch(requests);

  if (!check(responses['add score'], { 'status 200': (r) => r.status === 200 })) {
    fail('failed to add score');
  }
}

function randomIntFromInterval(min, max) { // min and max included 
  return Math.floor(Math.random() * (max - min + 1) + min)
}