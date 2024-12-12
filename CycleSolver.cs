using AvDFinal;

public class CycleSolver
{
    private readonly Dictionary<string, List<(string Destination, double Distance, double Time, double Cost)>> _graphData;

    public CycleSolver(Dictionary<string, List<(string Destination, double Distance, double Time, double Cost)>> graphData)
    {
        _graphData = graphData;
    }

    // 시간 기준 사이클 계산
    public (List<string> Cycle, double TotalDistance, double TotalTime, double TotalCost) CalculateCycleForTime(
        string start, List<string> waypoints, string end)
    {
        return CalculateCycle(start, waypoints, end, edge => edge.Time, "시간");
    }

    // 거리 기준 사이클 계산
    public (List<string> Cycle, double TotalDistance, double TotalTime, double TotalCost) CalculateCycleForDistance(
        string start, List<string> waypoints, string end)
    {
        return CalculateCycle(start, waypoints, end, edge => edge.Distance, "거리");
    }

    // 비용 기준 사이클 계산
    public (List<string> Cycle, double TotalDistance, double TotalTime, double TotalCost) CalculateCycleForCost(
        string start, List<string> waypoints, string end)
    {
        return CalculateCycle(start, waypoints, end, edge => edge.Cost, "비용");
    }

    // 공통 로직: 지정된 기준으로 사이클 계산
    private (List<string> Cycle, double TotalDistance, double TotalTime, double TotalCost) CalculateCycle(
        string start,
        List<string> waypoints,
        string end,
        Func<(double Distance, double Time, double Cost), double> metricSelector,
        string metricType)
    {
        var permutations = GetPermutations(waypoints);
        List<string> bestCycle = null;

        double bestMetric = double.MaxValue;
        double bestDistance = 0;
        double bestTime = 0;
        double bestCost = 0;

        foreach (var perm in permutations)
        {
            var currentCycle = new List<string> { start };
            currentCycle.AddRange(perm);
            currentCycle.Add(end);

            double currentMetric = 0;
            double currentDistance = 0;
            double currentTime = 0;
            double currentCost = 0;
            bool validCycle = true;

            for (int i = 0; i < currentCycle.Count - 1; i++)
            {
                var current = currentCycle[i];
                var next = currentCycle[i + 1];

                // 기준으로 최적 경로 계산
                var (segmentPath, segmentMetric) = Dijkstra.FindShortestPath(
                    current,
                    next,
                    _graphData,
                    metricSelector);

                if (segmentMetric == double.MaxValue)
                {
                    validCycle = false;
                    break;
                }

                // 최적 경로를 기반으로 거리, 시간, 비용 추가 계산
                foreach (var node in segmentPath.Zip(segmentPath.Skip(1)))
                {
                    var from = node.First;
                    var to = node.Second;

                    var edge = _graphData[from].FirstOrDefault(e => e.Destination == to);
                    if (edge.Destination == null)
                    {
                        validCycle = false;
                        break;
                    }

                    currentDistance += edge.Distance;
                    currentTime += edge.Time;
                    currentCost += edge.Cost;
                }

                currentMetric += segmentMetric;

                if (!validCycle)
                    break;
            }

            if (validCycle && currentMetric < bestMetric)
            {
                bestMetric = currentMetric;
                bestCycle = currentCycle;
                bestDistance = currentDistance;
                bestTime = currentTime;
                bestCost = currentCost;
            }
        }

        return (bestCycle, bestDistance, bestTime, bestCost);
    }

    // 경유지 순열 생성
    private List<List<string>> GetPermutations(List<string> waypoints)
    {
        if (waypoints.Count == 1)
            return new List<List<string>> { waypoints };

        var permutations = new List<List<string>>();
        foreach (var item in waypoints)
        {
            var remaining = waypoints.Where(x => x != item).ToList();
            foreach (var perm in GetPermutations(remaining))
            {
                perm.Insert(0, item);
                permutations.Add(perm);
            }
        }
        return permutations;
    }
}
