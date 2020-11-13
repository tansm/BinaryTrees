// The Computer Language Benchmarks Game
// https://salsa.debian.org/benchmarksgame-team/benchmarksgame/
//
// contributed by Marek Safar
// concurrency added by Peperud
// fixed long-lived tree by Anthony Lloyd
// ported from F# version by Anthony Lloyd

using System;
using System.Threading.Tasks;

class BinaryTreesV2
{
    struct TreeNode
    {
        class Next { public TreeNode left, right; }
        readonly Next next;

        TreeNode(TreeNode left, TreeNode right) =>
            next = new Next { left = left, right = right };

        internal static TreeNode Create(int d)
        {
            return d == 1 ? new TreeNode(new TreeNode(), new TreeNode())
                          : new TreeNode(Create(d - 1), Create(d - 1));
        }

        internal int Check()
        {
            int c = 1;
            var current = next;
            while (current != null)
            {
                c += current.right.Check() + 1;
                current = current.left.next;
            }
            return c;
        }
    }

    const int MinDepth = 4;
    const int NoTasks = 4;

    static int Check(int n, int depth)
    {
        var check = 0;
        for (int i = n; i > 0; i--)
            check += TreeNode.Create(depth).Check();
        return check;
    }

    public static void Run(string[] args)
    {
        int maxDepth = args.Length == 0 ? 10
            : Math.Max(MinDepth + 2, int.Parse(args[0]));

        var stretchTreeCheck = Task.Run(() =>
        {
            int stretchDepth = maxDepth + 1;
            return "stretch tree of depth " + stretchDepth + "\t check: " +
                        TreeNode.Create(stretchDepth).Check();
        });

        var longLivedTree = TreeNode.Create(maxDepth);
        var longLivedText = Task.Run(() =>
        {
            return "long lived tree of depth " + maxDepth +
                        "\t check: " + longLivedTree.Check();
        });

        var results = new string[(maxDepth - MinDepth) / 2 + 1];
        var tasks = new Task<string>[results.Length];

        for (int i = 0; i < results.Length; i++)
        {
            int depth = i * 2 + MinDepth;
            tasks[i] = Task.Run(() =>
            {
                int n = (1 << maxDepth - depth + MinDepth) / NoTasks;
                var result = new int[NoTasks];
                for (int t = 0; t < result.Length; t++)
                {
                    result[t] = Check(n, depth);
                }
                var check = result[0];
                for (int t = 1; t < result.Length; t++)
                    check += result[t];
                return (n * NoTasks) + "\t trees of depth " + depth +
                                "\t check: " + check;
            });
        }
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = tasks[i].Result;
        }

        Console.WriteLine(stretchTreeCheck.Result);

        for (int i = 0; i < results.Length; i++)
            Console.WriteLine(results[i]);

        Console.WriteLine(longLivedText.Result);
    }
}
