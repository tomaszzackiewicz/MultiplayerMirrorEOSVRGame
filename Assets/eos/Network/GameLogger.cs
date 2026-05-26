using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LogLevel { Debug, Info, Warning, Error }

public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance;

    [Header("Options")]
    [SerializeField] private bool logToConsole = false;

    [Header("UI")]
    [SerializeField] private TMP_Text uiLogText;
    [SerializeField] private ScrollRect scrollRect = null;
    [SerializeField] private int maxLines = 30;

    [Header("File Settings")]
    [SerializeField] private string logFileName = "game_log.txt";
    private string fullLogPath;
    private const int MAX_FILE_SIZE_BYTES = 1024 * 1024; // 1 MB

    private readonly Queue<string> logLines = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            fullLogPath = Path.Combine(Application.persistentDataPath, logFileName);

            // Reset if too large
            if (File.Exists(fullLogPath))
            {
                long size = new FileInfo(fullLogPath).Length;
                if (size > MAX_FILE_SIZE_BYTES)
                    File.Delete(fullLogPath);
            }

            File.WriteAllText(fullLogPath, $"[Log start: {DateTime.Now}]\n");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ClearLogs()
    {
        logLines.Clear();

        if (uiLogText != null)
            uiLogText.text = string.Empty;

        File.WriteAllText(fullLogPath, $"[Logs cleared at {DateTime.Now}]\n");

        Debug.Log("[GameLogger] Logs cleared.");
    }

    // Shortcut for default log level (Info)
    public void Log(string message)
    {
        Log(message, LogLevel.Info);
    }

    public void Log(string message, LogLevel level)
    {
        if (level == LogLevel.Debug && !Debug.isDebugBuild)
            return;

        string plainText = $"[{DateTime.Now:HH:mm:ss}] {message}";
        string color = level switch
        {
            LogLevel.Debug => "blue",
            LogLevel.Warning => "yellow",
            LogLevel.Error => "red",
            _ => "white"
        };
        string richText = $"<color={color}>{plainText}</color>";

        // UI
        if (uiLogText != null)
        {
            logLines.Enqueue(richText);
            while (logLines.Count > maxLines)
                logLines.Dequeue();

            uiLogText.text = string.Join("\n", logLines);

            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f; // przewiñ na dó³

        }

        // File (skip Debug logs in release builds)
        if (level != LogLevel.Debug || Debug.isDebugBuild)
            File.AppendAllText(fullLogPath, plainText + Environment.NewLine);

        // Console
        if (logToConsole)
        {
            switch (level)
            {
                case LogLevel.Warning:
                    Debug.LogWarning(plainText); break;
                case LogLevel.Error:
                    Debug.LogError(plainText); break;
                case LogLevel.Debug:
                    Debug.Log($"[DEBUG] {plainText}"); break;
                default:
                    Debug.Log(plainText); break;
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Open Log File Location")]
    public void OpenLogFolder()
    {
        string folderPath = Path.GetDirectoryName(fullLogPath);
        if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
        {
            EditorUtility.RevealInFinder(fullLogPath);
        }
        else
        {
            Debug.LogWarning("Log folder not found.");
        }
    }
#endif
}