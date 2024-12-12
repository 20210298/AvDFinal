using System;
using System.Collections.Generic;
using System.Linq;

namespace AvDFinal
{
    public class Dijkstra
    {
        public static (List<string> Path, double Metric) FindShortestPath(
            string start,
            string end,
            Dictionary<string, List<(string Destination, double Distance, double Time, double Cost)>> graph,
            Func<(double Distance, double Time, double Cost), double> metricSelector)
        {
            var distances = new Dictionary<string, double>();
            var previous = new Dictionary<string, string>(); 
            var unvisited = new PriorityQueue<string, double>(); 

            foreach (var node in graph.Keys)
            {
                distances[node] = double.MaxValue;
                previous[node] = null;
                unvisited.Enqueue(node, double.MaxValue);
            }

            distances[start] = 0;
            unvisited.Enqueue(start, 0); 

            // 다익스트라
            while (unvisited.Count > 0)
            {
                // 가장 가까운 노드 선택
                var current = unvisited.Dequeue(); 

                // 도착점이면 종료
                if (current == end)
                    break;

                // 인접 노드 업데이트
                if (graph.ContainsKey(current))
                {
                    foreach (var (neighbor, distance, time, cost) in graph[current])
                    {
                        double alt = distances[current] + metricSelector((distance, time, cost));
                        if (alt < distances[neighbor])
                        {
                            distances[neighbor] = alt;
                            previous[neighbor] = current; 
                            unvisited.Enqueue(neighbor, alt); 
                        }
                    }
                }
            }

            // 경로 생성
            var path = GeneratePath(previous, start, end);

            if (distances[end] == double.MaxValue || path.Count <= 1)
            {
                return (new List<string> { "경로 없음" }, double.MaxValue);
            }

            return (path, distances[end]);
        }

        private static List<string> GeneratePath(Dictionary<string, string> previous, string start, string end)
        {
            var path = new List<string>();
            var step = end;

            // 이전 노드를 추적
            while (step != null)
            {
                path.Insert(0, step);
                step = previous[step];
            }

            if (path.First() != start)
                return new List<string>();

            return path;
        }
    }
}
