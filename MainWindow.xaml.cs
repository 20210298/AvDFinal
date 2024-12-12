using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AvDFinal
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, List<(string Destination, double Distance, double Time, double Cost)>> fullGraphData;
        private List<string> selectedLocations = new List<string>();    // 선택한 버튼 리스트

        public MainWindow()
        {
            InitializeComponent();

            // 버튼 리스트
            var buttons = new List<Button>
            {
                금오랜드, 금오산, 구미역, 낙동강체육공원, 유학산,
                동락공원, 에코랜드, 천생산, 구미과학관, 다온숲,
                새마을운동테마공원, 김천구미역, 김천과학관, 칠곡가산성, 칠곡양떼목장, 금오공대
            };

            // 전체 그래프 생성
            var makeGraph = new MakeGraph(buttons);
            fullGraphData = makeGraph.GetGraphData(); 
            makeGraph.GenerateGraph("graph.txt"); // 그래프 모델링 결과
        }      

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string locationName = button.Content.ToString();

                // 선택된 버튼 추가
                if (!selectedLocations.Contains(locationName))
                {
                    selectedLocations.Add(locationName);
                }

                // 선택된 버튼 리스트 출력
                DisplaySelectedLocations();

            }
        }

        private void DisplaySelectedLocations()
        {
            ResultStackPanel.Children.Clear();

            // 선택된 버튼 이름 출력
            foreach (var location in selectedLocations)
            {
                var textBlock = new TextBlock
                {
                    Text = location,
                    FontSize = 14,
                    Margin = new Thickness(5)
                };
                ResultStackPanel.Children.Add(textBlock);
            }
        }

        private void OnCalculateCycleClick(object sender, RoutedEventArgs e)
        {
            if (selectedLocations.Count < 2)
            {
                MessageBox.Show("최소 두 개의 장소를 선택해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string start = selectedLocations.First();
            string end = selectedLocations.Last();
            var waypoints = selectedLocations.Skip(1).Take(selectedLocations.Count - 2).ToList();

            var cycleSolver = new CycleSolver(fullGraphData);

            try
            {
                // 거리 기준 사이클 계산
                var (distanceCycle, totalDistance, totalTimeForDistance, totalCostForDistance) =
                    cycleSolver.CalculateCycleForDistance(start, waypoints, end);

                // 시간 기준 사이클 계산
                var (timeCycle, totalDistanceForTime, totalTimeForTime, totalCostForTime) =
                    cycleSolver.CalculateCycleForTime(start, waypoints, end);

                // 비용 기준 사이클 계산
                var (costCycle, totalDistanceForCost, totalTimeForCost, totalCostForCost) =
                    cycleSolver.CalculateCycleForCost(start, waypoints, end);

                // 결과 출력
                ResultStackPanel.Children.Clear();

                if (distanceCycle != null && distanceCycle.Count > 1)
                {
                    ResultStackPanel.Children.Add(CreateCycleResultTextBlock("거리 기준",
                        (distanceCycle, totalDistance, totalTimeForDistance, totalCostForDistance)));
                }

                if (timeCycle != null && timeCycle.Count > 1)
                {
                    ResultStackPanel.Children.Add(CreateCycleResultTextBlock("시간 기준",
                        (timeCycle, totalDistanceForTime, totalTimeForTime, totalCostForTime)));
                }

                if (costCycle != null && costCycle.Count > 1)
                {
                    ResultStackPanel.Children.Add(CreateCycleResultTextBlock("비용 기준",
                        (costCycle, totalDistanceForCost, totalTimeForCost, totalCostForCost)));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"사이클 계산 중 오류가 발생했습니다: {ex.Message}",
                                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TextBlock CreateCycleResultTextBlock(string title,
    (List<string> Path, double TotalDistance, double TotalTime, double TotalCost) cycleResult)
        {
            int hours = (int)(cycleResult.TotalTime / 60);
            int minutes = (int)(cycleResult.TotalTime % 60);
            string timeText = hours > 0 ? $"{hours}시간 {minutes}분" : $"{minutes}분";

            int thousnad = (int)(cycleResult.TotalCost / 1000);
            int under = (int)(cycleResult.TotalCost % 1000);
            string costText = thousnad > 0 ? $"{thousnad},{under:D3}원" : $"{under:D3}원";

            TextBlock textBlock = new TextBlock
            {
                FontSize = 14,  
                Margin = new Thickness(5),
                TextWrapping = TextWrapping.Wrap,
                Width = 400
            };

            textBlock.Inlines.Add(new Run
            {
                Text = $"{title}\n",
                FontSize = 18  
            });

            textBlock.Inlines.Add(new Run
            {
                Text = $"총 시간: {timeText}\n",
            });

            textBlock.Inlines.Add(new Run
            {
                Text = $"총 비용: {costText}, 총 거리: {cycleResult.TotalDistance / 1000:F2} km\n",
            });

            textBlock.Inlines.Add(new Run
            {
                Text = $"경로: {string.Join(" -> ", cycleResult.Path)}"
            });

            // TextBlock 클릭 이벤트 추가
            textBlock.MouseDown += (sender, e) =>
            {
                HighlightStartEndButtons(cycleResult.Path);
                DisplayPathOnCanvas(cycleResult.Path);                 
            };

            Canvas.SetZIndex(textBlock, 1);

            return textBlock;
        }

        
        // 다익스트라
        private void OnCalculateGlobalPathClick(object sender, RoutedEventArgs e)
        {
            if (fullGraphData == null || fullGraphData.Count == 0)
            {
                MessageBox.Show("그래프 데이터가 초기화되지 않았습니다.",
                                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string start;
            if (selectedLocations != null && selectedLocations.Count > 0)
            {
                start = selectedLocations.First(); // 선택된 첫 번째 위치를 시작 지점으로 설정
            }
            else
            {
                MessageBox.Show("출발지를 선택해주세요.",
                                "출발지 선택 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var timeResults = GetOptimalPathResults(start, edge => edge.Time);
                var distanceResults = GetOptimalPathResults(start, edge => edge.Distance);
                var costResults = GetOptimalPathResults(start, edge => edge.Cost);

                // 결과 출력
                ResultStackPanel.Children.Clear();

                ResultStackPanel.Children.Add(CreateDetailedResultTextBlock("시간 우선", timeResults));
                ResultStackPanel.Children.Add(CreateDetailedResultTextBlock("거리 우선", distanceResults));
                ResultStackPanel.Children.Add(CreateDetailedResultTextBlock("비용 우선", costResults));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"전체 최적 경로 계산 중 오류가 발생했습니다: {ex.Message}",
                                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private (double TotalMetric, double TotalDistance, double TotalTime, double TotalCost, List<string> Path)
    GetOptimalPathResults(string start, Func<(double Distance, double Time, double Cost), double> metricSelector)
        {
            double totalMetric = 0, totalDistance = 0, totalTime = 0, totalCost = 0;
            var path = new List<string> { start };
            var current = start;

            while (true)
            {
                // 다음 목적지 찾기
                var nextNode = fullGraphData[current]
                    .Where(edge => !path.Contains(edge.Destination)) // 방문하지 않은 노드만
                    .OrderBy(edge => metricSelector((edge.Distance, edge.Time, edge.Cost)))
                    .FirstOrDefault();

                if (nextNode.Destination == null) break; // 방문할 노드가 없으면 종료

                // 경로 업데이트
                path.Add(nextNode.Destination);
                totalMetric += metricSelector((nextNode.Distance, nextNode.Time, nextNode.Cost));
                totalDistance += nextNode.Distance;
                totalTime += nextNode.Time; // 총 시간 누적
                totalCost += nextNode.Cost;

                current = nextNode.Destination;
            }

            return (totalMetric, totalDistance, totalTime, totalCost, path);
        }



        private TextBlock CreateDetailedResultTextBlock(string title,
    (double TotalMetric, double TotalDistance, double TotalTime, double TotalCost, List<string> Path) result)
        {
            int hours = (int)(result.TotalTime / 60);
            int minutes = (int)(result.TotalTime % 60);
            string timeText = hours > 0 ? $"{hours}시간 {minutes}분" : $"{minutes}분";

            int thousnad = (int)(result.TotalCost / 1000);
            int under = (int)(result.TotalCost % 1000);
            string costText = thousnad > 0 ? $"{thousnad},{under:D3}원" : $"{under:D3}원";

            // TextBlock 생성 및 부분별 폰트 크기 설정
            TextBlock textBlock = new TextBlock
            {
                FontSize = 14,  // 기본 폰트 크기
                Margin = new Thickness(5),
                TextWrapping = TextWrapping.Wrap,
                Width = 400
            };

            // title 부분은 크게
            textBlock.Inlines.Add(new Run
            {
                Text = $"{title}\n",
                FontSize = 18  // title의 폰트 크기
            });

            // 나머지 내용은 기본 폰트 크기
            textBlock.Inlines.Add(new Run
            {
                Text = $"총 시간: {timeText}\n",
            });

            textBlock.Inlines.Add(new Run
            {
                Text = $"총 비용: {costText}, 총 거리: {result.TotalDistance / 1000:F2} km\n",
            });

            textBlock.Inlines.Add(new Run
            {
                Text = $"경로: {string.Join(" -> ", result.Path)}"
            });

            textBlock.MouseDown += (sender, e) =>
            {
                HighlightStartEndButtons(result.Path);
                DisplayPathOnCanvas(result.Path);
            };

            Canvas.SetZIndex(textBlock, 1);

            return textBlock;
        }
        
        // 경로 그리기
        private void DisplayPathOnCanvas(List<string> path)
        {
            // 초기화
            var linesToRemove = PathCanvas.Children.OfType<Line>().ToList();
            foreach (var line in linesToRemove)
            {
                PathCanvas.Children.Remove(line);
            }

            // 경로 그리기
            for (int i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];

                var fromButton = GetButtonFromLocation(from);
                var toButton = GetButtonFromLocation(to);

                if (fromButton == null || toButton == null)
                {
                    MessageBox.Show($"버튼 '{from}' 또는 '{to}'을(를) 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                double fromX = Canvas.GetLeft(fromButton) + fromButton.ActualWidth / 2;
                double fromY = Canvas.GetTop(fromButton) + fromButton.ActualHeight / 2;
                double toX = Canvas.GetLeft(toButton) + toButton.ActualWidth / 2;
                double toY = Canvas.GetTop(toButton) + toButton.ActualHeight / 2;

                // 경로
                Line line = new Line
                {
                    X1 = fromX,
                    Y1 = fromY,
                    X2 = toX,
                    Y2 = toY,
                    Stroke = Brushes.Green, 
                    StrokeThickness = 5,    
                };

                Canvas.SetZIndex(line, 0);

                PathCanvas.Children.Add(line);
            }
        }

        // 출발지 목적지 표시
        private void HighlightStartEndButtons(List<string> path)
        {
            foreach (var button in new List<Button>
            {
                금오랜드, 금오산, 구미역, 낙동강체육공원, 유학산,
                동락공원, 에코랜드, 천생산, 구미과학관, 다온숲,
                새마을운동테마공원, 김천구미역, 김천과학관, 칠곡가산성, 칠곡양떼목장, 금오공대
            })

            {
                button.Background = Brushes.White;
                button.BorderBrush = Brushes.Gray;
                button.Foreground = Brushes.Black;
            }

            if (path.Count > 0)
            {
                var startButton = GetButtonFromLocation(path.First());
                if (startButton != null)
                {
                    startButton.Background = Brushes.Green;
                    startButton.BorderBrush = Brushes.DarkGreen;
                    startButton.Foreground = Brushes.White;
                }

                var endButton = GetButtonFromLocation(path.Last());
                if (endButton != null)
                {
                    endButton.Background = Brushes.Red;
                    endButton.BorderBrush = Brushes.DarkRed;
                    endButton.Foreground = Brushes.White;
                }
            }
        }

        private Button GetButtonFromLocation(string location)
        {
            return this.FindName(location) as Button;
        }              
    }
}
