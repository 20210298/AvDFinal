using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace AvDFinal
{
    public class MakeGraph
    {
        private List<Button> locations;
        private Random random;

        public MakeGraph(List<Button> buttons)
        {
            locations = buttons;
            random = new Random();
        }

        // 거리 계산 ( x10 해야 m)
        private double CalculateDistance(Button start, Button end)
        {
            // Canvas에서의 정확한 좌표를 가져옴
            double x1 = Canvas.GetLeft(start) + start.ActualWidth / 2;
            double y1 = Canvas.GetTop(start) + start.ActualHeight / 2;
            double x2 = Canvas.GetLeft(end) + end.ActualWidth / 2;
            double y2 = Canvas.GetTop(end) + end.ActualHeight / 2;

            // 두 지점 간 거리 계산
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)) * 10; // 거리 * 10 (단위: 미터)
        }


        // 거리 기반 시간 계산 (분)
        private double CalculateTime(double distance)
        {
            double randomTime = random.NextDouble() * 2.0;
            return distance / 50.0 * 60.0 + randomTime; // 50 픽셀 ≈ 1km, 1km당 2분
        }

        // 거리 기반 비용 계산 
        private double CalculateCost(double distance)
        {
            double randomCost = random.Next(0, 501);
            return distance * 1000 + randomCost; // 1km당 1000원
        }

        // 그래프 생성 및 저장
        public void GenerateGraph(string filePath)
        {
            var edges = new List<string>();

            for (int i = 0; i < locations.Count; i++)
            {
                for (int j = i + 1; j < locations.Count; j++)
                {
                    var start = locations[i];
                    var end = locations[j];

                    // 거리, 시간, 비용 계산
                    double distance = CalculateDistance(start, end);
                    double time = CalculateTime(distance/1000);
                    double cost = CalculateCost(distance/1000);

                    // 그래프 정보 저장
                    string edge = $"\"{start.Content}\" \"{end.Content}\" 거리: {distance:F2} 소요시간: {time:F2} 비용: {cost:F2}";
                    edges.Add(edge);
                }
            }
            File.WriteAllLines(filePath, edges);
        }

        // 그래프 데이터 반환
        public Dictionary<string, List<(string Destination, double Distance, double Time, double Cost)>> GetGraphData()
        {
            var graph = new Dictionary<string, List<(string Destination, double Distance, double Time, double Cost)>>();

            for (int i = 0; i < locations.Count; i++)
            {
                for (int j = i + 1; j < locations.Count; j++)
                {
                    var start = locations[i];
                    var end = locations[j];

                    // 거리, 시간, 비용 계산
                    double distance = CalculateDistance(start, end);
                    double time = CalculateTime(distance / 1000);
                    double cost = CalculateCost(distance / 1000);

                    // 출발지 -> 목적지 추가
                    AddEdge(graph, start.Content.ToString(), end.Content.ToString(), distance, time, cost);

                    // 목적지 -> 출발지 추가 (양방향)
                    AddEdge(graph, end.Content.ToString(), start.Content.ToString(), distance, time, cost);
                }
            }

            return graph;
        }

        private void AddEdge(Dictionary<string, List<(string Destination, double Distance, double Time, double Cost)>> graph,
                             string from, string to, double distance, double time, double cost)
        {
            if (!graph.ContainsKey(from))
                graph[from] = new List<(string Destination, double Distance, double Time, double Cost)>();

            graph[from].Add((to, distance, time, cost));
        }
    }
}
