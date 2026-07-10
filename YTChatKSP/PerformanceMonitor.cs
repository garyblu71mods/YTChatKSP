using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Performance monitor - śledzi FPS, GC allocations, i czas renderowania
/// Loguje do pliku dla analizy
/// </summary>
public class PerformanceMonitor
{
    private static PerformanceMonitor instance;
    private StreamWriter logFile;
    private float frameTimeTotal = 0f;
    private int frameCount = 0;
    private float lastFpsUpdateTime = 0f;
    private float currentFps = 60f;
    private long lastGCMemory = 0;
    private long gcAllocated = 0;
    private Stopwatch frameWatch = new Stopwatch();

    public static PerformanceMonitor Instance
    {
        get
        {
            if (instance == null)
                instance = new PerformanceMonitor();
            return instance;
        }
    }

    public PerformanceMonitor()
    {
        try
        {
            string logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "YTChatKSP_Performance.log"
            );
            logFile = new StreamWriter(logPath, true) { AutoFlush = false }; // Disable AutoFlush for better performance
            LogLine($"=== Performance Monitor Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
        }
        catch (Exception ex)
        {
            Debug.LogError("[PerformanceMonitor] Failed to create log: " + ex.Message);
        }
    }

    public void OnFrameBegin()
    {
        frameWatch.Restart();
        // Don't query GC every frame - too expensive, only do it on logging tick
    }

    public void OnFrameEnd()
    {
        frameWatch.Stop();

        // Update FPS every 1 second
        frameCount++;
        frameTimeTotal += frameWatch.ElapsedMilliseconds;

        if (Time.realtimeSinceStartup - lastFpsUpdateTime >= 1.0f)
        {
            // Get memory info only once per second (expensive operation)
            long currentMemory = GC.GetTotalMemory(false);
            gcAllocated = currentMemory - lastGCMemory;
            lastGCMemory = currentMemory;

            currentFps = 1f / (frameTimeTotal / frameCount / 1000f);

            // Log every second
            LogLine($"FPS: {currentFps:F1} | Frame Time: {frameTimeTotal/frameCount:F2}ms | GC Alloc: {gcAllocated/1024}KB | Memory: {currentMemory/1024/1024}MB");

            frameTimeTotal = 0f;
            frameCount = 0;
            lastFpsUpdateTime = Time.realtimeSinceStartup;
        }
    }

    public float GetCurrentFPS()
    {
        return currentFps;
    }

    public void LogLine(string message)
    {
        try
        {
            if (logFile != null)
            {
                logFile.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
            }
            Debug.Log("[PERF] " + message);
        }
        catch { }
    }

    public void Close()
    {
        try
        {
            if (logFile != null)
            {
                logFile.Close();
                logFile = null;
            }
        }
        catch { }
    }

    ~PerformanceMonitor()
    {
        Close();
    }
}
