﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class GitUnMain
{
    private static IGitDiffReader _diffReader;
    private static PrefabSaver _saver;
    private static ITerminalInterface _terminal;
    private static ICommandBuilder _commandBuilder;
    private static FileLocking _fileLocking;

    static GitUnMain()
    {
        _diffReader = new DiffGameObjectExtractor();
        _saver = new PrefabSaver();
        _terminal = new GitBashInterface();
        _commandBuilder = new GitBashCommandBuilder();

        _fileLocking = new FileLocking(_terminal, _commandBuilder);

        GitUnGUI.OnStartSceneGet += Main;
        GitUnGUI.InitGitDataObject += GetGitData;
        GitUnGUI.OnSetupGitUn += SetupGitUn;
        GitUnGUI.OnLockFile += _fileLocking.LockFile;
    }

    private static void SetupGitUn()
    {
        string initCmd = _commandBuilder.GetInit();
        _terminal.Execute(initCmd);
        
        string touchCmd = _commandBuilder.GetTouch("", GlobalRefs.gitignore);
        _terminal.Execute(touchCmd);

        string gitignoreContentCmd = _commandBuilder.GetNewestGitignoreContent();
        _terminal.Execute(gitignoreContentCmd);

        string addToGitignoreCmd = _commandBuilder.GetWriteLinesToFile(
            new[] { GlobalRefs.filePaths.logsFile, GlobalRefs.filePaths.lockedProtocolFile }, GlobalRefs.gitignore);
        _terminal.Execute(addToGitignoreCmd);
        
        string commitCmd = _commandBuilder.GetCommit(GlobalRefs.gitignore);
        _terminal.Execute(commitCmd);
        
        string branchCmd = _commandBuilder.GetCreateBranch(GlobalRefs.lockingBranch);
        _terminal.Execute(branchCmd);
        
        string switchCmd = _commandBuilder.GetSwitch(GlobalRefs.lockingBranch);
        _terminal.Execute(switchCmd);

        string newIgnore = _commandBuilder.GetOverrideFileContent
           ($"'*\\n!{GlobalRefs.filePaths.lockedProtocolFile}'",GlobalRefs.gitignore);
        _terminal.Execute(newIgnore);
        // Again commiting gitignore so can be reused.
        _terminal.Execute(commitCmd);
        
        //TODO: rm the hardcoded master 
        string switch2Cmd = _commandBuilder.GetSwitch("master");
        _terminal.Execute(switch2Cmd);
        
        string commit2Cmd = _commandBuilder.GetCommit(".");
        _terminal.Execute(commit2Cmd);

        string addRemoteCmd = _commandBuilder.GetAddRemote();
        _terminal.Execute(addRemoteCmd);

        string pushAllCmd = _commandBuilder.GetPushAllBranches();
        _terminal.Execute(pushAllCmd);
        
        LogSystem.WriteLog(new []
        {
            initCmd, touchCmd, gitignoreContentCmd, addToGitignoreCmd, commitCmd, branchCmd, switch2Cmd,
            newIgnore, commitCmd, switch2Cmd, commit2Cmd, addRemoteCmd, pushAllCmd
        });
        
    }

    private static void GetGitData()
    {
        var branches = _terminal.ExecuteResultToStringArr(GitCommands.branch);
        GitUnGUI.InitGitDataObj(branches);
    }

    private static void Main(string targetBranch, string sourceBranch)
    {
        WriteRelevantDiffToTxt(targetBranch, sourceBranch);
        SaveDiffObjectsAsPrefab(_diffReader.GetDiffObjects());
        PseudoMerge(targetBranch, sourceBranch);
    }


    private static void PseudoMerge(string targetBranch, string sourceBranch)
    {
        string commitCommand = _commandBuilder.GetCommit(GlobalRefs.filePaths.DiffPrefabsDirectory);
        _terminal.Execute(commitCommand);

        string switchCommand = _commandBuilder.GetSwitch(targetBranch);
        _terminal.Execute(switchCommand);

        string mergeCommand = _commandBuilder.GetMergeXours(sourceBranch);
        _terminal.Execute(mergeCommand);

        LogSystem.WriteLog(new string[] { commitCommand, switchCommand, mergeCommand });
    }

    private static void WriteRelevantDiffToTxt(string targetBranch, string sourceBranch)
    {
        string mergeBaseCommand = _commandBuilder.GetMergeBase(targetBranch, sourceBranch);
        string mergeBaseResult = _terminal.ExecuteResultToString(mergeBaseCommand);

        string revParseCommand = _commandBuilder.GetRevParse(sourceBranch);
        string revParseResult = _terminal.ExecuteResultToString(revParseCommand);


        string diffCommand = _commandBuilder.GetDiff(mergeBaseResult, revParseResult);
        _terminal.ExecuteResultToTxt(diffCommand);

        LogSystem.WriteLog(new string[]
            { mergeBaseCommand, mergeBaseResult, revParseCommand, revParseResult, diffCommand });
    }

    private static void SaveDiffObjectsAsPrefab(IList<GameObject> diffGaObjects)
    {
        foreach (var go in diffGaObjects)
        {
            _saver.CreatePrefab(go);
        }
    }
}