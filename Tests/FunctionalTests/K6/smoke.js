import http from "k6/http";
import { check, sleep, fail } from "k6";

export const options = {
  vus: 5,
  duration: "1m",
};

export default function () {
  const BASE_URL = "http://localhost:5244"; // make sure this is not production
  const tournamentId = "B84D7572-D177-40AD-843D-1CF8EF662B51";
  const competitionID = "49BE8785-675A-4368-BC3C-5BF6E97C26BD";

  const fishersIds = [
    "7448D5D7-BDFE-48F8-A550-F7BA7C8F6CEC",
    "314559DF-142F-415C-B70D-162F9B2C8E90",
    "D5E5BDDB-F5AB-446B-A25E-B2789352570F"
  ]


  const requests = {
    'add score': {
      method: "POST",
      url: `${BASE_URL}/tournaments/${tournamentId}/competitions/${competitionID}/scores`,
      body: JSON.stringify({ "FisherId": fishersIds[randomIntFromInterval(0, 2)], "Score": 2 }),
      params: {
        headers: { 'Content-Type': 'application/json; charset=utf-8' },
      }
    }
  }

  const responses = http.batch(requests/*[
    // ["GET", `${BASE_URL}/tournaments/${tournamentId}/competitions/${competitionID}/leaderboard`],
    // ["GET", `${BASE_URL}/tournaments/${tournamentId}/leaderboard`],
  ]*/);

  // console.log(responses);

  if (!check(responses['add score'], { 'status 200': (r) => r.status === 200 })) {
    fail('failed to add score');
  }

  sleep(1);
}


function randomIntFromInterval(min, max) { // min and max included 
  return Math.floor(Math.random() * (max - min + 1) + min)
}

