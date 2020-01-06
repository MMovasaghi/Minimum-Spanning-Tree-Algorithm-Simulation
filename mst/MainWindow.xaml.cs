using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mst
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int size = 10;
        private Point[] Positions = new Point[size];
        private Single[,] Network = new Single[size, size];
        private Random R = new Random();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Generator_btn(object sender, RoutedEventArgs e)
        {
            canvas1.Width = 600;
            canvas1.Height = 600;
            setnet(Network, Positions);
            shownet(Network);
        }
        private void PrimAlgorithm_btn(object sender, RoutedEventArgs e)
        {
            prims();
        }
        private void KruskalAlgorithm_btn(object sender, RoutedEventArgs e)
        {
            kruskal();
        }
        private void setnet(Single[,] Net, Point[] Pos)
        {
            int maxlength = (int)(Math.Min(canvas1.Width,canvas1.Height) * 0.9);
            int minlength = maxlength / size;
            for (int i = 0; i < size; i++)
            {
                Pos[i].X = R.Next(minlength, maxlength);
                Pos[i].Y = R.Next(minlength, maxlength);
                for (int j = 0; j <= i; j++)
                {
                    Net[i, j] = distance(Pos[i], Pos[j]);
                    Net[j, i] = Net[i, j];
                    if (i == j) Net[i, j] = 0;
                }
            }
        }
        private Single distance(Point a, Point b)
        {
            return (Single)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        private void shownet(Single[,] Net)
        {
            canvas1.Children.Clear();
            Line myLine;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (Net[i, j] != 0)
                    {
                        myLine = new Line();
                        myLine.Stroke = Brushes.Black;
                        myLine.X1 = Positions[i].X;
                        myLine.X2 = Positions[j].X;
                        myLine.Y1 = Positions[i].Y;
                        myLine.Y2 = Positions[j].Y;
                        myLine.StrokeThickness = 1;
                        
                        canvas1.Children.Add(myLine);
                    }
                }
            }
            Rectangle myMarker;
            for (int i = 0; i < size; i++)
            {
                myMarker = new Rectangle();
                myMarker.Stroke = Brushes.Black;
                myMarker.Fill = Brushes.Red;
                myMarker.Height = 10;
                myMarker.Width = 10;
                myMarker.SetValue(Canvas.TopProperty,
                    Positions[i].Y - myMarker.Height / 2);
                myMarker.SetValue(Canvas.LeftProperty,
                    Positions[i].X - myMarker.Width / 2);
                canvas1.Children.Add(myMarker);
            }
        }

        private void prims()
        {
            int[] included = new int[size];
            int[] excluded = new int[size];
            Single[,] finished = new Single[size, size];
            int start = 0;
            int finish = 0;
            for (int i = 0; i < size; i++)
            {
                excluded[i] = i;
                included[i] = -1;
            }
            included[0] = excluded[R.Next(size)];
            excluded[included[0]] = -1;
            for (int n = 1; n < size; n++)
            {
                closest(n, ref start, ref finish, included, excluded);
                included[n] = excluded[finish];
                excluded[finish] = -1;
                finished[included[n], included[start]] = Network[included[n], included[start]];
                finished[included[start], included[n]] = Network[included[start], included[n]];
            }
            shownet(finished);
        }
        private void closest(int n, ref int start, ref int finish, int[] included, int[] excluded)
        {
            Single smallest = -1;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (excluded[j] == -1) 
                        continue;

                    if (smallest == -1) 
                        smallest = Network[included[i], excluded[j]];

                    if (Network[included[i], excluded[j]] > smallest) 
                        continue;

                    smallest = Network[included[i], excluded[j]];
                    start = i;
                    finish = j;
                }
            }
        }        
        private void kruskal()
        {
            int[] edges = new int[(size * (size-1))/2];
            int number = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = i+1; j < size; j++)
                {
                    edges[number] = (int)Network[i, j];
                    number++;
                }
            }
            Graph graph = new Graph(size, (size * (size - 1)) / 2 , edges);
            shownet(graph.KruskalMST());
        }
    }
    public class Graph
    {
        // A class to represent a graph edge  
        public class Edge : IComparable<Edge>
        {
            public int src, dest, weight;

            // Comparator function used for sorting edges  
            // based on their weight  
            public int CompareTo(Edge compareEdge)
            {
                return this.weight - compareEdge.weight;
            }
        }

        // A class to represent a subset for union-find  
        public class subset
        {
            public int parent, rank;
        };

        int V, E; // V-> no. of vertices & E->no.of edges  
        Edge[] edge; // collection of all edges  

        // Creates a graph with V vertices and E edges  
        public Graph(int v, int e, int[] edeges)
        {
            V = v;
            E = e;
            edge = new Edge[E];
            for (int i = 0 , k = 0; i < V && k < E; i++)
            {
                for (int j = i+1; j < V && k < E; j++ , k++)
                {
                    edge[k] = new Edge()
                    {
                        src = i,
                        dest = j,                        
                        weight = edeges[k],
                    };
                }
            }

        }

        // A utility function to find set of an element i  
        // (uses path compression technique)  
        int find(subset[] subsets, int i)
        {
            // find root and make root as  
            // parent of i (path compression)  
            if (subsets[i].parent != i)
                subsets[i].parent = find(subsets, subsets[i].parent);

            return subsets[i].parent;
        }

        // A function that does union of  
        // two sets of x and y (uses union by rank)  
        void Union(subset[] subsets, int x, int y)
        {
            int xroot = find(subsets, x);
            int yroot = find(subsets, y);

            // Attach smaller rank tree under root of 
            // high rank tree (Union by Rank)  
            if (subsets[xroot].rank < subsets[yroot].rank)
                subsets[xroot].parent = yroot;
            else if (subsets[xroot].rank > subsets[yroot].rank)
                subsets[yroot].parent = xroot;

            // If ranks are same, then make one as root  
            // and increment its rank by one  
            else
            {
                subsets[yroot].parent = xroot;
                subsets[xroot].rank++;
            }
        }

        // The main function to construct MST  
        // using Kruskal's algorithm  
        public Single[,] KruskalMST()
        {
            Edge[] result = new Edge[V]; // This will store the resultant MST  
            int e = 0; // An index variable, used for result[]  
            int i = 0; // An index variable, used for sorted edges  
            for (i = 0; i < V; ++i)
                result[i] = new Edge();

            // Step 1: Sort all the edges in non-decreasing  
            // order of their weight. If we are not allowed  
            // to change the given graph, we can create 
            // a copy of array of edges  
            Array.Sort(edge);

            // Allocate memory for creating V ssubsets  
            subset[] subsets = new subset[V];
            for (i = 0; i < V; ++i)
                subsets[i] = new subset();

            // Create V subsets with single elements  
            for (int v = 0; v < V; ++v)
            {
                subsets[v].parent = v;
                subsets[v].rank = 0;
            }

            i = 0; // Index used to pick next edge  

            // Number of edges to be taken is equal to V-1  
            while (e < V - 1)
            {
                // Step 2: Pick the smallest edge. And increment  
                // the index for next iteration  
                Edge next_edge = new Edge();
                next_edge = edge[i++];

                int x = find(subsets, next_edge.src);
                int y = find(subsets, next_edge.dest);

                // If including this edge does't cause cycle,  
                // include it in result and increment the index  
                // of result for next edge  
                if (x != y)
                {
                    result[e++] = next_edge;
                    Union(subsets, x, y);
                }
                // Else discard the next_edge  
            }
            Single[,] finished = new Single[10, 10];

            // print the contents of result[] to display  
            // the built MST  
            for (i = 0; i < e; ++i)
            {
                finished[result[i].src, result[i].dest] = result[i].weight;
                finished[result[i].dest, result[i].src] = result[i].weight;
            }
            return finished;
        }
    }
}
