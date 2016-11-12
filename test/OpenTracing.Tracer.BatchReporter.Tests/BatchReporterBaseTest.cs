// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Xunit;

// namespace OpenTracing.Tracer.BatchedReporting.Tests
// {
//     public class BackoffTimerTest
//     {
//         private const double TickInterval = 25d;
//         private const int OneTick = 30;
//         private const int TwoTicks = 55;
//         private const int ThreeTicks = 85;

//         private BatchedReporterOptions GetOptions()
//         {
//             return new BatchedReporterOptions
//             {
//                 FlushInterval = TimeSpan.FromMilliseconds(TickInterval)
//             };
//         }

//         private BackoffTimer GetTimer(
//             BatchedReporterOptions options = null,
//             Func<TickContext, Task> onTick = null,
//             Action<Exception> onError = null)
//         {
//             options = options ?? GetOptions();
//             onTick = onTick ?? new Func<TickContext, Task>((_) => Task.FromResult(0));

//             return new BackoffTimer(options, onTick, onError);
//         }

//         [Fact]
//         public void NewInstance_IsRunning_is_false()
//         {
//             var timer = GetTimer();
//             Assert.False(timer.IsRunning);
//         }

//         [Fact]
//         public void NewInstance_IsShuttingDown_is_false()
//         {
//             var timer = GetTimer();
//             Assert.False(timer.IsShuttingDown);
//         }

//         [Fact]
//         public void Start_sets_IsRunning()
//         {
//             var timer = GetTimer();
//             timer.Start();
//             Assert.True(timer.IsRunning);
//         }

//         [Fact]
//         public void Stop_clears_IsRunning()
//         {
//             var timer = GetTimer();

//             timer.Start();
//             Assert.True(timer.IsRunning);

//             timer.Stop();
//             Assert.False(timer.IsRunning);
//         }

//         [Fact]
//         public async Task Stop_waits_for_tick_ThreadSleep()
//         {
//             // Arrange
//             var tickCount = 0;
//             var timer = GetTimer(onTick: _ => {
//                 Thread.Sleep(100);
//                 tickCount++;
//                 return Task.CompletedTask;
//             });

//             // Act
//             timer.Start();
//             await Task.Delay(20); // Give timer some time to start the tick
//             timer.Stop();

//             // Assert
//             Assert.Equal(1, tickCount);
//         }

//         [Fact]
//         public async Task Stop_waits_for_tick_CpuWork()
//         {
//             // Arrange
//             var tickCount = 0;
//             var timer = GetTimer(onTick: _ => {
//                 var future = DateTime.UtcNow.AddMilliseconds(100);
//                 while (DateTime.UtcNow < future)
//                 {
//                     /* this nonsense loop simulates cpu work */
//                 }
//                 tickCount++;
//                 return Task.CompletedTask;
//             });

//             // Act
//             timer.Start();
//             await Task.Delay(20); // Give timer some time to start the tick
//             timer.Stop();

//             // Assert
//             Assert.Equal(1, tickCount);
//         }

//         [Fact]
//         public async Task Stop_waits_for_tick_Await()
//         {
//             // Arrange
//             var tickCount = 0;
//             var timer = GetTimer(onTick: async _ => {
//                 await Task.Delay(100);
//                 tickCount++;
//             });

//             // Act
//             timer.Start();
//             await Task.Delay(20); // Give timer some time to start the tick
//             timer.Stop();

//             // Assert
//             Assert.Equal(1, tickCount);
//         }

//         [Fact]
//         public async Task Tick_completes_if_tick_uses_await()
//         {
//             // Arrange
//             var tickCount = 0;
//             var timer = GetTimer(onTick: async _ => {
//                 await Task.Delay(10);
//                 tickCount++;
//             });

//             // Act
//             timer.Start();
//             await Task.Delay(OneTick + 15 /* Tick-Delay */);

//             // Assert
//             Assert.Equal(1, tickCount);
//         }

//         [Fact]
//         public async Task Ticks_again_if_it_uses_await()
//         {
//             // Arrange
//             var tickCount = 0;
//             var timer = GetTimer(onTick: async _ => {
//                 await Task.Delay(10);
//                 tickCount++;
//             });

//             // Act
//             timer.Start();
//             await Task.Delay(TwoTicks + 25 /* Tick-Delay */);

//             // Assert
//             Assert.Equal(2, tickCount);
//         }

//         [Fact]
//         public async Task Ticks_again_after_error()
//         {
//             // Arrange
//             var tickCount = 0;
//             var errorCount = 0;

//             var onTick = new Func<TickContext, Task>(_ =>
//             {
//                 tickCount++;

//                 if (tickCount == 2)
//                     throw new Exception();

//                 return Task.FromResult(0);
//             });

//             var onError = new Action<Exception>(_ => { errorCount++; });

//             var timer = GetTimer(null, onTick, onError);

//             // Act
//             timer.Start();
//             await Task.Delay(ThreeTicks);

//             // Assert
//             Assert.Equal(3, tickCount);
//             Assert.Equal(1, errorCount);
//         }
//     }
// }