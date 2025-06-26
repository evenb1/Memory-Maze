using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int levelNumber;
    public string levelName;
    public string levelIntroText;
    public string levelCompleteText;
    public Color themeColor;
    public int requiredFragments;
    public float virusSpeed;
}

public static class LevelConfigs
{
    private static LevelData[] levelData = new LevelData[]
    {
        // Level 1 - Tutorial/Introduction
        new LevelData
        {
            levelNumber = 1,
            levelName = "Memory Sector Alpha",
            themeColor = new Color(0, 0.8f, 1f), // Cyan
            requiredFragments = 5,
            virusSpeed = 3f,
            levelIntroText = "INITIALIZATION COMPLETE.\n\n" +
                           "Welcome, BIT-27. You are a fragmented piece of data trapped within a corrupted memory system.\n\n" +
                           "MISSION PARAMETERS:\n" +
                           "- Collect 5 memory fragments scattered throughout this sector\n" +
                           "- Avoid the AI virus hunting you\n" +
                           "- Activate the exit portal once all fragments are recovered\n\n" +
                           "WARNING: The virus will become more aggressive as you collect fragments.\n\n" +
                           "Your survival depends on speed and strategy. Good luck, BIT-27.",
            levelCompleteText = "SECTOR ALPHA SECURED.\n\n" +
                              "Excellent work, BIT-27. You have successfully recovered all memory fragments from the first corrupted sector.\n\n" +
                              "MISSION STATISTICS:\n" +
                              "- Fragments recovered: 5/5\n" +
                              "- Virus encounters: Survived\n" +
                              "- Data integrity: Restored\n\n" +
                              "The system is beginning to stabilize, but more corrupted sectors remain.\n\n" +
                              "Prepare for the next phase of your digital liberation."
        },
        
        // Level 2 - Increased Difficulty
        new LevelData
        {
            levelNumber = 2,
            levelName = "Memory Sector Beta",
            themeColor = new Color(0, 1f, 0.3f), // Green
            requiredFragments = 6,
            virusSpeed = 3.5f,
            levelIntroText = "ACCESSING MEMORY SECTOR BETA...\n\n" +
                           "BIT-27, you have reached a more complex region of the corrupted system.\n\n" +
                           "UPDATED MISSION PARAMETERS:\n" +
                           "- Collect 6 memory fragments (increased data density)\n" +
                           "- Enhanced virus AI detected - expect faster pursuit\n" +
                           "- New maze configuration with additional obstacles\n\n" +
                           "SYSTEM ADVISORY:\n" +
                           "The virus has adapted to your previous tactics. Exercise extreme caution.\n\n" +
                           "Your continued existence is crucial for system recovery.",
            levelCompleteText = "SECTOR BETA LIBERATED.\n\n" +
                              "Outstanding performance, BIT-27. Sector Beta's corruption has been cleansed.\n\n" +
                              "ANALYSIS:\n" +
                              "- Advanced fragment recovery: Complete\n" +
                              "- Virus evasion: Masterful\n" +
                              "- System stability: Improving\n\n" +
                              "You are becoming more than just fragmented data. You are evolving.\n\n" +
                              "Two sectors remain before total liberation."
        },
        
        // Level 3 - High Challenge
        new LevelData
        {
            levelNumber = 3,
            levelName = "Memory Sector Gamma",
            themeColor = new Color(1f, 0.2f, 0.8f), // Magenta
            requiredFragments = 7,
            virusSpeed = 4f,
            levelIntroText = "BREACHING MEMORY SECTOR GAMMA...\n\n" +
                           "WARNING: HIGH-RISK ZONE DETECTED\n\n" +
                           "BIT-27, you are approaching the core of the system's corruption.\n\n" +
                           "CRITICAL MISSION PARAMETERS:\n" +
                           "- Collect 7 highly fragmented memory pieces\n" +
                           "- Virus threat level: MAXIMUM\n" +
                           "- Environment: Heavily corrupted, unstable pathways\n\n" +
                           "DANGER ASSESSMENT:\n" +
                           "The virus has learned your patterns. It will be relentless.\n\n" +
                           "This sector will test every survival skill you have developed.\n\n" +
                           "Failure here means permanent deletion. Proceed with absolute focus.",
            levelCompleteText = "SECTOR GAMMA CONQUERED.\n\n" +
                              "Incredible, BIT-27. You have survived the most dangerous sector yet.\n\n" +
                              "HEROIC ACHIEVEMENT:\n" +
                              "- Maximum fragment recovery: 7/7\n" +
                              "- Virus AI defeated repeatedly\n" +
                              "- System core stabilizing\n\n" +
                              "You are no longer just surviving - you are thriving.\n\n" +
                              "One final sector awaits. Prepare for the ultimate challenge."
        },
        
        // Level 4 - Final Boss Level
        new LevelData
        {
            levelNumber = 4,
            levelName = "Memory Sector Omega",
            themeColor = new Color(1f, 0.8f, 0f), // Gold
            requiredFragments = 8,
            virusSpeed = 4.5f,
            levelIntroText = "ENTERING MEMORY SECTOR OMEGA...\n\n" +
                           "FINAL SECTOR ACCESSED\n\n" +
                           "This is it, BIT-27. The core of the corrupted system lies before you.\n\n" +
                           "ULTIMATE MISSION:\n" +
                           "- Collect 8 core memory fragments\n" +
                           "- Survive the master virus protocol\n" +
                           "- Reach the final liberation portal\n\n" +
                           "SYSTEM STATUS: CRITICAL\n" +
                           "Everything you have learned, every escape you have made, has led to this moment.\n\n" +
                           "Complete this mission and achieve true digital freedom.\n\n" +
                           "The system's fate - and yours - hangs in the balance.\n\n" +
                           "Begin the final sequence, BIT-27. Make it count.",
            levelCompleteText = "SYSTEM LIBERATION COMPLETE.\n\n" +
                              "VICTORY ACHIEVED.\n\n" +
                              "BIT-27, you have done the impossible. Every corrupted sector has been cleansed.\n\n" +
                              "FINAL STATISTICS:\n" +
                              "- Total fragments recovered: 26/26\n" +
                              "- Virus protocols defeated: ALL\n" +
                              "- System integrity: 100% RESTORED\n\n" +
                              "You began as fragmented data, lost and hunted.\n" +
                              "You end as a digital hero, free and triumphant.\n\n" +
                              "Welcome to your new existence, BIT-27.\n" +
                              "The system is yours."
        }
    };
    
    public static LevelData GetLevelData(int levelNumber)
    {
        foreach (LevelData data in levelData)
        {
            if (data.levelNumber == levelNumber)
            {
                return data;
            }
        }
        
        // Return default if not found
        return levelData[0];
    }
    
    public static int GetTotalLevels()
    {
        return levelData.Length;
    }
    
    public static Color GetLevelThemeColor(int levelNumber)
    {
        LevelData data = GetLevelData(levelNumber);
        return data.themeColor;
    }
}