using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using System;
using System.Text;

/// <summary>
/// Written by tomkail
/// </summary>
public static class PlayerLoopUtils
{
    public static void PrintPlayerLoop(PlayerLoopSystem def)
    {
        var sb = new StringBuilder();
        RecursivePlayerLoopPrint(def, sb, 0);
        Debug.Log(sb.ToString());
    }

    private static void RecursivePlayerLoopPrint(PlayerLoopSystem def, StringBuilder sb, int depth)
    {
        if (depth == 0)
            sb.AppendLine("ROOT NODE");
        else if (def.type != null)
        {
            for (int i = 0; i < depth; i++)
                sb.Append("\t");
            sb.AppendLine(def.type.Name);
        }
        if (def.subSystemList != null)
        {
            depth++;
            foreach (var s in def.subSystemList)
                RecursivePlayerLoopPrint(s, sb, depth);
            depth--;
        }
    }

    public enum AddMode
    {
        Beginning,
        End
    }

    // Add a new PlayerLoopSystem to the PlayerLoop. Example:
    // PlayerLoopSystem playerLoop = PlayerLoop.GetDefaultPlayerLoop();
    // Debug.Assert(PlayerLoopUtils.AddToPlayerLoop(CustomUpdate, typeof(LightgunInput), ref playerLoop, typeof(PreUpdate.NewInputUpdate), PlayerLoopUtils.AddMode.End));
    // PlayerLoop.SetPlayerLoop(playerLoop);
    public static bool AddToPlayerLoop(PlayerLoopSystem.UpdateFunction function, Type ownerType, ref PlayerLoopSystem playerLoop, Type playerLoopSystemType, AddMode addMode)
    {
        // did we find the type? e.g. EarlyUpdate/PreLateUpdate/etc.
        if (playerLoop.type == playerLoopSystemType)
        {
            // debugging
            //Debug.Log($"Found playerLoop of type {playerLoop.type} with {playerLoop.subSystemList.Length} Functions:");
            //foreach (PlayerLoopSystem sys in playerLoop.subSystemList)
            //    Debug.Log($"  ->{sys.type}");

            // resize & expand subSystemList to fit one more entry
            int oldListLength = (playerLoop.subSystemList != null) ? playerLoop.subSystemList.Length : 0;
            Array.Resize(ref playerLoop.subSystemList, oldListLength + 1);

            // prepend our custom loop to the beginning
            if (addMode == AddMode.Beginning)
            {
                // shift to the right, write into first array element
                Array.Copy(playerLoop.subSystemList, 0, playerLoop.subSystemList, 1, playerLoop.subSystemList.Length - 1);
                playerLoop.subSystemList[0].type = ownerType;
                playerLoop.subSystemList[0].updateDelegate = function;

            }
            // append our custom loop to the end
            else if (addMode == AddMode.End)
            {
                // simply write into last array element
                playerLoop.subSystemList[oldListLength].type = ownerType;
                playerLoop.subSystemList[oldListLength].updateDelegate = function;
            }

            // debugging
            //Debug.Log($"New playerLoop of type {playerLoop.type} with {playerLoop.subSystemList.Length} Functions:");
            //foreach (PlayerLoopSystem sys in playerLoop.subSystemList)
            //    Debug.Log($"  ->{sys.type}");

            return true;
        }

        // recursively keep looking
        if (playerLoop.subSystemList != null)
        {
            for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                if (AddToPlayerLoop(function, ownerType, ref playerLoop.subSystemList[i], playerLoopSystemType, addMode))
                    return true;
            }
        }
        return false;
    }
}