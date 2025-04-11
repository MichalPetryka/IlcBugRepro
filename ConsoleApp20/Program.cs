using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Running;
using TerraFX.Interop.Mimalloc;

namespace ConsoleApp20;

internal class Program
{
    static void Main()
    {
        BenchmarkRunner.Run<Benchmark>();
    }
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.NativeAot90)]
public unsafe class Benchmark
{
    [Params(4, 16, 512, 1024, 4096, 65536)]
    public int Size;

    [Benchmark]
    public void Array()
    {
        Use(new byte[Size]);
    }

    [Benchmark]
    public void AllocArray()
    {
        Use(GC.AllocateArray<byte>(Size));
    }

    [Benchmark]
    public void AllocArrayUninitialized()
    {
        Use(GC.AllocateUninitializedArray<byte>(Size));
    }

    [Benchmark]
    public void ArrayPool()
    {
        byte[] arr = ArrayPool<byte>.Shared.Rent(Size);
        Use(arr);
        ArrayPool<byte>.Shared.Return(arr);
    }

    [Benchmark]
    public void Stackalloc()
    {
        Use(stackalloc byte[Size]);
    }

    [Benchmark]
    [SkipLocalsInit]
    public void StackallocSkipLocalsInit()
    {
        Use(stackalloc byte[Size]);
    }

    [Benchmark]
    public void Malloc()
    {
        byte* ptr = (byte*)NativeMemory.Alloc((uint)Size);
        Use(new Span<byte>(ptr, Size));
        NativeMemory.Free(ptr);
    }

    [Benchmark]
    public void Calloc()
    {
        byte* ptr = (byte*)NativeMemory.AllocZeroed((uint)Size);
        Use(new Span<byte>(ptr, Size));
        NativeMemory.Free(ptr);
    }

    [Benchmark]
    public void AllocAligned()
    {
        byte* ptr = (byte*)NativeMemory.AlignedAlloc((uint)Size, 64);
        Use(new Span<byte>(ptr, Size));
        NativeMemory.AlignedFree(ptr);
    }

    [Benchmark]
    public void CoTaskMem()
    {
        byte* ptr = (byte*)Marshal.AllocCoTaskMem(Size);
        Use(new Span<byte>(ptr, Size));
        Marshal.FreeCoTaskMem((nint)ptr);
    }

    [Benchmark]
    public void HGlobal()
    {
        byte* ptr = (byte*)Marshal.AllocHGlobal(Size);
        Use(new Span<byte>(ptr, Size));
        Marshal.FreeHGlobal((nint)ptr);
    }

    [Benchmark]
    public void ManagedMimalloc()
    {
        byte* ptr = (byte*)Mimalloc.mi_malloc((uint)Size);
        Use(new Span<byte>(ptr, Size));
        Mimalloc.mi_free(ptr);
    }

    [Benchmark]
    public void ManagedMicalloc()
    {
        byte* ptr = (byte*)Mimalloc.mi_calloc((uint)Size, 1);
        Use(new Span<byte>(ptr, Size));
        Mimalloc.mi_free(ptr);
    }

    [Benchmark]
    public void ManagedMimallocAligned()
    {
        byte* ptr = (byte*)Mimalloc.mi_aligned_alloc(64, (uint)Size);
        Use(new Span<byte>(ptr, Size));
        Mimalloc.mi_free_aligned(ptr, 64);
    }

    [Benchmark]
    public void Mimalloc1()
    {
        byte* ptr = (byte*)MiAlloc1((uint)Size);
        Use(new Span<byte>(ptr, Size));
        MiFree1(ptr);
    }

    [Benchmark]
    public void Micalloc1()
    {
        byte* ptr = (byte*)MiCalloc1((uint)Size, 1);
        Use(new Span<byte>(ptr, Size));
        MiFree1(ptr);
    }

    [Benchmark]
    public void MimallocAligned1()
    {
        byte* ptr = (byte*)MiAlignedAlloc1((uint)Size, 64);
        Use(new Span<byte>(ptr, Size));
        MiAlignedFree1(ptr, 64);
    }

    [Benchmark]
    public void Mimalloc2()
    {
        byte* ptr = (byte*)MiAlloc2((uint)Size);
        Use(new Span<byte>(ptr, Size));
        MiFree2(ptr);
    }

    [Benchmark]
    public void Micalloc2()
    {
        byte* ptr = (byte*)MiCalloc2((uint)Size, 1);
        Use(new Span<byte>(ptr, Size));
        MiFree2(ptr);
    }

    [Benchmark]
    public void MimallocAligned2()
    {
        byte* ptr = (byte*)MiAlignedAlloc2((uint)Size, 64);
        Use(new Span<byte>(ptr, Size));
        MiAlignedFree2(ptr, 64);
    }

    [Benchmark]
    public void Mimalloc3()
    {
        byte* ptr = (byte*)MiAlloc3((uint)Size);
        Use(new Span<byte>(ptr, Size));
        MiFree3(ptr);
    }

    [Benchmark]
    public void Micalloc3()
    {
        byte* ptr = (byte*)MiCalloc3((uint)Size, 1);
        Use(new Span<byte>(ptr, Size));
        MiFree3(ptr);
    }

    [Benchmark]
    public void MimallocAligned3()
    {
        byte* ptr = (byte*)MiAlignedAlloc3((uint)Size, 64);
        Use(new Span<byte>(ptr, Size));
        MiAlignedFree3(ptr, 64);
    }

    [DllImport("mimalloc1", EntryPoint = "mi_malloc")]
    public static extern void* MiAlloc1(nuint size);

    [DllImport("mimalloc1", EntryPoint = "mi_calloc")]
    public static extern void* MiCalloc1(nuint count, nuint size);

    [DllImport("mimalloc1", EntryPoint = "mi_free")]
    public static extern void MiFree1(void* ptr);

    [DllImport("mimalloc1", EntryPoint = "mi_malloc_aligned")]
    public static extern void* MiAlignedAlloc1(nuint size, nuint alignment);

    [DllImport("mimalloc1", EntryPoint = "mi_free_aligned")]
    public static extern void MiAlignedFree1(void* ptr, nuint alignment);

    [DllImport("mimalloc2", EntryPoint = "mi_malloc")]
    public static extern void* MiAlloc2(nuint size);

    [DllImport("mimalloc2", EntryPoint = "mi_calloc")]
    public static extern void* MiCalloc2(nuint count, nuint size);

    [DllImport("mimalloc2", EntryPoint = "mi_free")]
    public static extern void MiFree2(void* ptr);

    [DllImport("mimalloc2", EntryPoint = "mi_malloc_aligned")]
    public static extern void* MiAlignedAlloc2(nuint size, nuint alignment);

    [DllImport("mimalloc2", EntryPoint = "mi_free_aligned")]
    public static extern void MiAlignedFree2(void* ptr, nuint alignment);

    [DllImport("mimalloc3", EntryPoint = "mi_malloc")]
    public static extern void* MiAlloc3(nuint size);

    [DllImport("mimalloc3", EntryPoint = "mi_calloc")]
    public static extern void* MiCalloc3(nuint count, nuint size);

    [DllImport("mimalloc3", EntryPoint = "mi_free")]
    public static extern void MiFree3(void* ptr);

    [DllImport("mimalloc3", EntryPoint = "mi_malloc_aligned")]
    public static extern void* MiAlignedAlloc3(nuint size, nuint alignment);

    [DllImport("mimalloc3", EntryPoint = "mi_free_aligned")]
    public static extern void MiAlignedFree3(void* ptr, nuint alignment);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Use(Span<byte> span) {}
}