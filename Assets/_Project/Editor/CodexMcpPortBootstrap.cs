using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Subss.Editor
{
    [InitializeOnLoad]
    public static class CodexMcpPortBootstrap
    {
        private const int ProjectPort = 7894;
        private const string ExpectedProjectFolderName = "subss";
        private const string PortKey = "UnityMCP_Port";
        private const string UseManualPortKey = "UnityMCP_UseManualPort";

        static CodexMcpPortBootstrap()
        {
            EditorApplication.delayCall += ConfigureProjectMcpPort;
        }

        private static void ConfigureProjectMcpPort()
        {
            if (!IsExpectedProject())
            {
                return;
            }

            bool needsRestart = EditorPrefs.GetInt(PortKey, 7890) != ProjectPort ||
                                !EditorPrefs.GetBool(UseManualPortKey, false);

            EditorPrefs.SetBool(UseManualPortKey, true);
            EditorPrefs.SetInt(PortKey, ProjectPort);

            if (!needsRestart)
            {
                return;
            }

            RestartAnkleBreakerBridge();
        }

        private static bool IsExpectedProject()
        {
            string projectPath = Directory.GetParent(Application.dataPath)?.FullName;
            string folderName = Path.GetFileName(projectPath);
            return string.Equals(folderName, ExpectedProjectFolderName, StringComparison.OrdinalIgnoreCase);
        }

        private static void RestartAnkleBreakerBridge()
        {
            Type bridgeType = FindType("UnityMCP.Editor.MCPBridgeServer");
            if (bridgeType == null)
            {
                Debug.Log("[Codex MCP] AnkleBreaker bridge type was not found yet. Port preference is saved for next editor start.");
                return;
            }

            MethodInfo stop = bridgeType.GetMethod("Stop", BindingFlags.Public | BindingFlags.Static);
            MethodInfo start = bridgeType.GetMethod("Start", BindingFlags.Public | BindingFlags.Static);

            stop?.Invoke(null, null);
            EditorApplication.delayCall += () =>
            {
                start?.Invoke(null, null);
                Debug.Log($"[Codex MCP] Project MCP bridge is configured for port {ProjectPort}.");
            };
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
