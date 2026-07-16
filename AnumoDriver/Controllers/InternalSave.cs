using System.IO;
using System.Text.Json;
using Timer = System.Timers.Timer;

namespace AnumoDriver.Controllers;

public class InternalSave
{
    private readonly Timer _saveDebounceTimer;
    private readonly object _fileLock = new();
    private readonly JsonSerializerOptions _jsonOption = new() { WriteIndented = true };
    private readonly string _fileName;

    private class InternalSaveData()
    {
        public string SG { get; set; } = "";
        public string SFP { get; set; } = "";
        public int WPX { get; set; } = 52;
        public int WPY { get; set; } = 52;
        public bool WPIN { get; set; } = false;
        public bool WTOP { get; set; } = true;
    }
    private InternalSaveData _saveData = new();

    public string CurrentSerialGate
    {
        get => _saveData.SG;
        set { _saveData.SG = value; SaveRequest(); }
    }
    public string CurrentSetFilePath
    {
        get => _saveData.SFP;
        set { _saveData.SFP = value; SaveRequest(); }
    }
    public int MainWindowPosX
    {
        get => _saveData.WPX;
        set { _saveData.WPX = value; SaveRequest(); }
    }
    public int MainWindowPosY
    {
        get => _saveData.WPY;
        set { _saveData.WPY = value; SaveRequest(); }
    }
    public bool MainWindowPinned
    {
        get => _saveData.WPIN;
        set { _saveData.WPIN = value; SaveRequest(); }
    }
    public bool MainWindowTopmost
    {
        get => _saveData.WTOP;
        set { _saveData.WTOP = value; SaveRequest(); }
    }

    public InternalSave()
    {
        _fileName = Path.Combine(App.LocalDataFolder, "internal");

        //Load saved file
        if (File.Exists(_fileName))
        {
            var raw = File.ReadAllText(_fileName);

            if (!string.IsNullOrEmpty(raw))
                _saveData = JsonSerializer.Deserialize<InternalSaveData>(raw) ?? new();
            else
                File.CreateText(_fileName);
        }
        else
            File.CreateText(_fileName);


        // Debounce: Wait 500ms after the last property change before writing to disk
        _saveDebounceTimer = new(500) { AutoReset = false };
        _saveDebounceTimer.Elapsed += (s, e) => CommitToDisk();
    }

    /***************************************************************/

    private void SaveRequest()
    {
        _saveDebounceTimer.Stop();
        _saveDebounceTimer.Start();
    }
    private void CommitToDisk()
    {
        lock (_fileLock) // Prevent multiple threads from writing at the same time
        {
            try
            {
                string tempPath = _fileName + "tmp";

                // 1. Serialize to a string
                string json = JsonSerializer.Serialize(_saveData, _jsonOption);

                // 2. Write to a TEMPORARY file first (Atomic Write Pattern)
                File.WriteAllText(tempPath, json);

                // 3. Replace the old file with the new one
                // This prevents file corruption if the app crashes mid-save
                if (File.Exists(_fileName))
                    File.Delete(_fileName);
                File.Move(tempPath, _fileName);
            }
            catch (Exception ex)
            {
                // Handle or Log I/O errors
                System.Diagnostics.Debug.WriteLine($"Save failed: {ex.Message}");
            }
        }
    }
}

