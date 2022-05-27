using System.Collections.Generic;


namespace Nez.AI.Pathfinding
{
    /// <summary>
    /// calculates paths given an IUnweightedGraph and start/goal positions
    /// </summary>
    public static class BreadthFirstPathfinder
    {
        public static bool Search<T>(IUnweightedGraph<T> graph, T start, T goal, out Dictionary<T, T> cameFrom)
        {
            bool foundPath = false;
            var frontier = new Queue<T>();
            frontier.Enqueue(start);

            cameFrom = new Dictionary<T, T>
            {
                { start, start }
            };

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                if (current.Equals(goal))
                {
                    foundPath = true;
                    break;
                }

                foreach (var next in graph.GetNeighbors(current))
                {
                    if (!cameFrom.ContainsKey(next))
                    {
                        frontier.Enqueue(next);
                        cameFrom.Add(next, current);
                    }
                }
            }

            return foundPath;
        }


        public static List<T> Search<T>(IUnweightedGraph<T> graph, T start, T goal)
        {
            bool foundPath = Search(graph, start, goal, out var cameFrom);
            return foundPath ? AStarPathfinder.RecontructPath(cameFrom, start, goal) : null;
        }
    }
}