using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AzureAppGuessGame
{
    public static class Game
    {
        [FunctionName("Question")]
        public static async Task<string> RunGetAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("Getting question");
            var numbers = GetPseudoRandomNumbers();
            return $"{numbers.Item1}*{numbers.Item2}";
        }


        [FunctionName("Answer")]
        public static async Task<IActionResult> RunPostAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("Posting answer");
            var answer = await GetAnswerFromRequest(req.Body);

            if (!int.TryParse(answer, out int answeredNumber))
            {
                log.LogInformation("Wrong format: " + answer);
                return new BadRequestObjectResult("The request body is wrongly formatted.");
            }


            if (IsAnswerCorrect(answeredNumber))
            {
                log.LogInformation("We have a winner!");
                return new OkObjectResult("Congratulations! The code is: YSOFT ;)");
            }

            log.LogInformation("Too late or wrong answer: " + answeredNumber);
            return new StatusCodeResult(418);
        }

        private static async Task<string> GetAnswerFromRequest(Stream body)
        {
            using var streamReader = new StreamReader(body);
            return (await streamReader.ReadToEndAsync()).Trim();
        }

        private static bool IsAnswerCorrect(int answer)
        {
            var numbers = GetPseudoRandomNumbers();
            return answer == numbers.Item1 * numbers.Item2;
        }

        private static (int, int) GetPseudoRandomNumbers()
        {
            var now = DateTime.Now;
            var firstNumber = now.Second * 13;
            var secondNumber = (now.Minute * now.Second * 13 * now.Hour) % 111;
            return (firstNumber, secondNumber);
        }
    }
}