﻿using System;
using UnityEngine;

public class GitBashCommandBuilder : ICommandBuilder
{
    public string GetMergeBase(string targetBranch, string sourceBranch)
    {
        //space after command set in initialization
        return GitCommands.merge_base + targetBranch + " " + sourceBranch;
    }

    public string GetRevParse(string branch)
    {
        //space after command set in initialization
        return GitCommands.rev_parse + branch;
    }

    /// <summary>
    /// Shortens the lenght of hashes to 12 characters.
    /// Full 40 character hashes lead to wrong execution.
    /// </summary>
    /// <param name="mergeBase"></param>
    /// <param name="revParse"></param>
    /// <returns></returns>
    public string GetDiff(string mergeBase, string revParse)
    {
        mergeBase = mergeBase.Remove(mergeBase.Length - 28, 28);
        revParse = revParse.Remove(revParse.Length - 28, 28);
        //TODO: rn hard coded path, maybe bring to configs
        return GitCommands.diff + mergeBase + " " + revParse + " Assets/Scenes";
    }

    public string GetCommit(string contentPath)
    {
        //TODO: include .meta file
        return GitCommands.add + contentPath
                               + " && " + GitCommands.commit_m
                               + $" \' added {contentPath} on {DateTime.Now} \'";
    }

    public string GetPush(string branchName)
    {
        return GitCommands.push + branchName;
    }

    public string GetSwitch(string branch)
    {
        return GitCommands.g_switch + branch;
    }

    /// <summary>
    /// Merges using the -Xours strategy
    /// Uses the --no-edit flag (default commit message)
    /// </summary>
    /// <param name="sourceBranch"></param>
    /// <returns></returns>
    public string GetMergeXours(string sourceBranch)
    {
        return GitCommands.merge_Xours + sourceBranch + GitFlags.no_edit;
    }

    public string GetMkdir(string path)
    {
        return BashCommands.mkdir + path;
    }

    public string GetTouch(string path, string fileNameWithType)
    {
        return BashCommands.touch + path + fileNameWithType;
    }

    public string GetSubtreeSplitNewBranch(string prefix, string newBranchName)
    {
        return GitCommands.subtree_split_prefix + prefix + GitFlags.branch + newBranchName;
    }
}