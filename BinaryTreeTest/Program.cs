using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Runtime.InteropServices;

public unsafe class NativePool<T> : IDisposable where T : unmanaged {
    private readonly IntPtr _buffer;
    private readonly int _capacity;
    private readonly T* _items;
    private int _size;

    public NativePool(int maxCapacity) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCapacity);
        _capacity = maxCapacity;
        _buffer = (IntPtr)NativeMemory.Alloc((nuint)(maxCapacity * (uint)sizeof(T)));
        _items = (T*)_buffer.ToPointer();
        _size = 0;
    }

    public T* Allocate() {
        if (_size >= _capacity) throw new InvalidOperationException("Buffer full");
        return &_items[_size++];
    }

    public void Reset() => _size = 0;

    public void Dispose() {
        if (_buffer != IntPtr.Zero) {
            NativeMemory.Free(_buffer.ToPointer());
        }
    }
}

public unsafe struct Node {
    public Node* Left;
    public Node* Right;
}

public static class BinaryTrees {
    public unsafe static void Run(int maxDepth) {
        int minDepth = 4;
        int nodeCount = 1 << (maxDepth + 1); // Estimate max nodes
        using var pool = new NativePool<Node>(nodeCount);
        using var longPool = new NativePool<Node>(nodeCount);

        var stretchTree = longPool.CreateTree(maxDepth + 1);
        Console.WriteLine(string.Concat("stretch tree of depth ", maxDepth + 1,
            "\t check: ", CheckTree(stretchTree)));

        longPool.Reset();
        var longLivedTree = longPool.CreateTree(maxDepth);

        for (int depth = minDepth; depth <= maxDepth; depth += 2) {
            int iterations = 1 << (maxDepth - depth + minDepth);
            long checkSum = 0;

            for (int i = 1; i <= iterations; i++) {
                pool.Reset();
                var root = pool.CreateTree(depth);
                checkSum += CheckTree(root);
            }

            Console.WriteLine($"{iterations}\t trees of depth {depth}\t check: {checkSum}");
        }

        Console.WriteLine(string.Concat("long lived tree of depth ", maxDepth,
            "\t check: ", CheckTree(longLivedTree)));
    }

    private unsafe static Node* CreateTree(this NativePool<Node> pool, int depth) {
        if (depth <= 0) return null;
        Node* current = pool.Allocate();
        current->Left = pool.CreateTree(depth - 1);
        current->Right = pool.CreateTree(depth - 1);
        return current;
    }

    private unsafe static int CheckTree(Node* node) {
        if (node == null) return 1;
        return 1 + CheckTree(node->Left) + CheckTree(node->Right);
    }
}

public class NodeDefault {
    public NodeDefault? Left;
    public NodeDefault? Right;
}

public static class BinaryTreesDefault {
    public static void Run(int maxDepth) {
        int minDepth = 4;
        var stretchTree = CreateTree(maxDepth + 1);
        Console.WriteLine(string.Concat("stretch tree of depth ", maxDepth + 1,
            "\t check: ", CheckTree(stretchTree)));
        NodeDefault? longLivedTree = CreateTree(maxDepth);

        for (int depth = minDepth; depth <= maxDepth; depth += 2) {
            int iterations = 1 << (maxDepth - depth + minDepth);
            long checkSum = 0;
            for (int i = 1; i <= iterations; i++) {
                NodeDefault? root = CreateTree(depth);
                checkSum += CheckTree(root);
            }
            Console.WriteLine($"{iterations}\t trees of depth {depth}\t check: {checkSum}");
        }
        Console.WriteLine(string.Concat("long lived tree of depth ", maxDepth,
            "\t check: ", CheckTree(longLivedTree)));
    }

    private static NodeDefault? CreateTree(int depth) {
        if (depth <= 0) return null;
        NodeDefault node = new() {
            Left = CreateTree(depth - 1),
            Right = CreateTree(depth - 1)
        };
        return node;
    }

    private static int CheckTree(NodeDefault? node) {
        if (node == null) return 1;
        return 1 + CheckTree(node.Left) + CheckTree(node.Right);
    }
}

[MemoryDiagnoser]
public class BinaryTreesBenchmark {
    [Benchmark(Baseline = true)]
    public void Default() => BinaryTreesDefault.Run(21);

    [Benchmark]
    public void Optimized() => BinaryTrees.Run(21);
}

class Program {
    static void Main() => BenchmarkRunner.Run<BinaryTreesBenchmark>();
}