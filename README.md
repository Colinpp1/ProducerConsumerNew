# Producer-Consumer Pattern

A multi-threaded C# program demonstrating the Producer-Consumer pattern with thread synchronization.

## What This Program Does

- **3 Producer threads** generate random numbers and add them to a shared queue
- **2 Consumer threads** read and process numbers from the queue
- Uses `Monitor`, `lock`, and `Interlocked` for thread-safe operations
- Queue has a maximum capacity of 10 items
- Producers wait when queue is full, consumers wait when queue is empty

## How to Run

### Command Line
```bash
cd C:\Users\kenzo\CascadeProjects\ProducerConsumerNew
dotnet run
```

### Visual Studio
1. Open `ProducerConsumerNew.sln`
2. Press `F5` to run

## How to Run Tests

### Command Line
```bash
dotnet test
```

### Visual Studio
1. Open **Test Explorer** (`Test` → `Test Explorer`)
2. Click **Run All**

**Expected Result:** 5 tests pass ✅
