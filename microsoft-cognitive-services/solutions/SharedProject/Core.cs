using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedProject
{
    public class Core
    {
        private static async Task<Emotion[]> GetHappiness(Stream stream)
        {
            string emotionKey = "36ca44350a4747dfbfbae0bef30d89cb";

            EmotionServiceClient emotionClient = new EmotionServiceClient(emotionKey);

            var emotionResults = await emotionClient.RecognizeAsync(stream);

            if (emotionResults == null || emotionResults.Count() == 0)
            {
                throw new Exception("Can't detect face");
            }

            return emotionResults;
        }

        //Average happiness calculation in case of multiple people
        public static async Task<float> GetAverageHappinessScore(Stream stream)
        {
            Emotion[] emotionResults = await GetHappiness(stream);

            float score = 0;
            foreach (var emotionResult in emotionResults)
            {
                score = score + emotionResult.Scores.Happiness;
            }

            return score / emotionResults.Count();
        }

        public static async Task<string> GetVerboseEmotionString(Stream stream)
        {
            Emotion[] emotionResults = await GetHappiness(stream);
            var scores = new Dictionary<string, float>();
            var cnt = 0;
            foreach (var emotion in emotionResults)
            {
                foreach (var property in typeof(Scores).GetProperties())
                {
                    ++cnt;
                    var value = property.GetValue(emotion.Scores);
                    if (scores.ContainsKey(property.Name))
                    {
                        scores[property.Name] += (float)value;
                        continue;
                    }
                    scores.Add(property.Name, (float)value);
                }
            }
            var sb = new StringBuilder();
            foreach (var scoreKvp in scores.ToList())
            {
                var niceScore = Math.Round((scoreKvp.Value / cnt)*100,2);
                sb.AppendLine($"{scoreKvp.Key}: {niceScore}");
            }
            return sb.ToString();
        }

        public static string GetHappinessMessage(float score)
        {
            score = score * 100;
            double result = Math.Round(score, 2);

            if (score >= 50)
                return result + " % :-)";
            else
                return result + "% :-(";
        }
    }
}