using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Runtime.InteropServices;

public unsafe class NativePool<T> : IDisposable where T : unmanaged {
    private readonly IntPtr _buffer;
    private readonly int _itemCount;
    private readonly T* _items;
    private int _index;

    public NativePool(int itemCount) {
        if (itemCount <= 0) throw new ArgumentOutOfRangeException(nameof(itemCount));
        _itemCount = itemCount;
        _buffer = (IntPtr)NativeMemory.Alloc((nuint)(itemCount * (uint)sizeof(T)));
        _items = (T*)_buffer.ToPointer();
        _index = 0;
    }

    public int Allocate() {
        if (_index >= _itemCount) throw new InvalidOperationException("Buffer full");
        int currentIndex = _index++;
        _items[currentIndex] = default(T);
        return currentIndex;
    }

    public ref T GetItem(int index) {
        if (index < 0 || index >= _index) throw new ArgumentOutOfRangeException(nameof(index));
        return ref _items[index];
    }

    public void Reset() => _index = 0;

    public void Dispose() {
        if (_buffer != IntPtr.Zero) {
            NativeMemory.Free(_buffer.ToPointer());
        }
    }
}

public struct Node {
    public int LeftIndex;
    public int RightIndex;
}

public class BinaryTrees {
    public static void Run(int maxDepth) {
        int minDepth = 4;
        int nodeCount = 1 << (maxDepth + 1); // Estimate max nodes
        using var pool = new NativePool<Node>(nodeCount);
        using var longPool = new NativePool<Node>(nodeCount);

        var stretchTree = CreateTree(longPool, maxDepth + 1);
        Console.WriteLine(string.Concat("stretch tree of depth ", maxDepth + 1,
            "\t check: ", CheckTree(longPool, stretchTree)));

        longPool.Reset();
        var longLivedTree = CreateTree(longPool, maxDepth);

        for (int depth = minDepth; depth <= maxDepth; depth += 2) {
            int iterations = 1 << (maxDepth - depth + minDepth);
            long checkSum = 0;

            for (int i = 1; i <= iterations; i++) {
                pool.Reset();
                int root = CreateTree(pool, depth);
                checkSum += CheckTree(pool, root);
            }

            Console.WriteLine($"{iterations}\t trees of depth {depth}\t check: {checkSum}");
        }

        Console.WriteLine(string.Concat("long lived tree of depth ", maxDepth,
            "\t check: ", CheckTree(longPool, longLivedTree)));
    }

    private static int CreateTree(NativePool<Node> pool, int depth) {
        if (depth <= 0) return -1;
        int nodeIndex = pool.Allocate();
        ref Node current = ref pool.GetItem(nodeIndex);
        current.LeftIndex = CreateTree(pool, depth - 1);
        current.RightIndex = CreateTree(pool, depth - 1);
        return nodeIndex;
    }

    private static int CheckTree(NativePool<Node> pool, int nodeIndex) {
        if (nodeIndex == -1) return 1;
        ref Node node = ref pool.GetItem(nodeIndex);
        return 1 + CheckTree(pool, node.LeftIndex) + CheckTree(pool, node.RightIndex);
    }
}

[MemoryDiagnoser]
public class BinaryTreesBenchmark {
/*    [Benchmark(Baseline = true)]
    public void Default() => BinaryTreesDefault.Run(21);*/

    [Benchmark]
    public void Optimized() => BinaryTrees.Run(21);
}

class Program {
    static void Main() => BenchmarkRunner.Run<BinaryTreesBenchmark>();
}